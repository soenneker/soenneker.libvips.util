using System.Collections.Generic;
using Soenneker.Libvips.Util.Commands.Abstract;

namespace Soenneker.Libvips.Util.Tests;

/// <summary>Provides an intentionally invalid command for command-validation tests.</summary>
internal sealed class InvalidCommand : ILibvipsCommand
{
    public string Operation => "copy --version";
    public IReadOnlyList<string> Arguments { get; } = [];
    public IReadOnlyList<KeyValuePair<string, string?>> Options { get; } = [];

    public ILibvipsCommand AddArgument(object value) => this;
    public ILibvipsCommand AddOption(string name, object value) => this;
    public ILibvipsCommand AddFlag(string name, bool enabled = true) => this;
    public override string ToString() => Operation;
}
