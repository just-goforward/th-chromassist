namespace Chromassist.Core.Color;

public readonly record struct OklabColor(double L, double A, double B)
{
    public static OklabColor FromSrgb(byte red, byte green, byte blue)
    {
        var r = ToLinear(red / 255d);
        var g = ToLinear(green / 255d);
        var b = ToLinear(blue / 255d);

        var l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
        var m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
        var s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

        var lRoot = Math.Cbrt(l);
        var mRoot = Math.Cbrt(m);
        var sRoot = Math.Cbrt(s);

        return new OklabColor(
            0.2104542553 * lRoot + 0.7936177850 * mRoot - 0.0040720468 * sRoot,
            1.9779984951 * lRoot - 2.4285922050 * mRoot + 0.4505937099 * sRoot,
            0.0259040371 * lRoot + 0.7827717662 * mRoot - 0.8086757660 * sRoot);
    }

    public (byte Red, byte Green, byte Blue) ToSrgb()
    {
        var lRoot = L + 0.3963377774 * A + 0.2158037573 * B;
        var mRoot = L - 0.1055613458 * A - 0.0638541728 * B;
        var sRoot = L - 0.0894841775 * A - 1.2914855480 * B;

        var l = lRoot * lRoot * lRoot;
        var m = mRoot * mRoot * mRoot;
        var s = sRoot * sRoot * sRoot;

        var r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        var g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        var b = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        return (ToByte(ToSrgbChannel(r)), ToByte(ToSrgbChannel(g)), ToByte(ToSrgbChannel(b)));
    }

    public (double Lightness, double Chroma, double HueRadians) ToOklch() =>
        (L, Math.Sqrt(A * A + B * B), Math.Atan2(B, A));

    public static OklabColor FromOklch(double lightness, double chroma, double hueRadians) =>
        new(lightness, chroma * Math.Cos(hueRadians), chroma * Math.Sin(hueRadians));

    private static double ToLinear(double channel) => channel <= 0.04045
        ? channel / 12.92
        : Math.Pow((channel + 0.055) / 1.055, 2.4);

    private static double ToSrgbChannel(double channel) => channel <= 0.0031308
        ? 12.92 * channel
        : 1.055 * Math.Pow(channel, 1d / 2.4) - 0.055;

    private static byte ToByte(double value) => (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
}
