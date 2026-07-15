namespace Chromassist.Core.Imaging;

public sealed class RgbaImage
{
    public RgbaImage(int width, int height, byte[] pixels)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(pixels);

        if (pixels.Length != checked(width * height * 4))
        {
            throw new ArgumentException("RGBA pixel data length does not match the image dimensions.", nameof(pixels));
        }

        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }

    public int Height { get; }

    public byte[] Pixels { get; }

    public RgbaImage Clone() => new(Width, Height, (byte[])Pixels.Clone());
}
