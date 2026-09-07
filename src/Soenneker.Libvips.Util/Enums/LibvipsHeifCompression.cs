using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>The compression codec used for HEIF-family output.</summary>
[EnumValue<string>]
public sealed partial class LibvipsHeifCompression
{
    /// <summary>Uses HEVC compression.</summary>
    public static readonly LibvipsHeifCompression Hevc = new("hevc");
    /// <summary>Uses AVC compression.</summary>
    public static readonly LibvipsHeifCompression Avc = new("avc");
    /// <summary>Uses JPEG compression.</summary>
    public static readonly LibvipsHeifCompression Jpeg = new("jpeg");
    /// <summary>Uses AV1 compression.</summary>
    public static readonly LibvipsHeifCompression Av1 = new("av1");
}
