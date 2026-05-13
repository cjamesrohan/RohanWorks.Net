using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;

namespace RohanWorks.Net.Testing;

public static class LoggerMockExtensions
{
    public static LoggerAssertions<T> Should<T>(this Mock<ILogger<T>> instance)
        => new(instance);
}

public sealed class LoggerAssertions<T>
{
    private readonly Mock<ILogger<T>> _instance;

    public LoggerAssertions(Mock<ILogger<T>> instance) => _instance = instance;

    public LoggerNegationAssertions<T> Not => new(_instance);

    #region LogDebug
    public LoggerAssertionChain<T> LogDebug(string message, params object[] args) => Log(LogLevel.Debug, message, args);
    public LoggerAssertionChain<T> LogDebug(EventId eventId, string message, params object[] args) => Log(LogLevel.Debug, eventId, message, args);
    public LoggerAssertionChain<T> LogDebug(Exception exception, string message, params object[] args) => Log(LogLevel.Debug, exception, message, args);
    public LoggerAssertionChain<T> LogDebug(EventId eventId, Exception? exception, string message, params object[] args) => Log(LogLevel.Debug, eventId, exception, message, args);
    #endregion

    #region LogTrace
    public LoggerAssertionChain<T> LogTrace(string message, params object[] args) => Log(LogLevel.Trace, message, args);
    public LoggerAssertionChain<T> LogTrace(EventId eventId, string message, params object[] args) => Log(LogLevel.Trace, eventId, message, args);
    public LoggerAssertionChain<T> LogTrace(Exception exception, string message, params object[] args) => Log(LogLevel.Trace, exception, message, args);
    public LoggerAssertionChain<T> LogTrace(EventId eventId, Exception? exception, string message, params object[] args) => Log(LogLevel.Trace, eventId, exception, message, args);
    #endregion

    #region LogInformation
    public LoggerAssertionChain<T> LogInformation(string message, params object[] args) => Log(LogLevel.Information, message, args);
    public LoggerAssertionChain<T> LogInformation(EventId eventId, string message, params object[] args) => Log(LogLevel.Information, eventId, message, args);
    public LoggerAssertionChain<T> LogInformation(Exception exception, string message, params object[] args) => Log(LogLevel.Information, exception, message, args);
    public LoggerAssertionChain<T> LogInformation(EventId eventId, Exception? exception, string message, params object[] args) => Log(LogLevel.Information, eventId, exception, message, args);
    #endregion

    #region LogWarning
    public LoggerAssertionChain<T> LogWarning(string message, params object[] args) => Log(LogLevel.Warning, message, args);
    public LoggerAssertionChain<T> LogWarning(EventId eventId, string message, params object[] args) => Log(LogLevel.Warning, eventId, message, args);
    public LoggerAssertionChain<T> LogWarning(Exception exception, string message, params object[] args) => Log(LogLevel.Warning, exception, message, args);
    public LoggerAssertionChain<T> LogWarning(EventId eventId, Exception? exception, string message, params object[] args) => Log(LogLevel.Warning, eventId, exception, message, args);
    #endregion

    #region LogError
    public LoggerAssertionChain<T> LogError(string message, params object[] args) => Log(LogLevel.Error, message, args);
    public LoggerAssertionChain<T> LogError(EventId eventId, string message, params object[] args) => Log(LogLevel.Error, eventId, message, args);
    public LoggerAssertionChain<T> LogError(Exception exception, string message, params object[] args) => Log(LogLevel.Error, exception, message, args);
    public LoggerAssertionChain<T> LogError(EventId eventId, Exception? exception, string message, params object[] args) => Log(LogLevel.Error, eventId, exception, message, args);
    #endregion

    #region LogCritical
    public LoggerAssertionChain<T> LogCritical(string message, params object[] args) => Log(LogLevel.Critical, message, args);
    public LoggerAssertionChain<T> LogCritical(EventId eventId, string message, params object[] args) => Log(LogLevel.Critical, eventId, message, args);
    public LoggerAssertionChain<T> LogCritical(Exception exception, string message, params object[] args) => Log(LogLevel.Critical, exception, message, args);
    public LoggerAssertionChain<T> LogCritical(EventId eventId, Exception? exception, string message, params object[] args) => Log(LogLevel.Critical, eventId, exception, message, args);
    #endregion

    #region Log
    public LoggerAssertionChain<T> Log(LogLevel logLevel, string message, params object[] args) => Verify(logLevel, 0, null, message, args);
    public LoggerAssertionChain<T> Log(LogLevel logLevel, EventId eventId, string message, params object[] args) => Verify(logLevel, eventId, null, message, args);
    public LoggerAssertionChain<T> Log(LogLevel logLevel, Exception exception, string message, params object[] args) => Verify(logLevel, 0, exception, message, args);
    public LoggerAssertionChain<T> Log(LogLevel logLevel, EventId eventId, Exception? exception, string message, params object[] args) => Verify(logLevel, eventId, exception, message, args);
    #endregion

    public void HaveScope<TState>(TState state) where TState : notnull
        => _instance.Verify(x => x.BeginScope(state), "Missing or mismatched scope");

