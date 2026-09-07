using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls all TIFF-specific saver settings supported by the bundled libvips.</summary>
public sealed class TiffOptions : LibvipsOptions
{
    /// <summary>Gets the quality factor from 1 through 100 for codecs that use it.</summary>
    public int Quality { get; init; } = 80;

    /// <summary>Gets whether lossless WebP compression is enabled.</summary>
    public bool Lossless { get; init; }

    /// <summary>Gets the TIFF compression codec.</summary>
    public LibvipsTiffCompression CompressionFormat { get; init; } = LibvipsTiffCompression.None;

    /// <summary>Gets the compression predictor.</summary>
    public LibvipsTiffPredictor Predictor { get; init; } = LibvipsTiffPredictor.Horizontal;

    /// <summary>Gets whether tiled output is enabled.</summary>
    public bool Tile { get; init; }

    /// <summary>Gets the tile width in pixels.</summary>
    public int TileWidth { get; init; } = 128;

    /// <summary>Gets the tile height in pixels.</summary>
    public int TileHeight { get; init; } = 128;

    /// <summary>Gets whether pyramidal output is enabled.</summary>
    public bool Pyramid { get; init; }

    /// <summary>Gets whether zero represents white in one-bit output.</summary>
    public bool MinisWhite { get; init; }

    /// <summary>Gets the output bit depth, or 0 to preserve the input depth.</summary>
    public int BitDepth { get; init; }

    /// <summary>Gets the stored resolution unit.</summary>
    public LibvipsTiffResolutionUnit ResolutionUnit { get; init; } = LibvipsTiffResolutionUnit.Centimeter;

    /// <summary>Gets horizontal resolution in pixels per millimeter.</summary>
    public double XResolution { get; init; } = 1;

    /// <summary>Gets vertical resolution in pixels per millimeter.</summary>
    public double YResolution { get; init; } = 1;

    /// <summary>Gets whether BigTIFF output is enabled.</summary>
    public bool BigTiff { get; init; }

    /// <summary>Gets whether image properties are written to the image description.</summary>
    public bool Properties { get; init; }

    /// <summary>Gets the method used to shrink pyramid regions.</summary>
    public LibvipsRegionShrink RegionShrink { get; init; } = LibvipsRegionShrink.Mean;

    /// <summary>Gets the Deflate or Zstandard compression level.</summary>
    public int Level { get; init; }

    /// <summary>Gets the pyramid stopping depth.</summary>
    public LibvipsPyramidDepth Depth { get; init; } = LibvipsPyramidDepth.OneTile;

    /// <summary>Gets whether pyramid layers are stored as sub-IFDs.</summary>
    public bool SubIfd { get; init; }

    /// <summary>Gets whether alpha is saved premultiplied.</summary>
    public bool Premultiply { get; init; }

    /// <summary>Validates all common and TIFF-specific settings.</summary>
    public override void Validate()
    {
        base.Validate();
        System.ArgumentNullException.ThrowIfNull(CompressionFormat);
        System.ArgumentNullException.ThrowIfNull(Predictor);
        System.ArgumentNullException.ThrowIfNull(ResolutionUnit);
        System.ArgumentNullException.ThrowIfNull(RegionShrink);
        System.ArgumentNullException.ThrowIfNull(Depth);
        if (Quality is < 1 or > 100)
            throw new System.ArgumentOutOfRangeException(nameof(Quality));
        if (TileWidth is < 1 or > 32768)
            throw new System.ArgumentOutOfRangeException(nameof(TileWidth));
        if (TileHeight is < 1 or > 32768)
            throw new System.ArgumentOutOfRangeException(nameof(TileHeight));
        if (BitDepth is not (0 or 1 or 2 or 4 or 8))
            throw new System.ArgumentOutOfRangeException(nameof(BitDepth));
        if (!double.IsFinite(XResolution) || XResolution is < 0.001 or > 1_000_000)
            throw new System.ArgumentOutOfRangeException(nameof(XResolution));
        if (!double.IsFinite(YResolution) || YResolution is < 0.001 or > 1_000_000)
            throw new System.ArgumentOutOfRangeException(nameof(YResolution));
        if (Level is < 0 or > 22)
            throw new System.ArgumentOutOfRangeException(nameof(Level));
    }
}