using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>Chroma subsampling behavior used by JPEG and HEIF savers.</summary>
[EnumValue<string>]
public sealed partial class LibvipsSubsampleMode
{
    /// <summary>Lets libvips select the subsampling behavior.</summary>
    public static readonly LibvipsSubsampleMode Auto = new("auto");
    /// <summary>Enables chroma subsampling.</summary>
    public static readonly LibvipsSubsampleMode On = new("on");
    /// <summary>Disables chroma subsampling.</summary>
    public static readonly LibvipsSubsampleMode Off = new("off");
}
