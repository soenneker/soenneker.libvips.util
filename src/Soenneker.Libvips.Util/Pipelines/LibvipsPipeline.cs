using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Asyncs.Locks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Libvips.Util.Commands;
using Soenneker.Libvips.Util.Commands.Abstract;
using Soenneker.Libvips.Util.Enums;
using Soenneker.Libvips.Util.Pipelines.Abstract;

namespace Soenneker.Libvips.Util.Pipelines;

public sealed class LibvipsPipeline : ILibvipsPipeline
{
    private readonly List<ILibvipsPipelineStep> _steps = [];
    private readonly AsyncLock _lock = new();

    public async ValueTask<int> GetCount(CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken).NoSync())
            return _steps.Count;
    }

    public async ValueTask<IReadOnlyList<ILibvipsPipelineStep>> GetSteps(CancellationToken cancellationToken = default)
    {
        using (await _lock.Lock(cancellationToken).NoSync())
            return [.. _steps];
    }

    public ILibvipsPipeline Add(string operation, Action<ILibvipsCommand>? configure = null)
    {
        // Validate eagerly rather than failing after earlier pipeline steps have run.
        _ = new LibvipsCommand(operation);
        using (_lock.LockSync())
            _steps.Add(new Step(operation, configure));
        return this;
    }

    public ILibvipsPipeline Crop(int left, int top, int width, int height)
    {
        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), "Left must be zero or greater.");
        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top), "Top must be zero or greater.");
        ValidateDimensions(width, height);
        return Add("crop", command => command.AddArgument(left).AddArgument(top).AddArgument(width).AddArgument(height));
    }

    public ILibvipsPipeline SmartCrop(int width, int height) => SmartCrop(width, height, LibvipsInteresting.Attention);

    public ILibvipsPipeline SmartCrop(int width, int height, LibvipsInteresting interesting)
    {
        ArgumentNullException.ThrowIfNull(interesting);
        ValidateDimensions(width, height);
        return Add("smartcrop", command => command.AddArgument(width).AddArgument(height)
            .AddOption("interesting", interesting.Value));
    }

    public ILibvipsPipeline Rotate(LibvipsAngle angle)
    {
        ArgumentNullException.ThrowIfNull(angle);
        return Add("rot", command => command.AddArgument(angle.Value));
    }

    public ILibvipsPipeline AutoRotate() => Add("autorot");

    public ILibvipsPipeline Flip(LibvipsDirection direction)
    {
        ArgumentNullException.ThrowIfNull(direction);
        return Add("flip", command => command.AddArgument(direction.Value));
    }

    public ILibvipsPipeline Blur(double sigma)
    {
        if (!double.IsFinite(sigma) || sigma is < 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be between 0 and 1000.");
        return Add("gaussblur", command => command.AddArgument(sigma));
    }

    public ILibvipsPipeline Sharpen(double sigma = 0.5)
    {
        if (!double.IsFinite(sigma) || sigma is <= 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be greater than zero and no more than 10.");
        return Add("sharpen", command => command.AddOption("sigma", sigma));
    }

    public ILibvipsPipeline Gamma(double exponent = 1d / 2.4d)
    {
        if (!double.IsFinite(exponent) || exponent is <= 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be greater than zero and no more than 1000.");
        return Add("gamma", command => command.AddOption("exponent", exponent));
    }

    public ILibvipsPipeline Invert() => Add("invert");

    public ILibvipsPipeline Flatten(params double[] background)
    {
        ArgumentNullException.ThrowIfNull(background);
        double[] values = [.. background];
        ValidateBackground(values);

        return Add("flatten", values.Length == 0 ? null : command => command.AddOption("background",
            string.Join(',', Array.ConvertAll(values, value => value.ToString(CultureInfo.InvariantCulture)))));
    }

    private sealed record Step(string Operation, Action<ILibvipsCommand>? Configure) : ILibvipsPipelineStep;

    public void Dispose() => _lock.Dispose();

    public ValueTask DisposeAsync() => _lock.DisposeAsync();

    private static void ValidateBackground(double[] background)
    {
        for (var index = 0; index < background.Length; index++)
        {
            if (!double.IsFinite(background[index]))
                throw new ArgumentOutOfRangeException(nameof(background), "Background values must be finite numbers.");
        }
    }

    private static void ValidateDimensions(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
    }
}
