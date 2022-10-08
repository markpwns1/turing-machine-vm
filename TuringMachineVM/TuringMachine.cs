using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    public class TuringMachine
    {
        public struct Result
        {
            public ExecutionState FinalState { get; }
            public bool Accepted { get; }

            public Result(ExecutionState finalState, bool accepted)
            {
                FinalState = finalState;
                Accepted = accepted;
            }

            public string DumpTape()
            {
                return string.Join("", FinalState.Tape);
            }


            public override string ToString()
            {
                var str = TapeToString(FinalState);
                str += "\n" + (Accepted ? "Accepted" : "Rejected");
                return str;
            }
        }

        private const long DEFAULT_MEM_SIZE = 1024;

        private const string ACCEPT = "ha";
        private const string REJECT = "hr";

        private readonly Dictionary<string, State> states = new Dictionary<string, State>();
        private readonly List<char> alphabet;

        public State InitialState { get; private set; }

        public static bool verbose = false;

        public TuringMachine(string source, IEnumerable<char> alphabet)
        {
            this.alphabet = new List<char>() { '_' };
            this.alphabet.AddRange(alphabet);

            Parse(source);

            ValidateAlphabet();
            ValidateStates();
        }

        private void Parse(string source) {
            var lines = source.Split("\n");

            var lineNo = 0;
            foreach (var line in lines)
            {
                lineNo++;
                if (line.Trim() == "") continue;

                State.FromString(states, line, lineNo);
            }
        }

        public static string TapeToString(ExecutionState state)
        {
            long maxValue = Math.Max(0, Math.Min(state.ReadWriteHead, state.Tape.Length));
            for (long i = maxValue; i < state.Tape.Length; i++)
            {
                if (state.Tape[i] != '_')
                {
                    maxValue = i;
                }
            }

            string str = "";
            for (int i = 0; i <= maxValue; i++)
            {
                str += state.Tape[i];
            }

            str += "\n";

            for (int i = 0; i < state.ReadWriteHead; i++)
            {
                str += " ";
            }

            str += "^ [" + state.ReadWriteHead + "]";

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

        private void ValidateStates(List<string> visited, string name)
        {
            var next = states[name];

            if (!next.transitions.ContainsKey('*'))
            {
                foreach (var letter in alphabet)
                {
                    if(!next.transitions.ContainsKey(letter))
                        throw new SemanticException("State " + name + " does not have a transition for letter " + letter);
                }
            }

            visited.Add(name);

            foreach(var effect in next.transitions.Values)
            {
                if (!visited.Contains(effect.next) && effect.next != ACCEPT && effect.next != REJECT) {
                    if(!states.ContainsKey(effect.next))
                        throw new SemanticException(effect.line, "State " + effect.next + " does not exist");

                    ValidateStates(visited, effect.next);
                }
            }
        }

        private void ValidateStates()
        {
            var visited = new List<string>();

            if (!states.ContainsKey("S"))
                throw new SemanticException("VM does not contain a start state");

            InitialState = states["S"];

            ValidateStates(visited, "S");
        }

        private void ValidateAlphabet() {
            foreach (var state in states.Values)
            {
                foreach (var trigger in state.transitions.Keys) {
                    if (trigger != '*' && !alphabet.Contains(trigger))
                        throw new SemanticException(state.transitions[trigger].line, "State " + state.name + " has a transition for letter " + trigger + " which is not in the alphabet");
                }

                foreach (var effect in state.transitions.Values)
                {
                    if (effect.write != '*' && !alphabet.Contains(effect.write))
                        throw new SemanticException(effect.line, "State " + state.name + " writes " + effect.write + " which is not in the alphabet");
                }
            }
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
            return Run(new ExecutionState(CreateTape("", memorySize), 0, states["S"]));
        }

        public Result Run(string tape, long tapeSize)
        {
            return Run(new ExecutionState(CreateTape(tape, tapeSize), 0, states["S"]));
        }

        public Result? Step(ExecutionState state) {
            var cell = state.Tape[state.ReadWriteHead];

            Transition action;

            if (state.State.transitions.ContainsKey(cell))
                action = state.State.transitions[cell];
            else if (state.State.transitions.ContainsKey('*'))
                action = state.State.transitions['*'];
            else
            {
                throw new Exception(TapeToString(state) + "\nNo transition in " + state.State.name + " for " + cell);
            }

            if(verbose)
            {
                Console.WriteLine(TapeToString(state) + "\n" + action.line + ". " + state.State.name + ", " + cell + " -> " + action.ToString());
            }

            if(action.write != '*')
                state.Tape[state.ReadWriteHead] = action.write;

            state.MoveHead((int) action.move);

            if (state.ReadWriteHead < 0 || state.ReadWriteHead >= state.Tape.Length)
            {
                throw new Exception
                    (TapeToString(state)
                    + "\n" + state.State.name + ", " + cell + " -> " + action.ToString() + 
                    "\nWent out of bounds.");
            }

            if (action.next == ACCEPT || action.next == REJECT)
            {
                return new Result(state, action.next == ACCEPT);
            }
            else state.Transition(states[action.next]);

            return null;
        }

        public Result Run(ExecutionState state)
        {
            while(true)
            {
                var result = Step(state);
                if(result.HasValue) return result.Value;
            }
        }
    }
}
