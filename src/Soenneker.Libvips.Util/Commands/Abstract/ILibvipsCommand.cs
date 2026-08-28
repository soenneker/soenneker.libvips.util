using System.Collections.Generic;

namespace Soenneker.Libvips.Util.Commands.Abstract;

/// <summary>
/// A structured, fluent libvips CLI operation. This provides access to operations without requiring callers to
/// construct an argument string.
/// </summary>
public interface ILibvipsCommand
{
    /// <summary>The libvips operation nickname, for example <c>hist_find</c> or <c>colourspace</c>.</summary>
    string Operation { get; }

    /// <summary>The positional arguments currently configured for this command.</summary>
    IReadOnlyList<string> Arguments { get; }

    /// <summary>The named options currently configured for this command.</summary>
    IReadOnlyList<KeyValuePair<string, string?>> Options { get; }

    /// <summary>Adds a positional argument.</summary>
    ILibvipsCommand AddArgument(object value);

    /// <summary>Adds a named option and its value.</summary>
    ILibvipsCommand AddOption(string name, object value);

    /// <summary>Adds a boolean flag when <paramref name="enabled"/> is <see langword="true"/>.</summary>
    ILibvipsCommand AddFlag(string name, bool enabled = true);

    /// <summary>Returns the escaped command-line representation supplied to the process utility.</summary>
    string ToString();
}
