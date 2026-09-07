using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls all AVIF/HEIF-specific saver settings supported by the bundled libvips.</summary>
public sealed class AvifOptions : LibvipsOptions
{
    /// <summary>Gets the quality factor from 1 through 100.</summary>
    public int Quality { get; init; } = 80;

    /// <summary>Gets whether lossless encoding is enabled.</summary>
    public bool Lossless { get; init; }

    /// <summary>Gets the CPU effort from 0 through 9.</summary>
    public int Effort { get; init; } = 4;

    /// <summary>Gets the output bit depth from 8 through 12.</summary>
    public int BitDepth { get; init; } = 12;

    /// <summary>Gets the compression codec.</summary>
    public LibvipsHeifCompression CompressionFormat { get; init; } = LibvipsHeifCompression.Av1;

    /// <summary>Gets the chroma subsampling behavior.</summary>
    public LibvipsSubsampleMode SubsampleMode { get; init; } = LibvipsSubsampleMode.Auto;

    /// <summary>Gets the encoder implementation.</summary>
    public LibvipsHeifEncoder Encoder { get; init; } = LibvipsHeifEncoder.Auto;

    /// <summary>Gets optional encoder-specific tuning parameters.</summary>
    public string? Tune { get; init; }

    /// <summary>Validates all common and AVIF-specific settings.</summary>
    public override void Validate()
    {
        base.Validate();
        System.ArgumentNullException.ThrowIfNull(CompressionFormat);
        System.ArgumentNullException.ThrowIfNull(SubsampleMode);
        System.ArgumentNullException.ThrowIfNull(Encoder);
        if (Quality is < 1 or > 100)
            throw new System.ArgumentOutOfRangeException(nameof(Quality), "AVIF quality must be between 1 and 100.");
        if (Effort is < 0 or > 9)
            throw new System.ArgumentOutOfRangeException(nameof(Effort));
        if (BitDepth is < 8 or > 12)
            throw new System.ArgumentOutOfRangeException(nameof(BitDepth));
    }
}