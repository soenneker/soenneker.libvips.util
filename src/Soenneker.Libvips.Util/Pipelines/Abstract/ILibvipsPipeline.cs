using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Libvips.Util.Commands.Abstract;
using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Pipelines.Abstract;

/// <summary>
/// A reusable, fluent sequence of image operations. Steps use the native VIPS format between operations and encode
/// only the final result.
/// </summary>
public interface ILibvipsPipeline : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the number of operations in this pipeline.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel lock acquisition.</param>
    /// <returns>A task whose result is the requested value.</returns>
    ValueTask<int> GetCount(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a read-only snapshot of the configured operations.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel lock acquisition.</param>
    /// <returns>A task whose result is the collection returned by get Steps.</returns>
    ValueTask<IReadOnlyList<ILibvipsPipelineStep>> GetSteps(CancellationToken cancellationToken = default);

    /// <summary>Adds a unary libvips operation whose first arguments are the input and output images.</summary>
    /// <param name="operation">The libvips operation nickname.</param>
    /// <param name="configure">An optional callback that adds positional arguments and named options.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Add(string operation, Action<ILibvipsCommand>? configure = null);

    /// <summary>Adds an operation that extracts a rectangular region.</summary>
    /// <param name="left">The horizontal coordinate of the region's left edge.</param>
    /// <param name="top">The vertical coordinate of the region's top edge.</param>
    /// <param name="width">The region width.</param>
    /// <param name="height">The region height.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Crop(int left, int top, int width, int height);

    /// <summary>Adds an attention-based smart crop operation.</summary>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline SmartCrop(int width, int height);

    /// <summary>Adds a smart crop operation using the specified selection strategy.</summary>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <param name="interesting">The strategy used to select the retained region.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline SmartCrop(int width, int height, LibvipsInteresting interesting);

    /// <summary>Adds a lossless right-angle rotation.</summary>
    /// <param name="angle">The clockwise rotation.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Rotate(LibvipsAngle angle);

    /// <summary>Adds an operation that applies EXIF orientation.</summary>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline AutoRotate();

    /// <summary>Adds an operation that mirrors the image along an axis.</summary>
    /// <param name="direction">The mirror direction.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Flip(LibvipsDirection direction);

    /// <summary>Adds a Gaussian blur operation.</summary>
    /// <param name="sigma">The Gaussian standard deviation, from 0 through 1000.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Blur(double sigma);

    /// <summary>Adds an unsharp-mask operation.</summary>
    /// <param name="sigma">The Gaussian standard deviation, greater than 0 and no more than 10.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Sharpen(double sigma = 0.5);

    /// <summary>Adds a power-law gamma transform.</summary>
    /// <param name="exponent">The positive gamma exponent.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Gamma(double exponent = 1d / 2.4d);

    /// <summary>Adds an operation that inverts every pixel value.</summary>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Invert();

    /// <summary>Adds an operation that flattens alpha onto a background colour.</summary>
    /// <param name="background">Optional background band values. libvips uses black when empty.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    ILibvipsPipeline Flatten(params double[] background);
}

/// <summary>A configured operation in a libvips pipeline.</summary>
public interface ILibvipsPipelineStep
{
    /// <summary>Gets the libvips operation nickname.</summary>
    string Operation { get; }

    /// <summary>Gets the optional command configuration callback.</summary>
    Action<ILibvipsCommand>? Configure { get; }
}
