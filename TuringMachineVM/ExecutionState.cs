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
        return new ExecutionState(TuringMachine.CreateTape(prefix, tapeSize), headPosition, tm.InitialState);
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
}