using IssueManager.Core.Models.Enums;
using IssueManager.Core.Services.Helpers;

namespace IssueManager.Tests.Helpers;

public class HelpersMethodTests
{
    [Theory]
    [InlineData("open", IssueState.Open)]
    [InlineData("OPEN", IssueState.Open)]
    [InlineData("Open", IssueState.Open)]
    [InlineData("closed", IssueState.Closed)]
    [InlineData("CLOSED", IssueState.Closed)]
    [InlineData("Closed", IssueState.Closed)]
    public void MapState_ValidStates_ReturnsCorrectEnum(string input, IssueState expected)
    {
        // Act
        var result = HelpersMethod.MapState(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("in-progress")]
    [InlineData("pending")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void MapState_InvalidStates_ThrowsInvalidOperationException(string input)
    {
        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => HelpersMethod.MapState(input));

        // Assert
        Assert.StartsWith("Unknown", exception.Message);
    }
}
