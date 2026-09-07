using System;
using Soenneker.Libvips.Util.Commands.Abstract;

namespace Soenneker.Libvips.Util.Pipelines.Abstract;

/// <summary>A configured operation in a libvips pipeline.</summary>
public interface ILibvipsPipelineStep
{
    /// <summary>Gets the libvips operation nickname.</summary>
    string Operation { get; }

    /// <summary>Gets the optional command configuration callback.</summary>
    Action<ILibvipsCommand>? Configure { get; }
}
