using Xunit;


namespace CsxSourceGenerator.Test;


public class Tests
{
    [Fact]
    public void CheckGeneratedContent()
    {
        var content = File.ReadAllText("content.txt");
#pragma warning disable xUnit2000
        Assert.Equal(content, Generated.Content);
#pragma warning restore xUnit2000
    }
}