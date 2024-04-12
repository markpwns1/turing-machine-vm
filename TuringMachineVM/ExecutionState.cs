using System;
using TuringMachineVM;

namespace TuringMachineVM
{
    // Represents the state of a Turing Machine during execution. That is, 
    // what's written in the tape, the position of the read/write head, and
    // the current (logical) state that the machine is on
    public class ExecutionState
    {
        public char[] Tape { get; }
        public long ReadWriteHead { get; private set; }
        public State State { get; private set; } // Logical state that the machine is on

        public ExecutionState(char[] tape, long rwHead, State state)
        {
            Tape = tape;
            ReadWriteHead = rwHead;
            State = state;
        }

        // Creates a new state meant to represent the beginning of execution. `prefix` specifies what's
        // already written in the tape, and `headPosition` specifies where the read/write head should start
        public static ExecutionState Begin(TuringMachine tm, long tapeSize, string prefix = "", int headPosition = 0)
        {
            return new ExecutionState(CreateTape(prefix, tapeSize), headPosition, tm.InitialState);
        }

        // Same as above but with some things automatically filled in
        public static ExecutionState Begin(TuringMachine tm, long tapeSize, int headPosition)
        {
            return Begin(tm, tapeSize, "", headPosition);
        }

        // Offsets the read/write head by `amount`
        public void MoveHead(long amount)
        {
            ReadWriteHead += amount;
        }

        // Transitions the logical state of the machine
        public void Transition(State newState)
        {
            State = newState;
        }

        // Returns the entire tape in string form
        public string TapeToString()
        {
            long maxValue = Math.Max(0, Math.Min(ReadWriteHead, Tape.Length));
            for (long i = maxValue; i < Tape.Length; i++)
            {
                if (Tape[i] != '_')
                {
                    maxValue = i;
                }
            }

            string str = "";
            for (int i = 0; i <= maxValue; i++)
            {
                str += Tape[i];
            }

            str += "\n";

            for (int i = 0; i < ReadWriteHead; i++)
            {
                str += " ";
            }

            str += "^ [" + ReadWriteHead + "]";

            return str;
        }

        // Utility function to create a tape with a prefix
        public static char[] CreateTape(string prefix, long size)
        {
            prefix = "_" + prefix;
            if (prefix.Length > size)
                throw new Exception("Prefix cannot be stored in a tape so small");

            var tape = new char[size];

            for (int i = 0; i < tape.Length; i++)
            {
                tape[i] = (i < prefix.Length) ? prefix[i] : '_';
            }

            return tape;
        }
    }
}