using System;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls output encoding. Only options supported by the selected output format are emitted.</summary>
public sealed class LibvipsOptions
{
    /// <summary>Encoder quality from 1 through 100.</summary>
    public int Quality { get; init; } = 80;

    /// <summary>Encoder effort from 0 (fastest) through 6 (slowest).</summary>
    public int Effort { get; init; } = 4;

    /// <summary>Enables lossless encoding.</summary>
    public bool Lossless { get; init; }

    /// <summary>Removes metadata such as EXIF from the generated image.</summary>
    public bool StripMetadata { get; init; } = true;

    /// <summary>Enables progressive/interlaced output for formats which support it.</summary>
    public bool Progressive { get; init; }

    /// <summary>PNG compression level from 0 through 9.</summary>
    public int Compression { get; init; } = 6;

    /// <summary>Uses optimized Huffman coding for JPEG output.</summary>
    public bool OptimizeCoding { get; init; } = true;

    internal void Validate()
    {
        if (Quality is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Quality), "Quality must be between 1 and 100.");

        if (Effort is < 0 or > 6)
            throw new ArgumentOutOfRangeException(nameof(Effort), "Effort must be between 0 and 6.");

        if (Compression is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(Compression), "Compression must be between 0 and 9.");
    }
}
