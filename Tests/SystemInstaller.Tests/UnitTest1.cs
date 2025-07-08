namespace SystemInstaller.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Simple test to verify xUnit is working
        var result = 1 + 1;
        Assert.Equal(2, result);
    }

    [Fact]
    public void BasicAssertions_ShouldWork()
    {
        // Test multiple assertions
        Assert.True(true);
        Assert.False(false);
        Assert.NotNull("test");
        Assert.Equal("hello", "hello");
    }
}
