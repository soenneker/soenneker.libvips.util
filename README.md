[![](https://img.shields.io/nuget/v/soenneker.libvips.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libvips.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.libvips.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.libvips.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.libvips.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.libvips.util/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Libvips.Util
### A cross-platform .NET API for the bundled libvips command-line distributions.

## Installation

```
dotnet add package Soenneker.Libvips.Util
```

The package includes `Soenneker.Libvips.Linux` and `Soenneker.Libvips.Windows` and launches the bundled `vips` executable for the current x64 platform.

```csharp
using Soenneker.Libvips.Util;
using Soenneker.Libvips.Util.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soenneker.Libvips.Util.Abstract;
using Soenneker.Libvips.Util.Registrars;

await using ServiceProvider provider = new ServiceCollection()
    .AddLogging()
    .AddLibvipsUtilAsSingleton()
    .BuildServiceProvider();

ILibvipsUtil libvips = provider.GetRequiredService<ILibvipsUtil>();

await libvips.Run("copy \"wwwroot/images/photo.jpg\" \"wwwroot/images/photo.avif[Q=80,effort=4,strip]\"");

await libvips.ConvertToAvif("wwwroot/images/photo.jpg", "wwwroot/images/photo.avif", new LibvipsOptions
{
    Quality = 80,
    Effort = 4,
    StripMetadata = true
});

await libvips.ConvertToWebp("wwwroot/images/photo.jpg", "wwwroot/images/photo.webp");
await libvips.Resize("wwwroot/images/hero.png", "wwwroot/images/hero.webp", width: 1600);
```

## Image operations

The high-level API supports any output format available in the bundled libvips build. The encoder is selected from the output extension. Common operations include:

```csharp
await libvips.Convert("photo.tif", "photo.jpg");
await libvips.Crop("photo.jpg", "crop.png", left: 10, top: 20, width: 800, height: 600);
await libvips.SmartCrop("photo.jpg", "subject.webp", 640, 640);
await libvips.Rotate("photo.jpg", "rotated.jpg", LibvipsAngle.D90);
await libvips.AutoRotate("phone-photo.jpg", "upright.jpg");
await libvips.Flip("photo.jpg", "mirrored.jpg", LibvipsDirection.Horizontal);
await libvips.Blur("photo.jpg", "blurred.jpg", sigma: 2.5);
await libvips.Sharpen("photo.jpg", "sharp.jpg");
await libvips.Gamma("photo.jpg", "corrected.jpg");
await libvips.Invert("photo.jpg", "negative.jpg");
await libvips.Flatten("transparent.png", "flat.jpg", [255, 255, 255]);
```

`ResizeOptions` exposes libvips sizing, smart-crop, orientation, and linear-light behavior. `LibvipsOptions` controls format-aware quality, effort, lossless encoding, metadata, progressive output, and PNG compression.

## Pipelines

Use a pipeline to describe and reuse multi-step workflows. Intermediate images use the native VIPS format and only the result is encoded:

```csharp
var pipeline = new LibvipsPipeline()
    .AutoRotate()
    .SmartCrop(1200, 630, LibvipsInteresting.Attention)
    .Sharpen()
    .Gamma();

await libvips.Process("original.jpg", "social-card.avif", pipeline);
```

## Full libvips access

`LibvipsCommand` is a structured escape hatch for every operation exposed by the bundled build:

```csharp
var command = new LibvipsCommand("colourspace")
    .AddArgument("input.tif")
    .AddArgument("output.jpg")
    .AddArgument("srgb");

IReadOnlyList<string> output = await libvips.Execute(command, log: false);
```

`Run` remains available for raw CLI arguments. `Identify` returns dimensions, bands, pixel format, coding, colour interpretation, resolution, loader, and the complete metadata dictionary. `GetVersion` returns the bundled executable version.
