
public struct ExecutionResult
{
    public ExecutionState FinalState { get; }
    public bool Accepted { get; }

    public ExecutionResult(ExecutionState finalState, bool accepted)
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
        var str = FinalState.TapeToString();
        str += "\n" + (Accepted ? "Accepted" : "Rejected");
        return str;
    }
}