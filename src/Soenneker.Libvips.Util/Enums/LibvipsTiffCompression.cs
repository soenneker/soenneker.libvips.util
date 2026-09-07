using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>The compression codec used for TIFF output.</summary>
[EnumValue<string>]
public sealed partial class LibvipsTiffCompression
{
    /// <summary>Disables compression.</summary>
    public static readonly LibvipsTiffCompression None = new("none");
    /// <summary>Uses JPEG compression.</summary>
    public static readonly LibvipsTiffCompression Jpeg = new("jpeg");
    /// <summary>Uses Deflate compression.</summary>
    public static readonly LibvipsTiffCompression Deflate = new("deflate");
    /// <summary>Uses PackBits compression.</summary>
    public static readonly LibvipsTiffCompression Packbits = new("packbits");
    /// <summary>Uses CCITT Group 4 fax compression.</summary>
    public static readonly LibvipsTiffCompression CcittFax4 = new("ccittfax4");
    /// <summary>Uses LZW compression.</summary>
    public static readonly LibvipsTiffCompression Lzw = new("lzw");
    /// <summary>Uses WebP compression.</summary>
    public static readonly LibvipsTiffCompression Webp = new("webp");
    /// <summary>Uses Zstandard compression.</summary>
    public static readonly LibvipsTiffCompression Zstd = new("zstd");
    /// <summary>Uses JPEG 2000 compression.</summary>
    public static readonly LibvipsTiffCompression Jp2K = new("jp2k");
}
