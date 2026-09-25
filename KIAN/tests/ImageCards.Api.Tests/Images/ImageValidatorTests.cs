using ImageCards.Api.Configuration;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Services.Images;
using ImageCards.Api.Tests.TestSupport;
using Microsoft.Extensions.Options;

namespace ImageCards.Api.Tests.Images;

public class ImageValidatorTests
{
    private readonly ImageValidator _validator = new(Options.Create(new ImageUploadOptions { MaxBytes = 1024 }));

    public static TheoryData<byte[], string, string, string> ValidImages => new()
    {
        { TestImages.Png, "apple.png", "image/png", ".png" },
        { TestImages.Jpeg, "apple.jpg", "image/jpeg", ".jpg" },
        { TestImages.Jpeg, "APPLE.JPEG", "image/jpeg", ".jpg" },
        { TestImages.Webp, "apple.webp", "image/webp", ".webp" },
    };

    [Theory]
    [MemberData(nameof(ValidImages))]
    public async Task ValidImage_ReturnsFormatWithCanonicalExtension(
        byte[] content, string fileName, string contentType, string expectedExtension)
    {
        var format = await _validator.ValidateAsync(TestImages.FormFile(content, fileName, contentType), "image", default);

        Assert.Equal(contentType, format.ContentType);
        Assert.Equal(expectedExtension, format.Extension);
    }

    [Fact]
    public async Task EmptyFile_IsRejected()
    {
        var file = TestImages.FormFile([], "apple.png", "image/png");

        var error = await Assert.ThrowsAsync<RequestValidationException>(() => _validator.ValidateAsync(file, "image", default));
        Assert.Contains("image", error.Errors.Keys);
    }

    [Fact]
    public async Task FileLargerThanLimit_IsRejected()
    {
        var file = TestImages.FormFile([.. TestImages.Png, .. new byte[2048]], "apple.png", "image/png");

        var error = await Assert.ThrowsAsync<RequestValidationException>(() => _validator.ValidateAsync(file, "image", default));
        Assert.Contains("maximum size", error.Errors["image"][0]);
    }

    [Theory]
    [InlineData("image/gif", "apple.gif")]
    [InlineData("image/svg+xml", "apple.svg")]
    [InlineData("application/octet-stream", "apple.png")]
    public async Task UnsupportedContentType_IsRejected(string contentType, string fileName)
    {
        var file = TestImages.FormFile(TestImages.Png, fileName, contentType);

        await Assert.ThrowsAsync<RequestValidationException>(() => _validator.ValidateAsync(file, "image", default));
    }

    [Theory]
    [InlineData("apple.exe")]
    [InlineData("apple.png.exe")]
    [InlineData("apple")]
    [InlineData("apple.jpg")]
    public async Task ExtensionNotMatchingContentType_IsRejected(string fileName)
    {
        var file = TestImages.FormFile(TestImages.Png, fileName, "image/png");

        await Assert.ThrowsAsync<RequestValidationException>(() => _validator.ValidateAsync(file, "image", default));
    }

    [Fact]
    public async Task ContentNotMatchingSignature_IsRejected()
    {
        var file = TestImages.FormFile(TestImages.NotAnImage, "shell.png", "image/png");

        var error = await Assert.ThrowsAsync<RequestValidationException>(() => _validator.ValidateAsync(file, "image", default));
        Assert.Contains("not a valid image", error.Errors["image"][0]);
    }

    [Fact]
    public async Task JpegContentDeclaredAsPng_IsRejected()
    {
        var file = TestImages.FormFile(TestImages.Jpeg, "apple.png", "image/png");

        await Assert.ThrowsAsync<RequestValidationException>(() => _validator.ValidateAsync(file, "image", default));
    }
}
