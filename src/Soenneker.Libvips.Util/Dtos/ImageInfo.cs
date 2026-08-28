using System;
using System.Collections.Generic;

namespace Soenneker.Libvips.Util.Dtos;

/// <summary>Information read from an image header.</summary>
/// <param name="Width">The image width in pixels.</param>
/// <param name="Height">The image height in pixels.</param>
/// <param name="Bands">The number of image bands.</param>
/// <param name="Format">The native pixel format.</param>
/// <param name="Coding">The pixel coding scheme.</param>
/// <param name="Interpretation">The colour-space interpretation.</param>
/// <param name="XResolution">The horizontal resolution in pixels per millimetre, when available.</param>
/// <param name="YResolution">The vertical resolution in pixels per millimetre, when available.</param>
/// <param name="Loader">The libvips loader used to read the image.</param>
/// <param name="Metadata">The complete, read-only header metadata.</param>
public readonly record struct ImageInfo(
    int Width,
    int Height,
    int Bands,
    string? Format = null,
    string? Coding = null,
    string? Interpretation = null,
    double? XResolution = null,
    double? YResolution = null,
    string? Loader = null,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    /// <summary>The total number of pixels in the image.</summary>
    public long PixelCount => (long)Width * Height;

    /// <summary>The width divided by the height, or zero when the height is unavailable.</summary>
    public double AspectRatio => Height == 0 ? 0 : (double)Width / Height;

    /// <summary>Attempts to get a metadata value using a case-insensitive key.</summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value when found.</param>
    /// <returns><see langword="true"/> when the key exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetMetadata(string key, out string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (Metadata is not null && Metadata.TryGetValue(key, out string? metadataValue))
        {
            value = metadataValue;
            return true;
        }

        value = null;
        return false;
    }
}