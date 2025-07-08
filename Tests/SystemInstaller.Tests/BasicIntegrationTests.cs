using Xunit;

namespace SystemInstaller.Tests;

public class BasicIntegrationTests
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var expected = 2;
        
        // Act
        var actual = 1 + 1;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
