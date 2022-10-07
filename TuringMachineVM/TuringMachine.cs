using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    public class TuringMachine
    {
        public struct Result
        {
            public readonly char[] tape;
            public readonly long memPos;
            public readonly bool accepted;

            public Result(char[] tape, long memPos, bool accepted)
            {
                this.tape = tape;
                this.memPos = memPos;
                this.accepted = accepted;
            }

            public string DumpTape()
            {
                return string.Join("", tape);
            }


            public override string ToString()
            {
                var str = TapeToString(tape, memPos);
                str += "\n" + (accepted ? "Accepted" : "Rejected");
                return str;
            }
        }

        private const long DEFAULT_MEM_SIZE = 1024;

        private const string ACCEPT = "ha";
        private const string REJECT = "hr";

        private readonly Dictionary<string, State> states = new Dictionary<string, State>();
        private readonly List<char> alphabet;

        private readonly Dictionary<string, State.Effect.Movement> movements
            = new Dictionary<string, State.Effect.Movement>()
        {
            { "l", State.Effect.Movement.Left },
            { "r", State.Effect.Movement.Right },
            { "s", State.Effect.Movement.None },
        };

        public static bool verbose = false;

        public TuringMachine(string source, IEnumerable<char> alphabet)
        {
            this.alphabet = new List<char>() { '_' };
            this.alphabet.AddRange(alphabet);

            var lines = source.Split("\n");

            var lineNo = 0;
            foreach (var line in lines)
            {
                lineNo++;
                if (line.Trim() == "") continue;

                var parts = line.Split("->");
                var condition = parts[0].Split(",");
                var effect = parts[1].Split(",");

                var errorMsg = "Error on line " + lineNo + ": ";

                if (parts.Length != 2 || condition.Length != 2)
                    throw new Exception(errorMsg + "Rules must be in the form of <state>, <character> -> <next state>, <write>, <movement>");

                var name = condition[0].Trim();
                if (!states.ContainsKey(name))
                    states.Add(name, new State(name));

                var state = states[name];
                var movementString = effect[2].Trim();

                if(!movements.ContainsKey(movementString))
                    throw new Exception(errorMsg + "Movement does not exist: " + movementString);

                var toWrite = effect[1].Trim()[0];

                if (toWrite != '*' && !this.alphabet.Contains(toWrite))
                    throw new Exception(errorMsg + "Letter '" + toWrite + "' is not in the alphabet");

                state.transitions[condition[1].Trim()[0]]
                    = new State.Effect(effect[0].Trim(), toWrite, movements[effect[2].Trim()], lineNo);
            }
        }

        public static string TapeToString(char[] tape, long memPos)
        {
            long maxValue = Math.Max(0, Math.Min(memPos, tape.Length));
            for (long i = maxValue; i < tape.Length; i++)
            {
                if (tape[i] != '_')
                {
                    maxValue = i;
                }
            }

            string str = "";
            for (int i = 0; i <= maxValue; i++)
            {
                str += tape[i];
            }

            str += "\n";

            for (int i = 0; i < memPos; i++)
            {
                str += " ";
            }

            str += "^ [" + memPos + "]";

            return str;
        }

        private string ToStringFrom(List<string> visited, string from)
        {
            visited.Add(from);

            var state = states[from];
            var result = state.ToString();

            foreach (var effect in state.transitions.Values)
            {
                if (!visited.Contains(effect.next) && effect.next != ACCEPT && effect.next != REJECT)
                    result += ToStringFrom(visited, effect.next);
            }

            return result;
        }

        public override string ToString()
        {
            return ToStringFrom(new List<string>(), "S");
        }

        private void Verify(List<string> visited, string name)
        {
            if (!states.ContainsKey(name))
                throw new Exception("State " + name + " does not exist");

            var next = states[name];

            if (!next.transitions.ContainsKey('*'))
            {
                foreach (var trigger in next.transitions.Keys)
                {
                    if(!alphabet.Contains(trigger))
                        throw new Exception("State " + name + " does not cover the possibility of " + trigger);
                }
            }

            visited.Add(name);

            foreach(var effect in next.transitions.Values)
            {
                if (!visited.Contains(effect.next) && effect.next != ACCEPT && effect.next != REJECT)
                    Verify(visited, effect.next);
            }
        }

        public void Verify()
        {
            var visited = new List<string>();

            if (!states.ContainsKey("S"))
                throw new Exception("VM does not contain a start state");

            Verify(visited, "S");
        }

        public static char[] CreateTape(string prefix, long size)
        {
            prefix = "_" + prefix;
            if (prefix.Length > size)
                throw new Exception("Prefix cannot be stored in a tape so small");

            var tape = new char[size];

            for (int i = 0; i < tape.Length; i++)
            {
                tape[i] = (i < prefix.Length)? prefix[i] : '_';
            }

            return tape;
        }

        public Result Run()
        {
            return Run(DEFAULT_MEM_SIZE);
        }

        public Result Run(long memorySize)
        {
            return Run(CreateTape("", memorySize), 0, states["S"]);
        }

        public Result Run(string tape, long tapeSize)
        {
            return Run(CreateTape(tape, tapeSize), 0, states["S"]);
        }

        public Result Run(char[] tape, long memPos, State current)
        {
            while(true)
            {
                var cell = tape[memPos];

                State.Effect action;

                if (current.transitions.ContainsKey(cell))
                    action = current.transitions[cell];
                else if (current.transitions.ContainsKey('*'))
                    action = current.transitions['*'];
                else
                {
                    throw new Exception(TapeToString(tape, memPos) + "\nNo transition in " + current.name + " for " + cell);
                }

                if(verbose)
                {
                    Console.WriteLine(TapeToString(tape, memPos) + "\n" + action.line + ". " + current.name + ", " + cell + " -> " + action.ToString());
                }

                if(action.write != '*')
                    tape[memPos] = action.write;

                memPos += (int)action.move;

                if (memPos < 0 || memPos >= tape.Length)
                {
                    throw new Exception
                        (TapeToString(tape, memPos)
                        + "\n" + current.name + ", " + cell + " -> " + action.ToString() + 
                        "\nWent out of bounds.");
                }

                if (action.next == ACCEPT || action.next == REJECT)
                {
                    return new Result(tape, memPos, action.next == ACCEPT);
                }
                else current = states[action.next];
            }
        }
    }
}
