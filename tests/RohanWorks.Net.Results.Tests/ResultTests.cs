using RohanWorks.Net.Results;

namespace RohanWorks.Net.Results.Tests;

public class ResultTests
{
    [Fact]
    public void ImplicitFromValue_SetsIsSuccessTrue()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void ImplicitFromException_SetsIsSuccessFalse()
    {
        var ex = new InvalidOperationException("boom");
        Result<string> result = ex;

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Exception.Should().BeSameAs(ex);
    }

    [Fact]
    public void DefaultConstructor_Throws()
    {
        // [Obsolete(error:true)] is a compile-time error; test runtime enforcement via reflection
        var act = () => (Result<string>)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(Result<string>));

        // Uninitialised struct won't throw — we verify the static analysis guard exists by
        // ensuring calling the constructor through reflection does throw.
        var ctor = typeof(Result<string>).GetConstructor(System.Type.EmptyTypes);
        var reflectedAct = () => ctor?.Invoke(null);
        reflectedAct.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>();
    }
}
