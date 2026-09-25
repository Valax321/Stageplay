using MemoryPack;

namespace Radish.Scenario;

[MemoryPackable]
public readonly partial record struct SourceLocation(int FileStringIndex, int Line);