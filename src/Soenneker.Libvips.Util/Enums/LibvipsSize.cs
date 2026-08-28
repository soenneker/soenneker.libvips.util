using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>Controls whether a thumbnail may enlarge or reduce an image.</summary>
[EnumValue<string>]
public sealed partial class LibvipsSize
{
    /// <summary>Allow both enlargement and reduction.</summary>
    public static readonly LibvipsSize Both = new("both");
    /// <summary>Only enlarge images that are smaller than the requested dimensions.</summary>
    public static readonly LibvipsSize Up = new("up");
    /// <summary>Only reduce images that are larger than the requested dimensions.</summary>
    public static readonly LibvipsSize Down = new("down");
    /// <summary>Force the exact requested dimensions, potentially changing the aspect ratio.</summary>
    public static readonly LibvipsSize Force = new("force");
}
