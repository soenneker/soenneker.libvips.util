using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Libvips.Util.Abstract;
using Soenneker.Libvips.Util.Commands;
using Soenneker.Libvips.Util.Commands.Abstract;
using Soenneker.Libvips.Util.Dtos;
using Soenneker.Libvips.Util.Enums;
using Soenneker.Libvips.Util.Options;
using Soenneker.Libvips.Util.Pipelines.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Path.Abstract;
using Soenneker.Utils.Process.Abstract;
using Soenneker.Utils.Runtime;

namespace Soenneker.Libvips.Util;

/// <inheritdoc cref="ILibvipsUtil"/>
public sealed class LibvipsUtil : ILibvipsUtil
{
    private readonly IDirectoryUtil _directoryUtil;
    private readonly IFileUtil _fileUtil;
    private readonly IPathUtil _pathUtil;
    private readonly IProcessUtil _processUtil;
    private readonly string _vipsBinaryPath;
    private readonly string _vipsHeaderBinaryPath;

    /// <summary>Creates a libvips utility using the registered process and filesystem services.</summary>
    /// <param name="processUtil">The process execution utility.</param>
    /// <param name="directoryUtil">The directory utility.</param>
    /// <param name="fileUtil">The file utility.</param>
    public LibvipsUtil(IProcessUtil processUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil)
        : this(processUtil, directoryUtil, fileUtil, new Soenneker.Utils.Path.PathUtil())
    {
    }

    /// <summary>Creates a libvips utility using the registered process, path, and filesystem services.</summary>
    /// <param name="processUtil">The process execution utility.</param>
    /// <param name="directoryUtil">The directory utility.</param>
    /// <param name="fileUtil">The file utility.</param>
    /// <param name="pathUtil">The path utility used to allocate unique temporary paths.</param>
    public LibvipsUtil(IProcessUtil processUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil, IPathUtil pathUtil)
    {
        _processUtil = processUtil ?? throw new ArgumentNullException(nameof(processUtil));
        _directoryUtil = directoryUtil ?? throw new ArgumentNullException(nameof(directoryUtil));
        _fileUtil = fileUtil ?? throw new ArgumentNullException(nameof(fileUtil));
        _pathUtil = pathUtil ?? throw new ArgumentNullException(nameof(pathUtil));

        EnsureSupportedPlatform();

        _vipsBinaryPath = RuntimeUtil.IsWindows()
            ? Path.Join(AppContext.BaseDirectory, "Resources", "win-x64", "libvips", "bin", "vips.exe")
            : Path.Join(AppContext.BaseDirectory, "Resources", "linux-x64", "libvips", "vips.sh");

        _vipsHeaderBinaryPath = RuntimeUtil.IsWindows()
            ? Path.Join(AppContext.BaseDirectory, "Resources", "win-x64", "libvips", "bin", "vipsheader.exe")
            : Path.Join(AppContext.BaseDirectory, "Resources", "linux-x64", "libvips", "vipsheader.sh");
    }

