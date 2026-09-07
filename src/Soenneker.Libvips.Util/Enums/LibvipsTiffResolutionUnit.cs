using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>The resolution unit stored in TIFF output.</summary>
[EnumValue<string>]
public sealed partial class LibvipsTiffResolutionUnit
{
    /// <summary>Stores resolution in centimeters.</summary>
    public static readonly LibvipsTiffResolutionUnit Centimeter = new("cm");
    /// <summary>Stores resolution in inches.</summary>
    public static readonly LibvipsTiffResolutionUnit Inch = new("inch");
}
