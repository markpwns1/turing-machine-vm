using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    // Represents a Turing Machine
    public class TuringMachine
    {
        // Default tape size for all programs
        public const long DEFAULT_MEM_SIZE = 1024;

        // Constants for the accept and reject states
        public const string ACCEPT = "ha";
        public const string REJECT = "hr";

        // Set of all states in the Turing Machine
        public Dictionary<string, State> States { get; }

        // The alphabet of the Turing Machine
        public List<char> Alphabet { get; }

        // The initial (logical) state of the Turing Machine
        public State InitialState { get; private set; }

        // Whether or not to output debug information
        public static bool verbose = false;

        // Creates a new Turing Machine from a program's source code string and an alphabet
        public TuringMachine(string source, IEnumerable<char> alphabet)
        {
            Alphabet = new List<char>() { '_' }; // Always include '_' in the alphabet
            Alphabet.AddRange(alphabet);

            States = new Dictionary<string, State>();

            Parse(source);

            InitialState = TuringMachineValidation.Validate(this);
        }

        // Parses a program from source
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

        // Starting at a certain state, dumps each state and its transitions to a string
        // and recursively dumps the states it transitions to
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

        // Outputs the entire program in a string
        private string DumpProgram() {
            return DumpProgramFrom(new List<string>(), "S");
        }

        public override string ToString()
        {
            return DumpProgram();
        }

        // Runs the Turing Machine with a default memory size. Not guaranteed to terminate!
        public ExecutionResult Run()
        {
            return Run(DEFAULT_MEM_SIZE);
        }

        // Runs the Turing Machine. Not guaranteed to terminate!
        public ExecutionResult Run(long memorySize)
        {
            return Run(new ExecutionState(ExecutionState.CreateTape("", memorySize), 0, States["S"]));
        }

        // Runs the Turing Machine with a given tape. Not guaranteed to terminate!
        public ExecutionResult Run(string tape, long tapeSize)
        {
            return Run(new ExecutionState(ExecutionState.CreateTape(tape, tapeSize), 0, States["S"]));
        }

        // Runs one step of the Turing Machine, and returns the execution result if this step
        // causes the program to terminate
        public ExecutionResult? Step(ExecutionState state) {

            // Given the value of the current cell, find which transition it triggers

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

            // Write the required character to the tape (ignoring it if '*' is specified)
            if(action.write != '*')
                state.Tape[state.ReadWriteHead] = action.write;

            // Move the read/write head in the specified direction
            state.MoveHead((int) action.move);

            // Throw an exception if the tape goes out-of-bounds
            if (state.ReadWriteHead < 0 || state.ReadWriteHead >= state.Tape.Length)
            {
                throw new Exception
                    (state.TapeToString()
                    + "\n" + state.State.name + ", " + cell + " -> " + action.ToString() + 
                    "\nWent out of bounds.");
            }

            // Transition to the next state, or accept/reject if specified
            if (action.next == ACCEPT || action.next == REJECT)
            {
                return new ExecutionResult(state, action.next == ACCEPT);
            }
            else state.Transition(States[action.next]);

            return null;
        }

        // The base Run method, which runs a Turing Machine until (possible!) halt
        // from a starting state. Again, NOT guaranteed to terminate!
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
