using System;
using Soenneker.Libvips.Util.Commands.Abstract;
using Soenneker.Libvips.Util.Pipelines.Abstract;

namespace Soenneker.Libvips.Util.Pipelines;

/// <summary>Represents an immutable operation configured in a libvips pipeline.</summary>
internal sealed record LibvipsPipelineStep(string Operation, Action<ILibvipsCommand>? Configure) : ILibvipsPipelineStep;
