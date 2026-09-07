using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>A PNG row-filter configuration.</summary>
[EnumValue<string>]
public sealed partial class LibvipsPngFilter
{
    /// <summary>Disables explicit row filtering.</summary>
    public static readonly LibvipsPngFilter None = new("none");
    /// <summary>Uses the Sub filter.</summary>
    public static readonly LibvipsPngFilter Sub = new("sub");
    /// <summary>Uses the Up filter.</summary>
    public static readonly LibvipsPngFilter Up = new("up");
    /// <summary>Uses the Average filter.</summary>
    public static readonly LibvipsPngFilter Average = new("avg");
    /// <summary>Uses the Paeth filter.</summary>
    public static readonly LibvipsPngFilter Paeth = new("paeth");
    /// <summary>Allows all supported row filters.</summary>
    public static readonly LibvipsPngFilter All = new("all");
}
