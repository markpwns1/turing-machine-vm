using System;
using System.Collections.Generic;

public struct Transition
{
    private static readonly Dictionary<Movement, string> MOVEMENTS_TO_STRING
        = new Dictionary<Movement, string>()
    {
        { Movement.Left, "l" },
        { Movement.Right, "r" },
        { Movement.None, "s" },
    };
    private static readonly Dictionary<string, Movement> STRING_TO_MOVEMENTS
        = new Dictionary<string, Movement>()
    {
        { "l", Movement.Left },
        { "r", Movement.Right },
        { "s", Movement.None },
    };

    public enum Movement
    {
        Left = -1,
        None = 0,
        Right = 1
    }

    public readonly char write;
    public readonly string next;
    public readonly Movement move;
    public readonly int line;
    public Transition(string next, char write, Movement move, int line)
    {
        this.next = next;
        this.write = write;
        this.move = move;
        this.line = line;
    }

    public override string ToString()
    {
        return next + ", " + write + ", " + MOVEMENTS_TO_STRING[move];
    }

    public static Transition FromString(string str, int line) {
        var parts = str.Split(",");
        if (parts.Length != 3) 
            throw new ParsingException(line, "Invalid transition format: " + str + ". Expected format <next state>, <write>, <movement>");
        
        var write = parts[1].Trim();
        if(write.Length != 1)
            throw new ParsingException(line, "Invalid transition format: " + str + ". Write must be a single character");

        var move = parts[2].Trim();
        if(!STRING_TO_MOVEMENTS.ContainsKey(move))
            throw new ParsingException(line, "Invalid transition format: " + str + ". Movement must be one of: 'l', 'r', 's'");
        
        return new Transition(parts[0].Trim(), write[0], STRING_TO_MOVEMENTS[move], line);
    }
}