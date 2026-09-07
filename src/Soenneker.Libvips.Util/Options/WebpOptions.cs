using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls all WebP-specific saver settings supported by the bundled libvips.</summary>
public sealed class WebpOptions : LibvipsOptions
{
    /// <summary>Gets the quality factor from 0 through 100.</summary>
    public int Quality { get; init; } = 80;
    /// <summary>Gets whether lossless encoding is enabled.</summary>
    public bool Lossless { get; init; }
    /// <summary>Gets the CPU effort from 0 through 6.</summary>
    public int Effort { get; init; } = 4;
    /// <summary>Gets whether color values beneath transparent pixels are preserved.</summary>
    public bool Exact { get; init; }
    /// <summary>Gets the predefined encoder configuration.</summary>
    public LibvipsWebpPreset Preset { get; init; } = LibvipsWebpPreset.Default;
    /// <summary>Gets whether high-quality chroma subsampling is enabled.</summary>
    public bool SmartSubsample { get; init; }
    /// <summary>Gets whether near-lossless preprocessing is enabled.</summary>
    public bool NearLossless { get; init; }
    /// <summary>Gets alpha-plane fidelity from 0 through 100.</summary>
    public int AlphaQuality { get; init; } = 100;
    /// <summary>Gets whether the encoder optimizes for minimum size.</summary>
    public bool MinimizeSize { get; init; }
    /// <summary>Gets the minimum number of frames between keyframes.</summary>
    public int MinimumKeyframeDistance { get; init; } = 2_147_483_646;
    /// <summary>Gets the maximum number of frames between keyframes.</summary>
    public int MaximumKeyframeDistance { get; init; } = int.MaxValue;
    /// <summary>Gets the desired target size in bytes, or 0 when unspecified.</summary>
    public int TargetSize { get; init; }
    /// <summary>Gets whether mixed lossy and lossless encoding is allowed.</summary>
    public bool Mixed { get; init; }
    /// <summary>Gets whether automatic deblocking adjustment is enabled.</summary>
    public bool SmartDeblock { get; init; }
    /// <summary>Gets the number of entropy-analysis passes from 1 through 10.</summary>
    public int Passes { get; init; } = 1;

    /// <summary>Validates all common and WebP-specific settings.</summary>
    public override void Validate()
    {
        base.Validate();
        System.ArgumentNullException.ThrowIfNull(Preset);
        if (Quality is < 0 or > 100) throw new System.ArgumentOutOfRangeException(nameof(Quality));
        if (Effort is < 0 or > 6) throw new System.ArgumentOutOfRangeException(nameof(Effort), "WebP effort must be between 0 and 6.");
        if (AlphaQuality is < 0 or > 100) throw new System.ArgumentOutOfRangeException(nameof(AlphaQuality));
        if (MinimumKeyframeDistance < 0) throw new System.ArgumentOutOfRangeException(nameof(MinimumKeyframeDistance));
        if (MaximumKeyframeDistance < 0) throw new System.ArgumentOutOfRangeException(nameof(MaximumKeyframeDistance));
        if (TargetSize < 0) throw new System.ArgumentOutOfRangeException(nameof(TargetSize));
        if (Passes is < 1 or > 10) throw new System.ArgumentOutOfRangeException(nameof(Passes));
    }
}
