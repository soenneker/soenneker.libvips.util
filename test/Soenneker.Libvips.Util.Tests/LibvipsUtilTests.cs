using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Extensions.ValueTask;
using Soenneker.Libvips.Util.Abstract;
using Soenneker.Libvips.Util.Commands;
using Soenneker.Libvips.Util.Commands.Abstract;
using Soenneker.Libvips.Util.Enums;
using Soenneker.Libvips.Util.Pipelines;
using Soenneker.Libvips.Util.Pipelines.Abstract;
using Soenneker.Libvips.Util.Registrars;
using Soenneker.Utils.Path;

namespace Soenneker.Libvips.Util.Tests;

public sealed class LibvipsUtilTests
{
    private const string Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Test]
    public async Task Converts_image_to_webp_and_avif(CancellationToken cancellationToken)
    {
        string directory = await new PathUtil().GetUniqueTempDirectory("soenneker libvips test");
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(directory, "input.png");
            string webp = Path.Combine(directory, "output.webp");
            string avif = Path.Combine(directory, "output.avif");
            await File.WriteAllBytesAsync(input, Convert.FromBase64String(Png));

            await util.ConvertToWebp(input, webp, cancellationToken: cancellationToken).NoSync();
            await util.ConvertToAvif(input, avif, cancellationToken: cancellationToken).NoSync();

            Dtos.ImageInfo webpInfo = await util.Identify(webp, cancellationToken: cancellationToken).NoSync();
            Dtos.ImageInfo avifInfo = await util.Identify(avif, cancellationToken: cancellationToken).NoSync();

            if (webpInfo.Width != 1 || webpInfo.Height != 1 || avifInfo.Width != 1 || avifInfo.Height != 1)
                throw new InvalidOperationException("The generated image dimensions are incorrect.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Converts_image_to_jpeg_and_png(CancellationToken cancellationToken)
    {
        string directory = await new PathUtil().GetUniqueTempDirectory("soenneker libvips test");
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(directory, "input.png");
            string jpeg = Path.Combine(directory, "output.jpeg");
            string png = Path.Combine(directory, "output.png");
            await File.WriteAllBytesAsync(input, Convert.FromBase64String(Png), cancellationToken);

            await util.ConvertToJpeg(input, jpeg, cancellationToken: cancellationToken).NoSync();
            await util.ConvertToPng(jpeg, png, cancellationToken: cancellationToken).NoSync();
            Dtos.ImageInfo jpegInfo = await util.Identify(jpeg, cancellationToken).NoSync();
            Dtos.ImageInfo pngInfo = await util.Identify(png, cancellationToken).NoSync();

            if (jpegInfo.Width != 1 || jpegInfo.Height != 1 || pngInfo.Width != 1 || pngInfo.Height != 1)
                throw new InvalidOperationException("The converted image dimensions are incorrect.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Resizes_image_to_webp(CancellationToken cancellationToken)
    {
        string directory = await new PathUtil().GetUniqueTempDirectory("soenneker libvips test");
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(AppContext.BaseDirectory, "icon.png");
            string output = Path.Combine(directory, "resized.webp");

            await util.Resize(input, output, 32, 32).NoSync();
            Dtos.ImageInfo info = await util.Identify(output, cancellationToken: cancellationToken).NoSync();

            if (info.Width > 32 || info.Height > 32)
                throw new InvalidOperationException("The resized image exceeds the requested bounds.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Processes_a_typed_pipeline_and_reads_metadata(CancellationToken cancellationToken)
    {
        string directory = await new PathUtil().GetUniqueTempDirectory("soenneker libvips test");
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(AppContext.BaseDirectory, "icon.png");
            string output = Path.Combine(directory, "pipeline.png");
            using ILibvipsPipeline pipeline = new LibvipsPipeline()
                .Rotate(LibvipsAngle.D90)
                .Blur(0.5)
                .Invert();

            if (await pipeline.GetCount(cancellationToken: cancellationToken).NoSync() != 3 || (await pipeline.GetSteps(cancellationToken: cancellationToken).NoSync()).Count != 3)
                throw new InvalidOperationException("The pipeline snapshot is incomplete.");

            await util.Process(input, output, pipeline, cancellationToken: cancellationToken).NoSync();
            Dtos.ImageInfo info = await util.Identify(output, cancellationToken: cancellationToken).NoSync();

            if (info.Width != 128 || info.Height != 128 || info.Format is null || info.Loader is null || info.Metadata is null)
                throw new InvalidOperationException("The pipeline output metadata is incomplete.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Preserves_existing_output_when_encoding_fails(CancellationToken cancellationToken)
    {
        string directory = await new PathUtil().GetUniqueTempDirectory("soenneker libvips test");
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();
        byte[] originalOutput = [1, 2, 3, 4];

        try
        {
            string input = Path.Combine(directory, "invalid.png");
            string output = Path.Combine(directory, "existing.webp");
            await File.WriteAllTextAsync(input, "not an image");
            await File.WriteAllBytesAsync(output, originalOutput);

            try
            {
                await util.ConvertToWebp(input, output, cancellationToken: cancellationToken).NoSync();
                throw new InvalidOperationException("The invalid image unexpectedly converted successfully.");
            }
            catch (InvalidOperationException exception) when (!exception.Message.Contains("unexpectedly", StringComparison.Ordinal))
            {
            }

            byte[] currentOutput = await File.ReadAllBytesAsync(output);
            if (!currentOutput.AsSpan().SequenceEqual(originalOutput))
                throw new InvalidOperationException("A failed conversion modified the existing output.");

            if (Directory.GetFiles(directory).Length != 2)
                throw new InvalidOperationException("A failed conversion left a temporary output behind.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Safely_replaces_an_image_in_place(CancellationToken cancellationToken)
    {
        string directory = await new PathUtil().GetUniqueTempDirectory("soenneker libvips test");
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string path = Path.Combine(directory, "image.png");
            await File.WriteAllBytesAsync(path, Convert.FromBase64String(Png));

            await util.Convert(path, path, cancellationToken: cancellationToken).NoSync();
            Dtos.ImageInfo info = await util.Identify(path, cancellationToken: cancellationToken).NoSync();

            if (info.Width != 1 || info.Height != 1 || info.PixelCount != 1 || info.AspectRatio != 1)
                throw new InvalidOperationException("The in-place conversion produced an invalid image.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void Structured_command_exposes_a_read_only_diagnostic_view()
    {
        ILibvipsCommand command = new LibvipsCommand("copy")
                                 .AddArgument(@"C:\images with spaces\input.png")
                                 .AddArgument(@"C:\images with spaces\output.webp")
                                 .AddOption("Q", 80)
                                 .AddFlag("strip");

        if (command.Arguments.Count != 2 || command.Options.Count != 2 || !command.ToString().Contains("--Q 80", StringComparison.Ordinal))
            throw new InvalidOperationException("The structured command diagnostic view is incomplete.");

        if (command.Arguments is System.Collections.Generic.IList<string> mutableArguments && !mutableArguments.IsReadOnly)
            throw new InvalidOperationException("The public argument view must be read-only.");
    }

    [Test]
    public async Task Rejects_invalid_custom_commands(CancellationToken cancellationToken)
    {
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            _ = util.Execute(new InvalidCommand(), cancellationToken: cancellationToken);
            throw new InvalidOperationException("The invalid custom command was accepted.");
        }
        catch (InvalidOperationException exception) when (!exception.Message.Contains("was accepted", StringComparison.Ordinal))
        {
        }
    }

}
