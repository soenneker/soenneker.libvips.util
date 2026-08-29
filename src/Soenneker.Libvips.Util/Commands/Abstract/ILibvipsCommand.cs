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

    /// <summary>
    /// Adds a positional argument.
    /// </summary>
    /// <param name="value">Argument or option value to append to the command.</param>
    /// <returns>The same builder instance, so additional classes or variants can be chained.</returns>
    ILibvipsCommand AddArgument(object value);

    /// <summary>
    /// Adds a named option and its value.
    /// </summary>
    /// <param name="name">Name of the Libvips Command value to target.</param>
    /// <param name="value">Argument or option value to append to the command.</param>
    /// <returns>The same builder instance, so additional classes or variants can be chained.</returns>
    ILibvipsCommand AddOption(string name, object value);

    /// <summary>
    /// Adds a boolean flag when <paramref name="enabled"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="name">Name of the Libvips Command value to target.</param>
    /// <param name="enabled">Whether enabled.</param>
    /// <returns>The same builder instance, so additional classes or variants can be chained.</returns>
    ILibvipsCommand AddFlag(string name, bool enabled = true);

    /// <summary>
    /// Returns the escaped command-line representation supplied to the process utility.
    /// </summary>
    /// <returns>The requested text.</returns>
    string ToString();
}
