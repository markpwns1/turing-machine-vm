using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    public class State
    {
        public struct Effect
        {
            private static readonly Dictionary<Movement, string> MOVEMENTS_TO_STRING
                = new Dictionary<Movement, string>()
            {
                { Movement.Left, "l" },
                { Movement.Right, "r" },
                { Movement.None, "s" },
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
            public Effect(string next, char write, Movement move, int line)
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
        }

        public string name;
        public readonly Dictionary<char, Effect> transitions = new Dictionary<char, Effect>();

        public State(string name)
        {
            this.name = name;
        }

        public string ToString(char trigger)
        {
            return name + ", " + trigger + " -> " + transitions[trigger].ToString();
        }

        public override string ToString()
        {
            string str = "";

            foreach (var trigger in transitions.Keys)
            {
                str += ToString(trigger) + "\n";
            }

            return str;
        }
    }
}
