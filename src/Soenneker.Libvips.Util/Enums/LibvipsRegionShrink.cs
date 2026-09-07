using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>The method used to shrink TIFF pyramid regions.</summary>
[EnumValue<string>]
public sealed partial class LibvipsRegionShrink
{
    /// <summary>Uses the mean pixel value.</summary>
    public static readonly LibvipsRegionShrink Mean = new("mean");
    /// <summary>Uses the median pixel value.</summary>
    public static readonly LibvipsRegionShrink Median = new("median");
    /// <summary>Uses the modal pixel value.</summary>
    public static readonly LibvipsRegionShrink Mode = new("mode");
    /// <summary>Uses the maximum pixel value.</summary>
    public static readonly LibvipsRegionShrink Max = new("max");
    /// <summary>Uses the minimum pixel value.</summary>
    public static readonly LibvipsRegionShrink Min = new("min");
    /// <summary>Uses nearest-neighbor sampling.</summary>
    public static readonly LibvipsRegionShrink Nearest = new("nearest");
}
