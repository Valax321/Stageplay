using Cysharp.Text;
using Lua;

namespace Radish.Lua;

internal static class LuaStaticVmFunctions
{
    public static readonly LuaFunction LogInfo = new(LogInfoImpl);
    public static readonly LuaFunction LogWarning = new(LogWarningImpl);
    public static readonly LuaFunction LogError = new(LogErrorImpl);
    
    private static ValueTask<int> LogInfoImpl(LuaFunctionExecutionContext context, CancellationToken token)
    {
        using var sb = ZString.CreateStringBuilder();
        sb.AppendJoin(' ', context.Arguments);
        Log.Info(sb.AsSpan());
        return new ValueTask<int>(context.Return());
    }
    
    private static ValueTask<int> LogWarningImpl(LuaFunctionExecutionContext context, CancellationToken token)
    {
        using var sb = ZString.CreateStringBuilder();
        sb.AppendJoin(' ', context.Arguments);
        Log.Warning(sb.AsSpan());
        return new ValueTask<int>(context.Return());
    }
    
    private static ValueTask<int> LogErrorImpl(LuaFunctionExecutionContext context, CancellationToken token)
    {
        using var sb = ZString.CreateStringBuilder();
        sb.AppendJoin(' ', context.Arguments);
        Log.Error(sb.AsSpan());
        return new ValueTask<int>(context.Return());
    }
}