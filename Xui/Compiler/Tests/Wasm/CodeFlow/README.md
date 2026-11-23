# Control-flow WASM demos

These REPL-style tests compile the shared C# fixtures, emit WASM, and execute it in Node. Each snapshot contains source, captured output, exit code, and WAT. The individual demos call `Run` with `-3`, `0`, `1`, `2`, `5`, and `10`, covering skipped loops, one iteration, repeated iterations, and both conditional arms. Debug and Release output must match the CLR; the snapshots show Release WAT.

| Snapshot | Demonstrates |
| --- | --- |
| [If](Snapshots/If.md) | Conditional without an else |
| [IfElse](Snapshots/IfElse.md) | Both arms assigning a result |
| [ElseIf](Snapshots/ElseIf.md) | Negative, zero, and positive cases |
| [NestedConditionals](Snapshots/NestedConditionals.md) | Conditions inside both outer arms |
| [EarlyReturn](Snapshots/EarlyReturn.md) | Multiple return paths |
| [ConditionalValue](Snapshots/ConditionalValue.md) | Integer ternary |
| [StackPrefix](Snapshots/StackPrefix.md) | A ternary preserving an earlier call argument |
| [Conditionals](Snapshots/Conditionals.md) | Integer, float, and string results and static calls |
| [While](Snapshots/While.md) | Condition before the first iteration |
| [DoWhile](Snapshots/DoWhile.md) | Body executes at least once |
| [For](Snapshots/For.md) | Initialization, condition, and increment |
| [Nested](Snapshots/Nested.md) | If/else inside a loop |
| [NestedLoops](Snapshots/NestedLoops.md) | A do/while inside a while |
| [WhileWithBreak](Snapshots/WhileWithBreak.md) | Early exit to the loop continuation |
| [WhileWithContinue](Snapshots/WhileWithContinue.md) | Skip the rest of an iteration |

`LoopTests` additionally exercises values carried on the evaluation stack across back edges, different loop input/result lengths, and local initialization at exits. Loops currently require one entry and one shared continuation. Multiple-entry cycles, loops without a continuation, switches, and overlapping regions still throw.

The separate [instruction graph snapshots](../../XuiNet/CodeFlow/Snapshots) display MSIL edges and interned stack states for these fixtures. Source links in snapshots are relative to the tests project root.
