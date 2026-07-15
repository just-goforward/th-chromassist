using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Chromassist.Core.Imaging;

public static class PngCodec
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private const int MaximumDimension = 16_384;
    private const int MaximumDecodedBytes = 256 * 1024 * 1024;

    public static RgbaImage Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    public static RgbaImage Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Span<byte> signature = stackalloc byte[Signature.Length];
        stream.ReadExactly(signature);
        if (!signature.SequenceEqual(Signature))
        {
            throw new InvalidDataException("The file is not a PNG image.");
        }

        int? width = null;
        int? height = null;
        var seenHeader = false;
        var seenEnd = false;
        using var compressedData = new MemoryStream();
        var chunkHeaderBuffer = new byte[8];
        var crcBuffer = new byte[4];

        while (!seenEnd)
        {
            var chunkHeader = chunkHeaderBuffer.AsSpan();
            stream.ReadExactly(chunkHeader);
            var length = BinaryPrimitives.ReadUInt32BigEndian(chunkHeader[..4]);
            if (length > MaximumDecodedBytes)
            {
                throw new InvalidDataException("PNG chunk is too large.");
            }

            var chunkType = chunkHeader[4..8].ToArray();
            var data = new byte[length];
            stream.ReadExactly(data);
            var crcBytes = crcBuffer.AsSpan();
            stream.ReadExactly(crcBytes);
            var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(crcBytes);
            var actualCrc = Crc32.Compute(chunkType, data);
            if (actualCrc != expectedCrc)
            {
                throw new InvalidDataException($"PNG chunk {Encoding.ASCII.GetString(chunkType)} has an invalid CRC.");
            }

            var type = Encoding.ASCII.GetString(chunkType);
            switch (type)
            {
                case "IHDR":
                    if (seenHeader || data.Length != 13)
                    {
                        throw new InvalidDataException("PNG contains an invalid IHDR chunk.");
                    }

                    width = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(0, 4)));
                    height = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(4, 4)));
                    if (width is <= 0 or > MaximumDimension || height is <= 0 or > MaximumDimension)
                    {
                        throw new InvalidDataException("PNG dimensions are outside the supported range.");
                    }

                    if (data[8] != 8 || data[9] != 6 || data[10] != 0 || data[11] != 0 || data[12] != 0)
                    {
                        throw new NotSupportedException("Only non-interlaced 8-bit RGBA PNG images are supported.");
                    }

                    seenHeader = true;
                    break;
                case "IDAT":
                    if (!seenHeader)
                    {
                        throw new InvalidDataException("PNG IDAT appeared before IHDR.");
                    }

                    compressedData.Write(data);
                    break;
                case "IEND":
                    seenEnd = true;
                    break;
            }
        }

        if (!seenHeader || width is null || height is null || compressedData.Length == 0)
        {
            throw new InvalidDataException("PNG is missing required chunks.");
        }

        var stride = checked(width.Value * 4);
        var decodedLength = checked((stride + 1) * height.Value);
        if (decodedLength > MaximumDecodedBytes)
        {
            throw new InvalidDataException("Decoded PNG would exceed the safety limit.");
        }

        var filtered = new byte[decodedLength];
        compressedData.Position = 0;
        using (var zlib = new ZLibStream(compressedData, CompressionMode.Decompress, leaveOpen: true))
        {
            zlib.ReadExactly(filtered);
            if (zlib.ReadByte() != -1)
            {
                throw new InvalidDataException("PNG contains more decoded data than expected.");
            }
        }

        return new RgbaImage(width.Value, height.Value, Unfilter(filtered, width.Value, height.Value));
    }

    public static void Write(string path, RgbaImage image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(image);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path);
        Write(stream, image);
    }

    public static void Write(Stream stream, RgbaImage image)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);

        stream.Write(Signature);
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header[..4], checked((uint)image.Width));
        BinaryPrimitives.WriteUInt32BigEndian(header.Slice(4, 4), checked((uint)image.Height));
        header[8] = 8;
        header[9] = 6;
        WriteChunk(stream, "IHDR", header);

        var stride = checked(image.Width * 4);
        var scanlines = new byte[checked((stride + 1) * image.Height)];
        for (var row = 0; row < image.Height; row++)
        {
            var destinationOffset = row * (stride + 1);
            scanlines[destinationOffset] = 0;
            image.Pixels.AsSpan(row * stride, stride).CopyTo(scanlines.AsSpan(destinationOffset + 1, stride));
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(scanlines);
        }

        WriteChunk(stream, "IDAT", compressed.ToArray());
        WriteChunk(stream, "IEND", ReadOnlySpan<byte>.Empty);
    }

    private static byte[] Unfilter(byte[] filtered, int width, int height)
    {
        const int bytesPerPixel = 4;
        var stride = checked(width * bytesPerPixel);
        var pixels = new byte[checked(stride * height)];

        for (var row = 0; row < height; row++)
        {
            var inputOffset = row * (stride + 1);
            var outputOffset = row * stride;
            var filter = filtered[inputOffset];
            for (var column = 0; column < stride; column++)
            {
                var raw = filtered[inputOffset + 1 + column];
                var left = column >= bytesPerPixel ? pixels[outputOffset + column - bytesPerPixel] : (byte)0;
                var above = row > 0 ? pixels[outputOffset - stride + column] : (byte)0;
                var upperLeft = row > 0 && column >= bytesPerPixel
                    ? pixels[outputOffset - stride + column - bytesPerPixel]
                    : (byte)0;
                pixels[outputOffset + column] = filter switch
                {
                    0 => raw,
                    1 => unchecked((byte)(raw + left)),
                    2 => unchecked((byte)(raw + above)),
                    3 => unchecked((byte)(raw + ((left + above) >> 1))),
                    4 => unchecked((byte)(raw + Paeth(left, above, upperLeft))),
                    _ => throw new InvalidDataException($"Unsupported PNG filter type: {filter}.")
                };
            }
        }

        return pixels;
    }

    private static byte Paeth(byte left, byte above, byte upperLeft)
    {
        var estimate = left + above - upperLeft;
        var leftDistance = Math.Abs(estimate - left);
        var aboveDistance = Math.Abs(estimate - above);
        var upperLeftDistance = Math.Abs(estimate - upperLeft);
        return leftDistance <= aboveDistance && leftDistance <= upperLeftDistance
            ? left
            : aboveDistance <= upperLeftDistance ? above : upperLeft;
    }

    private static void WriteChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, checked((uint)data.Length));
        stream.Write(length);
        stream.Write(typeBytes);
        stream.Write(data);
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32.Compute(typeBytes, data));
        stream.Write(crc);
    }

    private static class Crc32
    {
        private static readonly uint[] Table = BuildTable();

        public static uint Compute(ReadOnlySpan<byte> prefix, ReadOnlySpan<byte> data)
        {
            var crc = uint.MaxValue;
            foreach (var value in prefix)
            {
                crc = Table[(crc ^ value) & 0xff] ^ (crc >> 8);
            }

            foreach (var value in data)
            {
                crc = Table[(crc ^ value) & 0xff] ^ (crc >> 8);
            }

            return crc ^ uint.MaxValue;
        }

        private static uint[] BuildTable()
        {
            var table = new uint[256];
            for (uint index = 0; index < table.Length; index++)
            {
                var value = index;
                for (var bit = 0; bit < 8; bit++)
                {
                    value = (value & 1) == 1 ? 0xedb88320U ^ (value >> 1) : value >> 1;
                }

                table[index] = value;
            }

            return table;
        }
    }
}
