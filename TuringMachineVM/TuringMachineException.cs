using System;

public class TuringMachineException : Exception {

    public TuringMachineException(string message) : base(message) { }

}

public class ParsingException : TuringMachineException {

    public ParsingException(int line, string message) : base("PARSING ERROR @ Ln " + line + " - " + message) { }

}

public class SemanticException : TuringMachineException {

    public SemanticException(int line, string message) : base("SEMANTIC ERROR @ Ln " + line + " - " + message) { }
    public SemanticException(string message) : base("SEMANTIC ERROR - " + message) { }

}