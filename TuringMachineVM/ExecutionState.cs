using System;
using TuringMachineVM;
public class ExecutionState {
    public char[] Tape { get; }
    public long ReadWriteHead { get; private set; }
    public State State { get; private set; }

    // Simple constructor
    public ExecutionState(char[] tape, long rwHead, State state) {
        Tape = tape;
        ReadWriteHead = rwHead;
        State = state;
    }

    // Creates a new state meant to represent the beginning of execution
    public static ExecutionState Begin(TuringMachine tm, long tapeSize, string prefix = "", int headPosition = 0) {
        return new ExecutionState(CreateTape(prefix, tapeSize), headPosition, tm.InitialState);
    }

    public static ExecutionState Begin(TuringMachine tm, long tapeSize, int headPosition) {
        return Begin(tm, tapeSize, "", headPosition);
    }

    public void MoveHead(long amount) {
        ReadWriteHead += amount;
    }

    public void Transition(State newState) {
        State = newState;
    }    

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
}