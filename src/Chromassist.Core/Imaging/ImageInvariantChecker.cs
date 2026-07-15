namespace Chromassist.Core.Imaging;

public static class ImageInvariantChecker
{
    public static ImageInvariantResult Compare(RgbaImage source, RgbaImage output)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(output);

        var dimensionsPreserved = source.Width == output.Width && source.Height == output.Height;
        if (!dimensionsPreserved)
        {
            return new(false, false, false, "Image dimensions changed.");
        }

        var alphaPreserved = true;
        var transparentPixelsPreserved = true;
        for (var index = 0; index < source.Pixels.Length; index += 4)
        {
            if (source.Pixels[index + 3] != output.Pixels[index + 3])
            {
                alphaPreserved = false;
            }

            if (source.Pixels[index + 3] == 0 &&
                !source.Pixels.AsSpan(index, 4).SequenceEqual(output.Pixels.AsSpan(index, 4)))
            {
                transparentPixelsPreserved = false;
            }
        }

        return new(dimensionsPreserved, alphaPreserved, transparentPixelsPreserved,
            dimensionsPreserved && alphaPreserved && transparentPixelsPreserved
                ? "All protected image invariants were preserved."
                : "One or more protected image invariants changed.");
    }
}

public sealed record ImageInvariantResult(
    bool DimensionsPreserved,
    bool AlphaPreserved,
    bool TransparentPixelsPreserved,
    string Summary)
{
    public bool Success => DimensionsPreserved && AlphaPreserved && TransparentPixelsPreserved;
}
