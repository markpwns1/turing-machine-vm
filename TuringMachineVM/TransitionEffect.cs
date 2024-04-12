using System;
using System.Collections.Generic;

namespace TuringMachineVM
{
    // Represents a transition in the Turing Machine, as in
    // the right hand side of the '->' operator in a state
    public struct Transition
    {
        // Table of movement directions to string and vice versa
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

        // The set of possible movement directions for a transition
        public enum Movement
        {
            Left = -1,
            None = 0,
            Right = 1
        }

        public readonly char write; // The character to write to the tape
        public readonly string next; // The next state to transition to
        public readonly Movement move; // The direction to move the read/write head
        public readonly int line; // The line in the source code where this transition was defined

        public Transition(string next, char write, Movement move, int line)
        {
            this.next = next;
            this.write = write;
            this.move = move;
            this.line = line;
        }

        // Outputs a transition in the format of <next state>, <write>, <movement>
        public override string ToString()
        {
            return next + ", " + write + ", " + MOVEMENTS_TO_STRING[move];
        }

        // Parses a transition from a string in the format of "<next state>, <write>, <movement>"
        public static Transition FromString(string str, int line)
        {
            var parts = str.Split(",");
            if (parts.Length != 3)
                throw new ParsingException(line, "Invalid transition format: " + str + ". Expected format <next state>, <write>, <movement>");

            var write = parts[1].Trim();
            if (write.Length != 1)
                throw new ParsingException(line, "Invalid transition format: " + str + ". Write must be a single character");

            var move = parts[2].Trim();
            if (!STRING_TO_MOVEMENTS.ContainsKey(move))
                throw new ParsingException(line, "Invalid transition format: " + str + ". Movement must be one of: 'l', 'r', 's'");

            return new Transition(parts[0].Trim(), write[0], STRING_TO_MOVEMENTS[move], line);
        }
    }
}