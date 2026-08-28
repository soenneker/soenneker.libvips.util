using System;
using System.Collections.Generic;
using System.Globalization;
using Soenneker.Libvips.Util.Commands;
using Soenneker.Libvips.Util.Enums;

namespace Soenneker.Libvips.Util.Pipelines;

/// <summary>
/// A reusable, fluent sequence of image operations. Steps use the native VIPS format between operations and encode
/// only the final result.
/// </summary>
public sealed class LibvipsPipeline
{
    internal readonly List<Step> Steps = [];

    /// <summary>Adds a unary libvips operation whose first arguments are the input and output images.</summary>
    /// <param name="operation">The libvips operation nickname.</param>
    /// <param name="configure">An optional callback that adds positional arguments and named options.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Add(string operation, Action<LibvipsCommand>? configure = null)
    {
        // Validate eagerly rather than failing after earlier pipeline steps have run.
        _ = new LibvipsCommand(operation);
        Steps.Add(new Step(operation, configure));
        return this;
    }

    /// <summary>Adds an operation that extracts a rectangular region.</summary>
    /// <param name="left">The horizontal coordinate of the region's left edge.</param>
    /// <param name="top">The vertical coordinate of the region's top edge.</param>
    /// <param name="width">The region width.</param>
    /// <param name="height">The region height.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Crop(int left, int top, int width, int height)
    {
        ValidateDimensions(width, height);
        return Add("crop", command => command.AddArgument(left).AddArgument(top).AddArgument(width).AddArgument(height));
    }

    /// <summary>Adds an attention-based smart crop operation.</summary>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline SmartCrop(int width, int height) => SmartCrop(width, height, LibvipsInteresting.Attention);

    /// <summary>Adds a smart crop operation using the specified selection strategy.</summary>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <param name="interesting">The strategy used to select the retained region.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline SmartCrop(int width, int height, LibvipsInteresting interesting)
    {
        ArgumentNullException.ThrowIfNull(interesting);
        ValidateDimensions(width, height);
        return Add("smartcrop", command => command.AddArgument(width).AddArgument(height)
            .AddOption("interesting", interesting.Value));
    }

    /// <summary>Adds a lossless right-angle rotation.</summary>
    /// <param name="angle">The clockwise rotation.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Rotate(LibvipsAngle angle)
    {
        ArgumentNullException.ThrowIfNull(angle);
        return Add("rot", command => command.AddArgument(angle.Value));
    }

    /// <summary>Adds an operation that applies EXIF orientation.</summary>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline AutoRotate() => Add("autorot");

    /// <summary>Adds an operation that mirrors the image along an axis.</summary>
    /// <param name="direction">The mirror direction.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Flip(LibvipsDirection direction)
    {
        ArgumentNullException.ThrowIfNull(direction);
        return Add("flip", command => command.AddArgument(direction.Value));
    }

    /// <summary>Adds a Gaussian blur operation.</summary>
    /// <param name="sigma">The Gaussian standard deviation, from 0 through 1000.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Blur(double sigma)
    {
        if (sigma is < 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be between 0 and 1000.");
        return Add("gaussblur", command => command.AddArgument(sigma));
    }

    /// <summary>Adds an unsharp-mask operation.</summary>
    /// <param name="sigma">The Gaussian standard deviation, greater than 0 and no more than 10.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Sharpen(double sigma = 0.5)
    {
        if (sigma is <= 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be greater than zero and no more than 10.");
        return Add("sharpen", command => command.AddOption("sigma", sigma));
    }

    /// <summary>Adds a power-law gamma transform.</summary>
    /// <param name="exponent">The positive gamma exponent.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Gamma(double exponent = 1d / 2.4d)
    {
        if (exponent is <= 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be greater than zero and no more than 1000.");
        return Add("gamma", command => command.AddOption("exponent", exponent));
    }

    /// <summary>Adds an operation that inverts every pixel value.</summary>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Invert() => Add("invert");

    /// <summary>Adds an operation that flattens alpha onto a background colour.</summary>
    /// <param name="background">Optional background band values. libvips uses black when empty.</param>
    /// <returns>This pipeline, enabling fluent chaining.</returns>
    public LibvipsPipeline Flatten(params double[] background) => Add("flatten",
        background is null ? throw new ArgumentNullException(nameof(background)) : background.Length == 0 ? null : command => command.AddOption("background",
            string.Join(',', Array.ConvertAll(background, value => value.ToString(CultureInfo.InvariantCulture)))));

    internal sealed record Step(string Operation, Action<LibvipsCommand>? Configure);

    private static void ValidateDimensions(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
    }
}
