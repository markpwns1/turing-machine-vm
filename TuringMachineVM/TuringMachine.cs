using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    public class TuringMachine
    {
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

        public ExecutionResult Run()
        {
            return Run(DEFAULT_MEM_SIZE);
        }

        public ExecutionResult Run(long memorySize)
        {
            return Run(new ExecutionState(ExecutionState.CreateTape("", memorySize), 0, states["S"]));
        }

        public ExecutionResult Run(string tape, long tapeSize)
        {
            return Run(new ExecutionState(ExecutionState.CreateTape(tape, tapeSize), 0, states["S"]));
        }

        public ExecutionResult? Step(ExecutionState state) {
            var cell = state.Tape[state.ReadWriteHead];

            Transition action;

            if (state.State.transitions.ContainsKey(cell))
                action = state.State.transitions[cell];
            else if (state.State.transitions.ContainsKey('*'))
                action = state.State.transitions['*'];
            else
            {
                throw new Exception(state.TapeToString() + "\nNo transition in " + state.State.name + " for " + cell);
            }

            if(verbose)
            {
                Console.WriteLine(state.TapeToString() + "\n" + action.line + ". " + state.State.name + ", " + cell + " -> " + action.ToString());
            }

            if(action.write != '*')
                state.Tape[state.ReadWriteHead] = action.write;

            state.MoveHead((int) action.move);

            if (state.ReadWriteHead < 0 || state.ReadWriteHead >= state.Tape.Length)
            {
                throw new Exception
                    (state.TapeToString()
                    + "\n" + state.State.name + ", " + cell + " -> " + action.ToString() + 
                    "\nWent out of bounds.");
            }

            if (action.next == ACCEPT || action.next == REJECT)
            {
                return new ExecutionResult(state, action.next == ACCEPT);
            }
            else state.Transition(states[action.next]);

            return null;
        }

        public ExecutionResult Run(ExecutionState state)
        {
            while(true)
            {
                var result = Step(state);
                if(result.HasValue) return result.Value;
            }
        }
    }
}
