using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Soenneker.Libvips.Util.Commands;

/// <summary>
/// A structured, fluent libvips CLI operation. This provides access to operations which do not yet have a dedicated
/// method on <c>ILibvipsUtil</c> without requiring callers to construct an unsafe argument string.
/// </summary>
public sealed class LibvipsCommand
{
    private readonly List<string> _arguments = [];
    private readonly List<KeyValuePair<string, string?>> _options = [];

    /// <summary>The libvips operation nickname, for example <c>hist_find</c> or <c>colourspace</c>.</summary>
    public string Operation { get; }

    /// <summary>Creates a structured command for a libvips operation.</summary>
    /// <param name="operation">The operation nickname. Only letters, digits, hyphens, and underscores are accepted.</param>
    public LibvipsCommand(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ValidateName(operation, nameof(operation));
        Operation = operation;
    }

    /// <summary>Adds a positional argument. Values are escaped when the command is built.</summary>
    public LibvipsCommand AddArgument(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _arguments.Add(Format(value));
        return this;
    }

    /// <summary>Adds a named option and its value.</summary>
    public LibvipsCommand AddOption(string name, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);
        ValidateName(name, nameof(name));
        _options.Add(new KeyValuePair<string, string?>(name, Format(value)));
        return this;
    }

    /// <summary>Adds a boolean flag when <paramref name="enabled"/> is <see langword="true"/>.</summary>
    public LibvipsCommand AddFlag(string name, bool enabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateName(name, nameof(name));
        if (enabled)
            _options.Add(new KeyValuePair<string, string?>(name, null));
        return this;
    }

    internal string Build()
    {
        var builder = new StringBuilder(Operation);

        foreach (string argument in _arguments)
            builder.Append(' ').Append(Quote(argument));

        foreach (KeyValuePair<string, string?> option in _options)
        {
            builder.Append(" --").Append(option.Key);
            if (option.Value is not null)
                builder.Append(' ').Append(Quote(option.Value));
        }

        return builder.ToString();
    }

    internal static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    private static string Format(object value) => value switch
    {
        bool boolean => boolean ? "true" : "false",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static void ValidateName(string value, string parameterName)
    {
        foreach (char character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_')
                throw new ArgumentException("Names may contain only ASCII letters, digits, hyphens, and underscores.", parameterName);
        }
    }
}
