using System;
using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls thumbnail sizing, cropping, orientation, and colour processing.</summary>
public sealed class ResizeOptions
{
    /// <summary>Gets the requested output width in pixels.</summary>
    public required int Width { get; init; }
    /// <summary>Gets the optional requested output height in pixels.</summary>
    public int? Height { get; init; }
    /// <summary>Gets whether the operation may enlarge, reduce, or force dimensions.</summary>
    public LibvipsSize Size { get; init; } = LibvipsSize.Down;
    /// <summary>Gets the strategy used to crop an image when filling target dimensions.</summary>
    public LibvipsInteresting Crop { get; init; } = LibvipsInteresting.None;
    /// <summary>Gets whether EXIF orientation is applied automatically. Defaults to <see langword="true"/>.</summary>
    public bool AutoRotate { get; init; } = true;
    /// <summary>Gets whether resizing is performed in linear-light colour space.</summary>
    public bool LinearLight { get; init; }

    /// <summary>Validates the requested dimensions and resize modes.</summary>
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Size);
        ArgumentNullException.ThrowIfNull(Crop);

        if (Width <= 0)
            throw new ArgumentOutOfRangeException(nameof(Width), "Width must be greater than zero.");
        if (Height is <= 0)
            throw new ArgumentOutOfRangeException(nameof(Height), "Height must be greater than zero.");
    }
}
