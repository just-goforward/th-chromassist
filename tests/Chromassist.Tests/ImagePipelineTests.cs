using Chromassist.Core.Imaging;
using Chromassist.Core.Presets;

namespace Chromassist.Tests;

public sealed class ImagePipelineTests
{
    [Fact]
    public void PngRoundTripPreservesRgbaBytes()
    {
        var source = CreateFixture();
        using var stream = new MemoryStream();

        PngCodec.Write(stream, source);
        stream.Position = 0;
        var decoded = PngCodec.Read(stream);

        Assert.Equal(source.Width, decoded.Width);
        Assert.Equal(source.Height, decoded.Height);
        Assert.Equal(source.Pixels, decoded.Pixels);
    }

    [Fact]
    public void PresetTransformationPreservesProtectedInvariants()
    {
        var source = CreateFixture();
        var transformed = PresetTransformer.Transform(source, PresetCatalog.All.Single(preset => preset.Id == "deutan-standard"));
        var invariant = ImageInvariantChecker.Compare(source, transformed);

        Assert.True(invariant.Success);
        Assert.Equal(source.Pixels[0..4], transformed.Pixels[0..4]);
        Assert.Equal(source.Pixels.Where((_, index) => index % 4 == 3), transformed.Pixels.Where((_, index) => index % 4 == 3));
        Assert.NotEqual(source.Pixels, transformed.Pixels);
    }

    [Fact]
    public void SameImageAndPresetProduceIdenticalPng()
    {
        var source = CreateFixture();
        var preset = PresetCatalog.All.Single(item => item.Id == "protan-standard");

        Assert.Equal(Encode(PresetTransformer.Transform(source, preset)), Encode(PresetTransformer.Transform(source, preset)));
    }

    [Fact]
    public void DecoderRejectsUnsupportedInput()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);
        Assert.Throws<InvalidDataException>(() => PngCodec.Read(stream));
    }

    private static RgbaImage CreateFixture() => new(3, 2,
    [
        12, 34, 56, 0,
        230, 20, 30, 255,
        20, 220, 40, 180,
        30, 50, 230, 255,
        240, 210, 30, 64,
        180, 40, 190, 255
    ]);

    private static byte[] Encode(RgbaImage image)
    {
        using var stream = new MemoryStream();
        PngCodec.Write(stream, image);
        return stream.ToArray();
    }
}
