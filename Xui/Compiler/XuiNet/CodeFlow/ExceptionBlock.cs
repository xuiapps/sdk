namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A protected body and its ordered catch or cleanup handlers, with structured normal leave destinations.</summary>
public sealed class ExceptionBlock : FlowNode
{
    internal ExceptionBlock(int start, int end, IReadOnlyList<int> continuations, SequenceBlock body, IReadOnlyList<ExceptionHandler> handlers)
    { Start = start; End = end; Continuations = continuations; Body = body; Handlers = handlers; }
    /// <summary>The first protected IL offset.</summary>
    public int Start { get; }
    /// <summary>The exclusive end of the protected body and its handlers.</summary>
    public int End { get; }
    /// <summary>The common leave target, if any; a throwing finally can still prevent reaching it.</summary>
    public int? Continuation => Continuations.Count == 1 ? Continuations[0] : null;
    /// <summary>Normal leave destinations in this protected region.</summary>
    public IReadOnlyList<int> Continuations { get; }
    /// <summary>Whether a leave can complete after any finally handler has executed.</summary>
    public bool CanContinue => Continuations.Count != 0 &&
        (Handlers[0].Kind != System.Reflection.Metadata.ExceptionRegionKind.Finally || CompletesCleanup(Handlers[0].Body));

    private static bool CompletesCleanup(SequenceBlock body) => body.Children.Any(node => node switch
    {
        InstructionBlock instruction => instruction.Instruction.Code == DotNet.Op.OpCode.Endfinally,
        ConditionalBlock conditional => CompletesCleanup(conditional.Taken) || CompletesCleanup(conditional.NotTaken),
        SwitchBlock selection => selection.Arms.Any(arm => CompletesCleanup(arm.Body)),
        LoopBlock loop => CompletesCleanup(loop.Body),
        _ => false
    });

    /// <summary>The structured protected body.</summary>
    public SequenceBlock Body { get; }
    /// <summary>Catch handlers in metadata order, or a single cleanup handler.</summary>
    public IReadOnlyList<ExceptionHandler> Handlers { get; }
}
