using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Extensions.ValueTask;
using Soenneker.Libvips.Util.Abstract;
using Soenneker.Libvips.Util.Enums;
using Soenneker.Libvips.Util.Pipelines;
using Soenneker.Libvips.Util.Registrars;

namespace Soenneker.Libvips.Util.Tests;

public sealed class LibvipsUtilTests
{
    private const string Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Test]
    public async Task Converts_image_to_webp_and_avif()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-libvips-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(directory, "input.png");
            string webp = Path.Combine(directory, "output.webp");
            string avif = Path.Combine(directory, "output.avif");
            await File.WriteAllBytesAsync(input, Convert.FromBase64String(Png));

            await util.ConvertToWebp(input, webp).NoSync();
            await util.ConvertToAvif(input, avif).NoSync();

            Dtos.ImageInfo webpInfo = await util.Identify(webp).NoSync();
            Dtos.ImageInfo avifInfo = await util.Identify(avif).NoSync();

            if (webpInfo.Width != 1 || webpInfo.Height != 1 || avifInfo.Width != 1 || avifInfo.Height != 1)
                throw new InvalidOperationException("The generated image dimensions are incorrect.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Resizes_image_to_webp()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-libvips-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(AppContext.BaseDirectory, "icon.png");
            string output = Path.Combine(directory, "resized.webp");

            await util.Resize(input, output, 32, 32).NoSync();
            Dtos.ImageInfo info = await util.Identify(output).NoSync();

            if (info.Width > 32 || info.Height > 32)
                throw new InvalidOperationException("The resized image exceeds the requested bounds.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Processes_a_typed_pipeline_and_reads_metadata()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-libvips-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        await using ServiceProvider provider = new ServiceCollection().AddLogging().AddLibvipsUtilAsSingleton().BuildServiceProvider();
        ILibvipsUtil util = provider.GetRequiredService<ILibvipsUtil>();

        try
        {
            string input = Path.Combine(AppContext.BaseDirectory, "icon.png");
            string output = Path.Combine(directory, "pipeline.png");
            var pipeline = new LibvipsPipeline()
                .Rotate(LibvipsAngle.D90)
                .Blur(0.5)
                .Invert();

            await util.Process(input, output, pipeline).NoSync();
            Dtos.ImageInfo info = await util.Identify(output).NoSync();

            if (info.Width != 128 || info.Height != 128 || info.Format is null || info.Loader is null || info.Metadata is null)
                throw new InvalidOperationException("The pipeline output metadata is incomplete.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
