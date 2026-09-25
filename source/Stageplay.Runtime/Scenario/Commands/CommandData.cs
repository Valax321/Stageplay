namespace Radish.Scenario.Commands;

public readonly record struct CommandData(string Name, params IReadOnlyList<object> Arguments);