using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
public readonly partial record struct DebuggerLocation(int FileStringIndex, int Line);
