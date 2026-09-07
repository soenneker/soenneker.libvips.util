using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls all JPEG-specific saver settings supported by the bundled libvips.</summary>
public sealed class JpegOptions : LibvipsOptions
{
    /// <summary>Gets the quality factor from 1 through 100.</summary>
    public int Quality { get; init; } = 80;
    /// <summary>Gets whether progressive JPEG output is enabled.</summary>
    public bool Progressive { get; init; }
    /// <summary>Gets whether optimized Huffman coding tables are generated.</summary>
    public bool OptimizeCoding { get; init; } = true;
    /// <summary>Gets whether trellis quantization is applied.</summary>
    public bool TrellisQuantization { get; init; }
    /// <summary>Gets whether overshooting is applied to samples with extreme values.</summary>
    public bool OvershootDeringing { get; init; }
    /// <summary>Gets whether DCT coefficients are divided into optimized scans.</summary>
    public bool OptimizeScans { get; init; }
    /// <summary>Gets the predefined quantization table index from 0 through 8.</summary>
    public int QuantizationTable { get; init; }
    /// <summary>Gets the chroma subsampling behavior.</summary>
    public LibvipsSubsampleMode SubsampleMode { get; init; } = LibvipsSubsampleMode.Auto;
    /// <summary>Gets the number of minimum coded units between restart markers.</summary>
    public int RestartInterval { get; init; }

    /// <summary>Validates all common and JPEG-specific settings.</summary>
    public override void Validate()
    {
        base.Validate();
        System.ArgumentNullException.ThrowIfNull(SubsampleMode);
        if (Quality is < 1 or > 100) throw new System.ArgumentOutOfRangeException(nameof(Quality), "JPEG quality must be between 1 and 100.");
        if (QuantizationTable is < 0 or > 8) throw new System.ArgumentOutOfRangeException(nameof(QuantizationTable));
        if (RestartInterval < 0) throw new System.ArgumentOutOfRangeException(nameof(RestartInterval));
    }
}
