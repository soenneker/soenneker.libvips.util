using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>TIFF compression prediction.</summary>
[EnumValue<string>]
public sealed partial class LibvipsTiffPredictor
{
    /// <summary>Disables prediction.</summary>
    public static readonly LibvipsTiffPredictor None = new("none");
    /// <summary>Uses horizontal differencing.</summary>
    public static readonly LibvipsTiffPredictor Horizontal = new("horizontal");
    /// <summary>Uses floating-point prediction.</summary>
    public static readonly LibvipsTiffPredictor Float = new("float");
}
