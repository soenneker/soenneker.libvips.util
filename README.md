[![](https://img.shields.io/nuget/v/soenneker.libvips.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libvips.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libvips.util/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.libvips.util/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libvips.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.libvips.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.libvips.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libvips.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libvips.util/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.libvips.util/actions/workflows/codeql.yml)

# Soenneker.Libvips.Util

### A cross-platform .NET API for the bundled libvips command-line distributions.

## Quick start

Install the package:

```bash
dotnet add package Soenneker.Libvips.Util
```

Register it and convert an image:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Libvips.Util.Abstract;
using Soenneker.Libvips.Util.Registrars;

await using ServiceProvider provider = new ServiceCollection()
    .AddLogging()
    .AddLibvipsUtilAsSingleton()
    .BuildServiceProvider();

ILibvipsUtil libvips = provider.GetRequiredService<ILibvipsUtil>();

await libvips.ConvertToWebp("images/photo.jpg", "images/photo.webp");
```

That is all the setup required. The package includes libvips for Windows x64 and Linux x64.

## Everyday operations

The output extension selects the image format.

### Convert

```csharp
await libvips.Convert("photo.tif", "photo.jpg");
await libvips.ConvertToAvif("photo.jpg", "photo.avif");
await libvips.ConvertToWebp("photo.jpg", "photo.webp");
```

### Resize

```csharp
await libvips.Resize("photo.jpg", "small.webp", width: 1200);
await libvips.Resize("photo.jpg", "thumbnail.webp", width: 400, height: 400);
```

Images are not enlarged unless you explicitly request it.

### Crop

```csharp
await libvips.Crop("photo.jpg", "crop.jpg", left: 20, top: 20, width: 800, height: 600);
await libvips.SmartCrop("photo.jpg", "avatar.webp", width: 512, height: 512);
```

### Transform

```csharp
using Soenneker.Libvips.Util.Enums;

await libvips.AutoRotate("phone-photo.jpg", "upright.jpg");
await libvips.Rotate("photo.jpg", "rotated.jpg", LibvipsAngle.D90);
await libvips.Flip("photo.jpg", "mirrored.jpg", LibvipsDirection.Horizontal);
await libvips.Blur("photo.jpg", "blurred.jpg", sigma: 2.5);
await libvips.Sharpen("photo.jpg", "sharpened.jpg");
await libvips.Gamma("photo.jpg", "corrected.jpg");
await libvips.Invert("photo.jpg", "inverted.jpg");
await libvips.Flatten("transparent.png", "flat.jpg", [255, 255, 255]);
```

## Control the output

Pass `LibvipsOptions` when you need more than the defaults:

```csharp
using Soenneker.Libvips.Util.Options;

await libvips.ConvertToAvif("photo.jpg", "photo.avif", new LibvipsOptions
{
    Quality = 85,
    Effort = 5,
    StripMetadata = true
});
```

Available controls include quality, encoder effort, lossless encoding, metadata stripping, progressive output, JPEG optimization, and PNG compression.

## Advanced resizing

Use `ResizeOptions` to control enlargement, cropping, orientation, and linear-light resizing:

```csharp
using Soenneker.Libvips.Util.Enums;
using Soenneker.Libvips.Util.Options;

await libvips.Resize("photo.jpg", "cover.webp", new ResizeOptions
{
    Width = 1200,
    Height = 630,
    Size = LibvipsSize.Both,
    Crop = LibvipsInteresting.Attention,
    AutoRotate = true
});
```

## Chain operations

A pipeline runs several operations while encoding only the final image:

```csharp
using Soenneker.Libvips.Util.Pipelines;
using Soenneker.Libvips.Util.Pipelines.Abstract;

using ILibvipsPipeline pipeline = new LibvipsPipeline()
    .AutoRotate()
    .SmartCrop(1200, 630)
    .Sharpen();

await libvips.Process("photo.jpg", "social-card.avif", pipeline);
```

Pipelines are reusable and safe to inspect asynchronously with `GetCount()` and `GetSteps()`.

## Inspect an image

```csharp
using Soenneker.Libvips.Util.Dtos;

ImageInfo info = await libvips.Identify("photo.jpg");

Console.WriteLine($"{info.Width}x{info.Height}");
Console.WriteLine($"{info.PixelCount} pixels");
Console.WriteLine(info.Format);
```

`Identify` also provides bands, coding, colour interpretation, resolution, loader information, and a case-insensitive metadata dictionary.

## Use any libvips operation

For operations without a dedicated method, build a structured command:

```csharp
using Soenneker.Libvips.Util.Commands;
using Soenneker.Libvips.Util.Commands.Abstract;

ILibvipsCommand command = new LibvipsCommand("colourspace")
    .AddArgument("input.tif")
    .AddArgument("output.jpg")
    .AddArgument("srgb");

IReadOnlyList<string> output = await libvips.Execute(command, log: false);
```

Prefer `Execute` over the raw `Run` method when arguments contain paths or application-supplied values.

## Useful behavior

- Missing output directories are created automatically.
- Output is written atomically, so a failed operation does not replace an existing image.
- Input and output may be the same path.
- Paths containing spaces are supported.
- Every asynchronous operation accepts a cancellation token.
- Missing inputs throw `FileNotFoundException`.
- Invalid options throw an argument exception.
- libvips failures throw `InvalidOperationException` with the native error output.
