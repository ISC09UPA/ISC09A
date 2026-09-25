using ImageCards.Api.Services.Images;
using ImageCards.Api.Services.Storage;

namespace ImageCards.Api.Tests.Storage;

public class BlobNamesTests
{
    [Fact]
    public void Create_GeneratesUniqueValidNamesWithCanonicalExtension()
    {
        var first = BlobNames.Create(ImageFormat.Jpeg);
        var second = BlobNames.Create(ImageFormat.Jpeg);

        Assert.NotEqual(first, second);
        Assert.True(BlobNames.IsValid(first));
        Assert.EndsWith(".jpg", first);
        Assert.Matches("^[0-9a-f]{32}\\.jpg$", first);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("../../secrets.png")]
    [InlineData("folder/0123456789abcdef0123456789abcdef.png")]
    [InlineData("0123456789abcdef0123456789abcdef.gif")]
    [InlineData("0123456789ABCDEF0123456789ABCDEF.png")]
    [InlineData("apple.png")]
    public void IsValid_RejectsAnythingNotGeneratedByCreate(string? blobName)
    {
        Assert.False(BlobNames.IsValid(blobName));
    }

    [Fact]
    public void EnsureValid_ThrowsForInvalidName()
    {
        Assert.Throws<ArgumentException>(() => BlobNames.EnsureValid("../x.png"));
    }
}
