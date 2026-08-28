using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Libvips.Util.Commands;
using Soenneker.Libvips.Util.Dtos;
using Soenneker.Libvips.Util.Enums;
using Soenneker.Libvips.Util.Options;
using Soenneker.Libvips.Util.Pipelines;

namespace Soenneker.Libvips.Util.Abstract;

/// <summary>
/// Provides asynchronous, cross-platform image processing through the bundled libvips command-line tools.
/// </summary>
public interface ILibvipsUtil
{
    /// <summary>Executes raw arguments against the bundled <c>vips</c> executable.</summary>
    /// <param name="arguments">The complete argument string passed to <c>vips</c>.</param>
    /// <param name="workingDirectory">The process working directory, or <see langword="null"/> to use the current directory.</param>
    /// <param name="log">Whether process output should be logged.</param>
    /// <param name="cancellationToken">A token that can cancel the process.</param>
    /// <returns>The standard output emitted by the process, split into lines.</returns>
    ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>Executes a structured operation against the bundled <c>vips</c> executable.</summary>
    /// <param name="command">The validated operation, positional arguments, and named options.</param>
    /// <param name="workingDirectory">The process working directory, or <see langword="null"/> to use the current directory.</param>
    /// <param name="log">Whether process output should be logged.</param>
    /// <param name="cancellationToken">A token that can cancel the process.</param>
    /// <returns>The standard output emitted by the process, split into lines.</returns>
    ValueTask<List<string>> Execute(LibvipsCommand command, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the version reported by the bundled <c>vips</c> executable.</summary>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The version string, or an empty string when the executable produces no output.</returns>
    ValueTask<string> GetVersion(CancellationToken cancellationToken = default);

    /// <summary>Reads dimensions, pixel representation, resolution, loader information, and available metadata.</summary>
    /// <param name="inputPath">The path of the image to inspect.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The parsed image header and metadata.</returns>
    ValueTask<ImageInfo> Identify(string inputPath, CancellationToken cancellationToken = default);

    /// <summary>Runs a reusable sequence of unary image operations and encodes the final result.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination path. Its extension selects the encoder.</param>
    /// <param name="pipeline">The ordered operations to execute.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Process(string inputPath, string outputPath, LibvipsPipeline pipeline, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Converts an image using the encoder selected by the destination extension.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Convert(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Converts an image to AVIF.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination path, which must end in <c>.avif</c>.</param>
    /// <param name="options">Optional AVIF encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask ConvertToAvif(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Converts an image to WebP.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination path, which must end in <c>.webp</c>.</param>
    /// <param name="options">Optional WebP encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask ConvertToWebp(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Resizes an image to fit within the requested bounds without enlarging it.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="width">The maximum output width.</param>
    /// <param name="height">The optional maximum output height.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Resize(string inputPath, string outputPath, int width, int? height = null, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Resizes an image using explicit size, crop, orientation, and linear-light settings.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="resizeOptions">The resize behavior.</param>
    /// <param name="outputOptions">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Resize(string inputPath, string outputPath, ResizeOptions resizeOptions, LibvipsOptions? outputOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Extracts a rectangular region from an image.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="left">The horizontal coordinate of the region's left edge.</param>
    /// <param name="top">The vertical coordinate of the region's top edge.</param>
    /// <param name="width">The region width.</param>
    /// <param name="height">The region height.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Crop(string inputPath, string outputPath, int left, int top, int width, int height,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Extracts a region selected using libvips' attention strategy.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask SmartCrop(string inputPath, string outputPath, int width, int height, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Extracts a region using the specified interestingness strategy.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <param name="interesting">The strategy used to select the retained region.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask SmartCrop(string inputPath, string outputPath, int width, int height, LibvipsInteresting interesting,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Rotates an image clockwise without resampling.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="angle">The clockwise rotation.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Rotate(string inputPath, string outputPath, LibvipsAngle angle, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Applies the image's EXIF orientation and removes the orientation metadata.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask AutoRotate(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Mirrors an image along the requested axis.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="direction">The mirror direction.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Flip(string inputPath, string outputPath, LibvipsDirection direction, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Applies a Gaussian blur.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="sigma">The Gaussian standard deviation, from 0 through 1000.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Blur(string inputPath, string outputPath, double sigma, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Sharpens an image using unsharp masking.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="sigma">The Gaussian standard deviation, greater than 0 and no more than 10.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Sharpen(string inputPath, string outputPath, double sigma = 0.5, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Applies a power-law gamma transform.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="exponent">The positive gamma exponent.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Gamma(string inputPath, string outputPath, double exponent = 1d / 2.4d, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Inverts every pixel value in an image.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Invert(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Flattens an alpha channel onto a background colour.</summary>
    /// <param name="inputPath">The source image path.</param>
    /// <param name="outputPath">The destination image path.</param>
    /// <param name="background">Optional background band values. libvips uses black when omitted.</param>
    /// <param name="options">Optional output encoding settings.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Flatten(string inputPath, string outputPath, double[]? background = null, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default);
}
