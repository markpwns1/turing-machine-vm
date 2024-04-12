using System;

namespace TuringMachineVM
{
    // A set of exceptions for use in the Turing Machine

    // General exception
    public class TuringMachineException : Exception
    {

        public TuringMachineException(string message) : base(message) { }

    }

    // Exception parsing the source code
    public class ParsingException : TuringMachineException
    {

        public ParsingException(int line, string message) : base("PARSING ERROR @ Ln " + line + " - " + message) { }

    }

    // Exception running the program
    public class SemanticException : TuringMachineException
    {

        public SemanticException(int line, string message) : base("SEMANTIC ERROR @ Ln " + line + " - " + message) { }
        public SemanticException(string message) : base("SEMANTIC ERROR - " + message) { }

    }
}