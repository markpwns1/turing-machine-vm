using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    public class TuringMachine
    {
        public const long DEFAULT_MEM_SIZE = 1024;

        public const string ACCEPT = "ha";
        public const string REJECT = "hr";

        public Dictionary<string, State> States { get; }
        public List<char> Alphabet { get; }
        public State InitialState { get; private set; }

        public static bool verbose = false;

        public TuringMachine(string source, IEnumerable<char> alphabet)
        {
            this.Alphabet = new List<char>() { '_' };
            this.Alphabet.AddRange(alphabet);

            States = new Dictionary<string, State>();

            Parse(source);

            InitialState = TuringMachineValidation.Validate(this);
        }

        private void Parse(string source) {
            var lines = source.Split("\n");

            var lineNo = 0;
            foreach (var line in lines)
            {
                lineNo++;
                if (line.Trim() == "") continue;

                State.FromString(States, line, lineNo);
            }
        }

        private string DumpProgramFrom(List<string> visited, string from)
        {
            visited.Add(from);

            var state = States[from];
            var result = state.ToString();

            foreach (var effect in state.transitions.Values)
            {
                if (!visited.Contains(effect.next) && effect.next != ACCEPT && effect.next != REJECT)
                    result += DumpProgramFrom(visited, effect.next);
            }

            return result;
        }

        private string DumpProgram() {
            return DumpProgramFrom(new List<string>(), "S");
        }

        public override string ToString()
        {
            return DumpProgram();
        }

        public ExecutionResult Run()
        {
            return Run(DEFAULT_MEM_SIZE);
        }

        public ExecutionResult Run(long memorySize)
        {
            return Run(new ExecutionState(ExecutionState.CreateTape("", memorySize), 0, States["S"]));
        }

        public ExecutionResult Run(string tape, long tapeSize)
        {
            return Run(new ExecutionState(ExecutionState.CreateTape(tape, tapeSize), 0, States["S"]));
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
            else state.Transition(States[action.next]);

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
