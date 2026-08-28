using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Soenneker.Libvips.Util.Commands.Abstract;

namespace Soenneker.Libvips.Util.Commands;

public sealed class LibvipsCommand : ILibvipsCommand
{
    private readonly List<string> _arguments = [];
    private readonly List<KeyValuePair<string, string?>> _options = [];
    private readonly ReadOnlyCollection<string> _readOnlyArguments;
    private readonly ReadOnlyCollection<KeyValuePair<string, string?>> _readOnlyOptions;

    public string Operation { get; }

    public IReadOnlyList<string> Arguments => _readOnlyArguments;

    public IReadOnlyList<KeyValuePair<string, string?>> Options => _readOnlyOptions;

    public LibvipsCommand(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ValidateName(operation, nameof(operation));
        Operation = operation;
        _readOnlyArguments = _arguments.AsReadOnly();
        _readOnlyOptions = _options.AsReadOnly();
    }

    public ILibvipsCommand AddArgument(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _arguments.Add(Format(value));
        return this;
    }

    public ILibvipsCommand AddOption(string name, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);
        ValidateName(name, nameof(name));
        _options.Add(new KeyValuePair<string, string?>(name, Format(value)));
        return this;
    }

    public ILibvipsCommand AddFlag(string name, bool enabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateName(name, nameof(name));
        if (enabled)
            _options.Add(new KeyValuePair<string, string?>(name, null));
        return this;
    }

    internal static string Build(ILibvipsCommand command)
    {
        var arguments = new List<string>(1 + command.Arguments.Count + (command.Options.Count * 2)) {command.Operation};
        arguments.AddRange(command.Arguments);

        foreach (KeyValuePair<string, string?> option in command.Options)
        {
            arguments.Add($"--{option.Key}");
            if (option.Value is not null)
                arguments.Add(option.Value);
        }

        return BuildArgumentString(arguments);
    }

    internal static string BuildArgumentString(IReadOnlyList<string> arguments)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < arguments.Count; index++)
        {
            if (index > 0)
                builder.Append(' ');
            builder.Append(Quote(arguments[index]));
        }

        return builder.ToString();
    }

    public override string ToString() => Build(this);

    private static string Quote(string value)
    {
        bool requiresQuotes = value.Length == 0;
        for (var index = 0; index < value.Length && !requiresQuotes; index++)
            requiresQuotes = char.IsWhiteSpace(value[index]) || value[index] == '"';

        if (!requiresQuotes)
            return value;

        StringBuilder builder = new StringBuilder(value.Length + 2).Append('"');
        var backslashCount = 0;

        foreach (char character in value)
        {
            if (character == '\\')
            {
                backslashCount++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', (backslashCount * 2) + 1).Append('"');
                backslashCount = 0;
                continue;
            }

            builder.Append('\\', backslashCount).Append(character);
            backslashCount = 0;
        }

        builder.Append('\\', backslashCount * 2).Append('"');
        return builder.ToString();
    }

    private static string Format(object value) => value switch
    {
        bool boolean => boolean ? "true" : "false",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static void ValidateName(string value, string parameterName)
    {
        if (!char.IsAsciiLetter(value[0]))
            throw new ArgumentException("Names must begin with an ASCII letter.", parameterName);

        foreach (char character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_')
                throw new ArgumentException("Names may contain only ASCII letters, digits, hyphens, and underscores.", parameterName);
        }
    }
}
