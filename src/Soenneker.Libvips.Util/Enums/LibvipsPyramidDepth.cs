using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>The depth at which a TIFF pyramid stops.</summary>
[EnumValue<string>]
public sealed partial class LibvipsPyramidDepth
{
    /// <summary>Stops when the smallest level is one pixel.</summary>
    public static readonly LibvipsPyramidDepth OnePixel = new("onepixel");
    /// <summary>Stops when the smallest level fits in one tile.</summary>
    public static readonly LibvipsPyramidDepth OneTile = new("onetile");
    /// <summary>Writes one pyramid level.</summary>
    public static readonly LibvipsPyramidDepth One = new("one");
}
