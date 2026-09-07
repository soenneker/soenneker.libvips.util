using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>The encoder implementation used for HEIF-family output.</summary>
[EnumValue<string>]
public sealed partial class LibvipsHeifEncoder
{
    /// <summary>Lets libvips select an available encoder.</summary>
    public static readonly LibvipsHeifEncoder Auto = new("auto");
    /// <summary>Uses the AOM encoder.</summary>
    public static readonly LibvipsHeifEncoder Aom = new("aom");
    /// <summary>Uses the rav1e encoder.</summary>
    public static readonly LibvipsHeifEncoder Rav1E = new("rav1e");
    /// <summary>Uses the SVT encoder.</summary>
    public static readonly LibvipsHeifEncoder Svt = new("svt");
    /// <summary>Uses the x265 encoder.</summary>
    public static readonly LibvipsHeifEncoder X265 = new("x265");
}
