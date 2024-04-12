
namespace TuringMachineVM
{
    // Represents the result of a Turing Machine's complete, halting execution
    public struct ExecutionResult
    {
        public ExecutionState FinalState { get; } // The state of the machine after execution
        public bool Accepted { get; } // Whether or not the input was accepted

        public ExecutionResult(ExecutionState finalState, bool accepted)
        {
            FinalState = finalState;
            Accepted = accepted;
        }

        // Dumps the tape to a string
        public string DumpTape()
        {
            return string.Join("", FinalState.Tape);
        }

        // Outputs the result of the execution, which is the final tape and whether or not the input was accepted
        public override string ToString()
        {
            var str = FinalState.TapeToString();
            str += "\n" + (Accepted ? "Accepted" : "Rejected");
            return str;
        }
    }
}