using JetBrains.Annotations;
using Radish.Scenario.Commands;

namespace Radish.Scenario;

[PublicAPI]
public readonly struct ArgumentValue
{
    public CommandArgumentType Type { get; private init; }

    // This is reinterpreted as the correct value at runtime
    // Proof that C# should have had unions
    private int Value { get; init; }

    public byte AsByte
    {
        get
        {
            if (Type is not CommandArgumentType.Byte)
                throw new ArgumentException("Value is not a byte", nameof(Type));

            return (byte)Value;
        }
    }
    
    public int AsInteger
    {
        get
        {
            if (Type is not CommandArgumentType.Integer)
                throw new ArgumentException("Value is not an integer", nameof(Type));

            return Value;
        }
    }
    
    public unsafe float AsFloat
    {
        get
        {
            if (Type is not CommandArgumentType.Float)
                throw new ArgumentException("Value is not a float", nameof(Type));

            var val = Value;
            return *(float*)&val;
        }
    }
    
    public bool AsBoolean
    {
        get
        {
            if (Type is not CommandArgumentType.Boolean)
                throw new ArgumentException("Value is not a bool", nameof(Type));

            return Value > 0;
        }
    }

    public int AsStringIndex
    {
        get
        {
            if (Type is not CommandArgumentType.String)
                throw new ArgumentException("Value is not a string", nameof(Type));

            return Value;
        }
    }

    public static ArgumentValue OfByte(byte val)
    {
        return new ArgumentValue
        {
            Type = CommandArgumentType.Byte,
            Value = val
        };
    }
    
    public static ArgumentValue OfInteger(int val)
    {
        return new ArgumentValue
        {
            Type = CommandArgumentType.Integer,
            Value = val
        };
    }
    
    public static unsafe ArgumentValue OfFloat(float val)
    {
        var ptrAsInt = (int*)&val;
        return new ArgumentValue
        {
            Type = CommandArgumentType.Float,
            Value = *ptrAsInt
        };
    }

    public static ArgumentValue OfString(int stringIndex)
    {
        return new ArgumentValue
        {
            Type = CommandArgumentType.String,
            Value = stringIndex
        };
    }

    public static ArgumentValue OfBoolean(bool val)
    {
        return new ArgumentValue
        {
            Type = CommandArgumentType.Boolean,
            Value = val ? 1 : 0
        };
    }

    public static implicit operator ArgumentValue(byte val) 
        => OfByte(val);
    
    public static implicit operator ArgumentValue(int val) 
        => OfInteger(val);
    
    public static implicit operator ArgumentValue(float val) 
        => OfFloat(val);
    
    public static implicit operator ArgumentValue(bool val) 
        => OfBoolean(val);
}