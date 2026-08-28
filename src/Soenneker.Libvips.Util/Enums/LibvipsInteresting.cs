using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>How libvips selects the interesting portion of an image when cropping.</summary>
[EnumValue<string>]
public sealed partial class LibvipsInteresting
{
    /// <summary>Do not apply an interestingness strategy.</summary>
    public static readonly LibvipsInteresting None = new("none");
    /// <summary>Prefer the centre of the image.</summary>
    public static readonly LibvipsInteresting Centre = new("centre");
    /// <summary>Prefer the region with the highest spatial entropy.</summary>
    public static readonly LibvipsInteresting Entropy = new("entropy");
    /// <summary>Prefer regions identified by the libvips attention heuristic.</summary>
    public static readonly LibvipsInteresting Attention = new("attention");
    /// <summary>Prefer the low-coordinate edge.</summary>
    public static readonly LibvipsInteresting Low = new("low");
    /// <summary>Prefer the high-coordinate edge.</summary>
    public static readonly LibvipsInteresting High = new("high");
    /// <summary>Consider the complete image.</summary>
    public static readonly LibvipsInteresting All = new("all");
}
