using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls all PNG-specific saver settings supported by the bundled libvips.</summary>
public sealed class PngOptions : LibvipsOptions
{
    /// <summary>Gets the compression level from 0 through 9.</summary>
    public int Compression { get; init; } = 6;

    /// <summary>Gets whether Adam7 interlacing is enabled.</summary>
    public bool Interlace { get; init; }

    /// <summary>Gets the palette quantization quality from 0 through 100.</summary>
    public int Quality { get; init; } = 100;

    /// <summary>Gets the PNG row-filter configuration.</summary>
    public LibvipsPngFilter Filter { get; init; } = LibvipsPngFilter.None;

    /// <summary>Gets whether the image is quantized to an 8-bit palette.</summary>
    public bool Palette { get; init; }

    /// <summary>Gets the dithering amount from 0 through 1.</summary>
    public double Dither { get; init; } = 1;

    /// <summary>Gets the output bit depth: 1, 2, 4, 8, or 16.</summary>
    public int BitDepth { get; init; } = 8;

    /// <summary>Gets the quantization CPU effort from 1 through 10.</summary>
    public int QuantizationEffort { get; init; } = 7;

    /// <summary>Validates all common and PNG-specific settings.</summary>
    public override void Validate()
    {
        base.Validate();
        System.ArgumentNullException.ThrowIfNull(Filter);
        if (Compression is < 0 or > 9)
            throw new System.ArgumentOutOfRangeException(nameof(Compression));
        if (Quality is < 0 or > 100)
            throw new System.ArgumentOutOfRangeException(nameof(Quality));
        if (!double.IsFinite(Dither) || Dither is < 0 or > 1)
            throw new System.ArgumentOutOfRangeException(nameof(Dither));
        if (BitDepth is not (1 or 2 or 4 or 8 or 16))
            throw new System.ArgumentOutOfRangeException(nameof(BitDepth));
        if (QuantizationEffort is < 1 or > 10)
            throw new System.ArgumentOutOfRangeException(nameof(QuantizationEffort));
    }
}