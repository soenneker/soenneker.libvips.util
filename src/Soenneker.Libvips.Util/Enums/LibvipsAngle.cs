using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>A clockwise rotation supported without resampling.</summary>
[EnumValue<string>]
public sealed partial class LibvipsAngle
{
    /// <summary>Do not rotate.</summary>
    public static readonly LibvipsAngle D0 = new("d0");
    /// <summary>Rotate 90 degrees clockwise.</summary>
    public static readonly LibvipsAngle D90 = new("d90");
    /// <summary>Rotate 180 degrees clockwise.</summary>
    public static readonly LibvipsAngle D180 = new("d180");
    /// <summary>Rotate 270 degrees clockwise.</summary>
    public static readonly LibvipsAngle D270 = new("d270");
}