    public async ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arguments);

        return await RunExecutable(_vipsBinaryPath, arguments, workingDirectory, log, cancellationToken).NoSync();
    }

    public ValueTask<List<string>> Execute(ILibvipsCommand command, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        return RunExecutable(_vipsBinaryPath, LibvipsCommand.Build(command), workingDirectory, log, cancellationToken);
    }

    private async ValueTask<List<string>> RunExecutable(string executablePath, string arguments,
        string? workingDirectory, bool log, CancellationToken cancellationToken)
    {
        if (!await _fileUtil.Exists(executablePath, cancellationToken).NoSync())
            throw new FileNotFoundException("The bundled libvips executable was not found.", executablePath);

        if (OperatingSystem.IsLinux())
        {
            File.SetUnixFileMode(executablePath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead |
                UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);

            string executableName = Path.GetFileNameWithoutExtension(executablePath);
            string executable = Path.Join(Path.GetDirectoryName(executablePath), "bin", executableName);
            File.SetUnixFileMode(executable,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead |
                UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        }

        return await _processUtil.Start(executablePath, workingDirectory, arguments, log: log,
            cancellationToken: cancellationToken).NoSync();
    }


    public async ValueTask<string> GetVersion(CancellationToken cancellationToken = default)
    {
        List<string> output = await Run("--version", log: false, cancellationToken: cancellationToken).NoSync();
        return output.Count == 0 ? string.Empty : output[0];
    }


    public async ValueTask<ImageInfo> Identify(string inputPath, CancellationToken cancellationToken = default)
    {
        await ValidateInput(inputPath, cancellationToken).NoSync();
        string fullInputPath = Path.GetFullPath(inputPath);
        string arguments = LibvipsCommand.BuildArgumentString(["-a", fullInputPath]);
        List<string> output = await RunExecutable(_vipsHeaderBinaryPath, arguments, null, false, cancellationToken).NoSync();
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 1; index < output.Count; index++)
        {
            string line = output[index];
            int separator = line.IndexOf(':');
            if (separator <= 0)
                continue;

            string key = line[..separator].Trim();
            if (key.Contains(Path.DirectorySeparatorChar) || key.Contains(Path.AltDirectorySeparatorChar))
                continue;
            metadata[key] = line[(separator + 1)..].Trim();
        }

        int width = ParseMetadataInt(metadata, "width");
        int height = ParseMetadataInt(metadata, "height");
        int bands = ParseMetadataInt(metadata, "bands");

        var readOnlyMetadata = new ReadOnlyDictionary<string, string>(metadata);
        return new ImageInfo(width, height, bands, GetMetadata(metadata, "format"), GetMetadata(metadata, "coding"),
            GetMetadata(metadata, "interpretation"), ParseMetadataDouble(metadata, "xres"), ParseMetadataDouble(metadata, "yres"),
            GetMetadata(metadata, "vips-loader"), readOnlyMetadata);
    }


    public async ValueTask Convert(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await ValidateInput(inputPath, cancellationToken).NoSync();
        ValidateOutput(outputPath);
        options ??= new LibvipsOptions();
        options.Validate();

        string fullOutputPath = Path.GetFullPath(outputPath);
        await _directoryUtil.Create(Path.GetDirectoryName(fullOutputPath)!, cancellationToken: cancellationToken).NoSync();
        string temporaryOutputPath = await CreateTemporaryOutputPath(fullOutputPath, cancellationToken).NoSync();

        try
        {
            ILibvipsCommand command = new LibvipsCommand("copy")
                                     .AddArgument(Path.GetFullPath(inputPath))
                                     .AddArgument(BuildOutputSpec(temporaryOutputPath, options));
            await Execute(command, cancellationToken: cancellationToken).NoSync();
            await CommitOutput(temporaryOutputPath, fullOutputPath, cancellationToken).NoSync();
        }
        finally
        {
            await _fileUtil.TryDeleteIfExists(temporaryOutputPath, log: false, CancellationToken.None).NoSync();
        }
    }

    public async ValueTask Process(string inputPath, string outputPath, ILibvipsPipeline pipeline,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        IReadOnlyList<ILibvipsPipelineStep> steps = await pipeline.GetSteps(cancellationToken).NoSync();
        if (steps.Count == 0)
        {
            await Convert(inputPath, outputPath, options, cancellationToken).NoSync();
            return;
        }

        await ValidateInput(inputPath, cancellationToken).NoSync();
        ValidateOutput(outputPath);
        options ??= new LibvipsOptions();
        options.Validate();

        string fullOutputPath = Path.GetFullPath(outputPath);
        await _directoryUtil.Create(Path.GetDirectoryName(fullOutputPath)!, cancellationToken: cancellationToken).NoSync();
        string temporaryDirectory = await _pathUtil.GetUniqueTempDirectory("soenneker-libvips", cancellationToken: cancellationToken).NoSync();
        string temporaryOutputPath = await CreateTemporaryOutputPath(fullOutputPath, cancellationToken).NoSync();

        try
        {
            string currentInput = Path.GetFullPath(inputPath);
            var commands = new List<ILibvipsCommand>(steps.Count);

            for (var index = 0; index < steps.Count; index++)
            {
                ILibvipsPipelineStep step = steps[index];
                bool isLast = index == steps.Count - 1;
                string currentOutput = isLast
                    ? BuildOutputSpec(temporaryOutputPath, options)
                    : Path.Combine(temporaryDirectory, $"{index}.v");

                ILibvipsCommand command = new LibvipsCommand(step.Operation).AddArgument(currentInput).AddArgument(currentOutput);
                step.Configure?.Invoke(command);
                commands.Add(command);
                currentInput = currentOutput;
            }

            foreach (ILibvipsCommand command in commands)
                await Execute(command, log: false, cancellationToken: cancellationToken).NoSync();

            await CommitOutput(temporaryOutputPath, fullOutputPath, cancellationToken).NoSync();
        }
        finally
        {
            await _fileUtil.TryDeleteIfExists(temporaryOutputPath, log: false, CancellationToken.None).NoSync();
            await _directoryUtil.DeleteIfExists(temporaryDirectory, CancellationToken.None).NoSync();
        }
    }


    public ValueTask ConvertToAvif(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ValidateExtension(outputPath, ".avif");
        return Convert(inputPath, outputPath, options, cancellationToken);
    }


    public ValueTask ConvertToWebp(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ValidateExtension(outputPath, ".webp");
        return Convert(inputPath, outputPath, options, cancellationToken);
    }


    public async ValueTask Resize(string inputPath, string outputPath, int width, int? height = null,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Resize(inputPath, outputPath, new ResizeOptions {Width = width, Height = height}, options, cancellationToken).NoSync();
    }

    public async ValueTask Resize(string inputPath, string outputPath, ResizeOptions resizeOptions,
        LibvipsOptions? outputOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resizeOptions);
        resizeOptions.Validate();

        await ExecuteImageOperation("thumbnail", inputPath, outputPath, outputOptions, command =>
        {
            command.AddArgument(resizeOptions.Width);
            if (resizeOptions.Height.HasValue)
                command.AddOption("height", resizeOptions.Height.Value);
            command.AddOption("size", resizeOptions.Size.Value);
            if (resizeOptions.Crop != LibvipsInteresting.None)
                command.AddOption("crop", resizeOptions.Crop.Value);
            command.AddFlag("no-rotate", !resizeOptions.AutoRotate);
            command.AddFlag("linear", resizeOptions.LinearLight);
        }, cancellationToken).NoSync();
    }

    public ValueTask Crop(string inputPath, string outputPath, int left, int top, int width, int height,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), "Left must be zero or greater.");
        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top), "Top must be zero or greater.");
        ValidateDimensions(width, height);
        return ExecuteImageOperation("crop", inputPath, outputPath, options,
            command => command.AddArgument(left).AddArgument(top).AddArgument(width).AddArgument(height), cancellationToken);
    }

    public ValueTask SmartCrop(string inputPath, string outputPath, int width, int height,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default) =>
        SmartCrop(inputPath, outputPath, width, height, LibvipsInteresting.Attention, options, cancellationToken);

    public ValueTask SmartCrop(string inputPath, string outputPath, int width, int height,
        LibvipsInteresting interesting, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(interesting);
        ValidateDimensions(width, height);
        return ExecuteImageOperation("smartcrop", inputPath, outputPath, options,
            command => command.AddArgument(width).AddArgument(height).AddOption("interesting", interesting.Value), cancellationToken);
    }

    public ValueTask Rotate(string inputPath, string outputPath, LibvipsAngle angle, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(angle);
        return ExecuteImageOperation("rot", inputPath, outputPath, options, command => command.AddArgument(angle.Value), cancellationToken);
    }

    public ValueTask AutoRotate(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ExecuteImageOperation("autorot", inputPath, outputPath, options, null, cancellationToken);

    public ValueTask Flip(string inputPath, string outputPath, LibvipsDirection direction, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(direction);
        return ExecuteImageOperation("flip", inputPath, outputPath, options, command => command.AddArgument(direction.Value), cancellationToken);
    }

    public ValueTask Blur(string inputPath, string outputPath, double sigma, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!double.IsFinite(sigma) || sigma is < 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be between 0 and 1000.");
        return ExecuteImageOperation("gaussblur", inputPath, outputPath, options, command => command.AddArgument(sigma), cancellationToken);
    }

    public ValueTask Sharpen(string inputPath, string outputPath, double sigma = 0.5, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!double.IsFinite(sigma) || sigma is <= 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be greater than zero and no more than 10.");
        return ExecuteImageOperation("sharpen", inputPath, outputPath, options, command => command.AddOption("sigma", sigma), cancellationToken);
    }

    public ValueTask Gamma(string inputPath, string outputPath, double exponent = 1d / 2.4d,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (!double.IsFinite(exponent) || exponent is <= 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be greater than zero and no more than 1000.");
        return ExecuteImageOperation("gamma", inputPath, outputPath, options, command => command.AddOption("exponent", exponent), cancellationToken);
    }

    public ValueTask Invert(string inputPath, string outputPath, LibvipsOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ExecuteImageOperation("invert", inputPath, outputPath, options, null, cancellationToken);

    public ValueTask Flatten(string inputPath, string outputPath, double[]? background = null,
        LibvipsOptions? options = null, CancellationToken cancellationToken = default)
    {
        double[]? values = background is null ? null : [.. background];
        if (values is not null)
            ValidateBackground(values);

        return ExecuteImageOperation("flatten", inputPath, outputPath, options,
            values is not {Length: > 0} ? null : command => command.AddOption("background",
                string.Join(',', Array.ConvertAll(values, value => value.ToString(CultureInfo.InvariantCulture)))), cancellationToken);
    }

    private async ValueTask ExecuteImageOperation(string operation, string inputPath, string outputPath,
        LibvipsOptions? options, Action<ILibvipsCommand>? configure, CancellationToken cancellationToken)
    {
        await ValidateInput(inputPath, cancellationToken).NoSync();
        ValidateOutput(outputPath);
        options ??= new LibvipsOptions();
        options.Validate();

        string fullOutputPath = Path.GetFullPath(outputPath);
        await _directoryUtil.Create(Path.GetDirectoryName(fullOutputPath)!, cancellationToken: cancellationToken).NoSync();
        string temporaryOutputPath = await CreateTemporaryOutputPath(fullOutputPath, cancellationToken).NoSync();

        try
        {
            ILibvipsCommand command = new LibvipsCommand(operation)
                                     .AddArgument(Path.GetFullPath(inputPath))
                                     .AddArgument(BuildOutputSpec(temporaryOutputPath, options));
            configure?.Invoke(command);
            await Execute(command, cancellationToken: cancellationToken).NoSync();
            await CommitOutput(temporaryOutputPath, fullOutputPath, cancellationToken).NoSync();
        }
        finally
        {
            await _fileUtil.TryDeleteIfExists(temporaryOutputPath, log: false, CancellationToken.None).NoSync();
        }
    }

    private static string BuildOutputSpec(string outputPath, LibvipsOptions options)
    {
        string extension = Path.GetExtension(outputPath).ToLowerInvariant();
        var values = new List<string>();

        switch (extension)
        {
            case ".avif":
            case ".heif":
            case ".heic":
            case ".webp":
                values.Add($"Q={options.Quality}");
                values.Add($"effort={options.Effort}");
                values.Add($"lossless={options.Lossless.ToString().ToLowerInvariant()}");
                values.Add($"strip={options.StripMetadata.ToString().ToLowerInvariant()}");
                break;
            case ".jpg":
            case ".jpeg":
                values.Add($"Q={options.Quality}");
                values.Add($"strip={options.StripMetadata.ToString().ToLowerInvariant()}");
                values.Add($"interlace={options.Progressive.ToString().ToLowerInvariant()}");
                values.Add($"optimize-coding={options.OptimizeCoding.ToString().ToLowerInvariant()}");
                break;
            case ".png":
                values.Add($"compression={options.Compression}");
                values.Add($"strip={options.StripMetadata.ToString().ToLowerInvariant()}");
                values.Add($"interlace={options.Progressive.ToString().ToLowerInvariant()}");
                break;
            case ".tif":
            case ".tiff":
                values.Add($"Q={options.Quality}");
                values.Add($"strip={options.StripMetadata.ToString().ToLowerInvariant()}");
                break;
        }

        return values.Count == 0 ? outputPath : $"{outputPath}[{string.Join(',', values)}]";
    }

    private ValueTask<string> CreateTemporaryOutputPath(string outputPath, CancellationToken cancellationToken)
    {
        string directory = Path.GetDirectoryName(outputPath)!;
        string extension = Path.GetExtension(outputPath);
        return _pathUtil.GetRandomUniqueFilePath(directory, extension, cancellationToken);
    }

    private ValueTask CommitOutput(string temporaryOutputPath, string outputPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _fileUtil.Move(temporaryOutputPath, outputPath, log: false, cancellationToken);
    }

    private static void ValidateBackground(double[] background)
    {
        for (var index = 0; index < background.Length; index++)
        {
            if (!double.IsFinite(background[index]))
                throw new ArgumentOutOfRangeException(nameof(background), "Background values must be finite numbers.");
        }
    }

    private static int ParseMetadataInt(IReadOnlyDictionary<string, string> metadata, string field)
    {
        if (!metadata.TryGetValue(field, out string? text) ||
            !int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            throw new InvalidOperationException($"vips did not return a valid {field} value.");
        return value;
    }

    private static double? ParseMetadataDouble(IReadOnlyDictionary<string, string> metadata, string field) =>
        metadata.TryGetValue(field, out string? text) && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : null;

    private static string? GetMetadata(IReadOnlyDictionary<string, string> metadata, string field) =>
        metadata.GetValueOrDefault(field);

    private static void ValidateDimensions(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
    }

    private async ValueTask ValidateInput(string inputPath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        if (!await _fileUtil.Exists(inputPath, cancellationToken).NoSync())
            throw new FileNotFoundException("The input image does not exist.", inputPath);
    }

    private static void ValidateOutput(string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        string extension = Path.GetExtension(outputPath);
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("The output path must have an extension so libvips can select an encoder.", nameof(outputPath));
    }

    private static void ValidateExtension(string outputPath, string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (!Path.GetExtension(outputPath).Equals(extension, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"The output path must use the {extension} extension.", nameof(outputPath));
    }

    private static void EnsureSupportedPlatform()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 ||
            (!RuntimeUtil.IsLinux() && !RuntimeUtil.IsWindows()))
            throw new PlatformNotSupportedException(
                "Soenneker.Libvips.Util currently supports Linux x64 and Windows x64.");
    }
}
