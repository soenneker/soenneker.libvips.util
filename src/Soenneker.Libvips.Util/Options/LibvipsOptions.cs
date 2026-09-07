using System;

namespace Soenneker.Libvips.Util.Options;

/// <summary>Controls output encoding. Only options supported by the selected output format are emitted.</summary>
public class LibvipsOptions
{
    /// <summary>Removes metadata such as EXIF from the generated image.</summary>
    public bool StripMetadata { get; init; } = true;

    /// <summary>Metadata flags to retain, such as <c>exif|icc</c>. Overrides <see cref="StripMetadata"/> when set.</summary>
    public string? KeepMetadata { get; init; }

    /// <summary>Optional ICC profile filename to embed.</summary>
    public string? Profile { get; init; }

    /// <summary>Optional background band values used when saving.</summary>
    public double[]? Background { get; init; }

    /// <summary>Page height for multipage output, or 0 to leave it unset.</summary>
    public int PageHeight { get; init; }

    /// <summary>Validates that all encoder settings are within their supported ranges.</summary>
    /// <exception cref="ArgumentOutOfRangeException">An option is outside its supported range.</exception>
    public virtual void Validate()
    {
        if (PageHeight is < 0 or > 100_000_000)
            throw new ArgumentOutOfRangeException(nameof(PageHeight), "Page height must be between 0 and 100,000,000.");

        if (Background is not null)
        {
            foreach (double value in Background)
                if (!double.IsFinite(value))
                    throw new ArgumentOutOfRangeException(nameof(Background), "Background values must be finite numbers.");
        }
    }
}