    private LoggerAssertionChain<T> Verify(LogLevel logLevel, EventId eventId, Exception? exception, string message, object[] args)
    {
        var formattedMessage = new FormattedLogValues(message, args).ToString();

        _instance.Verify(
            x => x.Log(
                logLevel,
                eventId,
                It.Is<It.IsAnyType>((v, _) => v.ToString() == formattedMessage),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, _) => true)),
            Times.AtLeastOnce(),
            $"Expected log at least once: Log(LogLevel.{logLevel}, {eventId}, {formattedMessage}, {exception?.Message ?? "null"})");

        return new LoggerAssertionChain<T>(_instance, logLevel, eventId, exception, formattedMessage);
    }
}

internal class FormattedLogValues : IReadOnlyList<KeyValuePair<string, object>>
{
    internal const int MaxCachedFormatters = 1024;
    private const string NullFormat = "[null]";
    private static int _count;
    private static readonly ConcurrentDictionary<string, LogValuesFormatter> Formatters = new();
    private readonly LogValuesFormatter? _formatter;
    private readonly object[] _values;
    private readonly string _originalMessage;

    internal LogValuesFormatter? Formatter => _formatter;

    public FormattedLogValues(string format, params object[]? values)
    {
        if (values?.Length > 0 && format != null)
        {
            if (_count >= MaxCachedFormatters)
            {
                if (!Formatters.TryGetValue(format, out _formatter))
                    _formatter = new LogValuesFormatter(format);
            }
            else
            {
                _formatter = Formatters.GetOrAdd(format, f =>
                {
                    Interlocked.Increment(ref _count);
                    return new LogValuesFormatter(f);
                });
            }
        }

        _originalMessage = format ?? NullFormat;
        _values = values ?? Array.Empty<object>();
    }

    public KeyValuePair<string, object> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException(nameof(index));

            if (index == Count - 1)
                return new KeyValuePair<string, object>("{OriginalFormat}", _originalMessage);

            return _formatter!.GetValue(_values, index);
        }
    }

    public int Count => _formatter is null ? 1 : _formatter.ValueNames.Count + 1;

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return this[i];
    }

    public override string ToString()
        => _formatter is null ? _originalMessage : _formatter.Format(_values);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class LogValuesFormatter
{
    private const string NullValue = "(null)";
    private static readonly object[] EmptyArray = Array.Empty<object>();
    private static readonly char[] FormatDelimiters = { ',', ':' };
    private readonly string _format;
    private readonly List<string> _valueNames = new();

    public LogValuesFormatter(string format)
    {
        OriginalFormat = format;

        var sb = new StringBuilder();
        var scanIndex = 0;
        var endIndex = format.Length;

        while (scanIndex < endIndex)
        {
            var openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
            var closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);
            var formatDelimiterIndex = FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

            if (closeBraceIndex == endIndex)
            {
                sb.Append(format, scanIndex, endIndex - scanIndex);
                scanIndex = endIndex;
            }
            else
            {
                sb.Append(format, scanIndex, openBraceIndex - scanIndex + 1);
                sb.Append(_valueNames.Count.ToString(CultureInfo.InvariantCulture));
                _valueNames.Add(format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
                sb.Append(format, formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1);
                scanIndex = closeBraceIndex + 1;
            }
        }

        _format = sb.ToString();
    }

    public string OriginalFormat { get; }
    public List<string> ValueNames => _valueNames;

    private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
    {
        var braceIndex = endIndex;
        var scanIndex = startIndex;
        var braceOccurrenceCount = 0;

        while (scanIndex < endIndex)
        {
            if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
            {
                if (braceOccurrenceCount % 2 == 0)
                {
                    braceOccurrenceCount = 0;
                    braceIndex = endIndex;
                }
                else
                {
                    break;
                }
            }
            else if (format[scanIndex] == brace)
            {
                if (brace == '}')
                {
                    if (braceOccurrenceCount == 0)
                        braceIndex = scanIndex;
                }
                else
                {
                    braceIndex = scanIndex;
                }
                braceOccurrenceCount++;
            }
            scanIndex++;
        }

        return braceIndex;
    }

    private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
    {
        var findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
        return findIndex == -1 ? endIndex : findIndex;
    }

    public string Format(object[] values)
    {
        var processedValues = values is null ? EmptyArray : (object[])values.Clone();
        for (var i = 0; i < processedValues.Length; i++)
        {
            var value = processedValues[i];
            if (value is null) { processedValues[i] = NullValue; continue; }
            if (value is string) continue;
            if (value is IEnumerable enumerable)
                processedValues[i] = string.Join(", ", enumerable.Cast<object>().Select(o => o ?? NullValue));
        }
        return string.Format(CultureInfo.InvariantCulture, _format, processedValues);
    }

    public KeyValuePair<string, object> GetValue(object[] values, int index)
    {
        if (index < 0 || index > _valueNames.Count)
            throw new IndexOutOfRangeException(nameof(index));

        return _valueNames.Count > index
            ? new KeyValuePair<string, object>(_valueNames[index], values[index])
            : new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
    }

    public IEnumerable<KeyValuePair<string, object>> GetValues(object[] values)
    {
        var valueArray = new KeyValuePair<string, object>[values.Length + 1];
        for (var index = 0; index < _valueNames.Count; index++)
            valueArray[index] = new KeyValuePair<string, object>(_valueNames[index], values[index]);
        valueArray[valueArray.Length - 1] = new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
        return valueArray;
    }
}
