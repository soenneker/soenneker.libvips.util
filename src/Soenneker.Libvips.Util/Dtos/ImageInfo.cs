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
    IReadOnlyDictionary<string, string>? Metadata = null);
