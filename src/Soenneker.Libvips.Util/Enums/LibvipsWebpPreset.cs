using Soenneker.Gen.EnumValues;

namespace Soenneker.Libvips.Util.Enums;

/// <summary>A predefined WebP encoder configuration.</summary>
[EnumValue<string>]
public sealed partial class LibvipsWebpPreset
{
    /// <summary>Uses the default encoder configuration.</summary>
    public static readonly LibvipsWebpPreset Default = new("default");

    /// <summary>Optimizes for digital pictures.</summary>
    public static readonly LibvipsWebpPreset Picture = new("picture");

    /// <summary>Optimizes for outdoor photographs.</summary>
    public static readonly LibvipsWebpPreset Photo = new("photo");

    /// <summary>Optimizes for drawings and illustrations.</summary>
    public static readonly LibvipsWebpPreset Drawing = new("drawing");

    /// <summary>Optimizes for small colorful images.</summary>
    public static readonly LibvipsWebpPreset Icon = new("icon");

    /// <summary>Optimizes for text-like images.</summary>
    public static readonly LibvipsWebpPreset Text = new("text");
}