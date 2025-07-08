namespace SystemInstaller.Tests;

public class XUnitVerificationTests
{
    [Fact]
    public void Fact_ShouldWork()
    {
        // Simple fact test
        Assert.True(true);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    public void Theory_WithInlineData_ShouldWork(int a, int b, int expected)
    {
        // Theory test with parameters
        var result = a + b;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AssertEqual_ShouldWork()
    {
        var expected = "Hello World";
        var actual = "Hello World";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AssertNotEqual_ShouldWork()
    {
        Assert.NotEqual(1, 2);
    }

    [Fact]
    public void AssertNull_ShouldWork()
    {
        string? nullValue = null;
        Assert.Null(nullValue);
    }

    [Fact]
    public void AssertNotNull_ShouldWork()
    {
        var value = "not null";
        Assert.NotNull(value);
    }

    [Fact]
    public void AssertThrows_ShouldWork()
    {
        Assert.Throws<ArgumentNullException>(() => 
        {
            string? nullParam = null;
            ArgumentNullException.ThrowIfNull(nullParam);
        });
    }

    [Fact]
    public void AssertCollection_ShouldWork()
    {
        var collection = new[] { 1, 2, 3 };
        Assert.Collection(collection,
            item => Assert.Equal(1, item),
            item => Assert.Equal(2, item),
            item => Assert.Equal(3, item));
    }
}
