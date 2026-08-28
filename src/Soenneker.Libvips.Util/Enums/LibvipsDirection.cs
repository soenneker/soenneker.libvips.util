using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>Specifies the axis along which an image is mirrored.</summary>
[EnumValue<string>]
public sealed partial class LibvipsDirection
{
    /// <summary>Mirror left to right about the vertical axis.</summary>
    public static readonly LibvipsDirection Horizontal = new("horizontal");
    /// <summary>Mirror top to bottom about the horizontal axis.</summary>
    public static readonly LibvipsDirection Vertical = new("vertical");
}
