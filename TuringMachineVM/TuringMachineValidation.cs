using TuringMachineVM;
using System.Collections.Generic;

public static class TuringMachineValidation {
    private static void ValidateStates(TuringMachine tm, List<string> visited, string name)
    {
        var next = tm.States[name];

        if (!next.transitions.ContainsKey('*'))
        {
            foreach (var letter in tm.Alphabet)
            {
                if(!next.transitions.ContainsKey(letter))
                    throw new SemanticException("State " + name + " does not have a transition for letter " + letter);
            }
        }

        visited.Add(name);

        foreach(var effect in next.transitions.Values)
        {
            if (!visited.Contains(effect.next) && effect.next != TuringMachine.ACCEPT && effect.next != TuringMachine.REJECT) {
                if(!tm.States.ContainsKey(effect.next))
                    throw new SemanticException(effect.line, "State " + effect.next + " does not exist");

                ValidateStates(tm, visited, effect.next);
            }
        }
    }

    private static State ValidateStates(TuringMachine tm)
    {
        var visited = new List<string>();

        if (!tm.States.ContainsKey("S"))
            throw new SemanticException("VM does not contain a start state");

        var initialState = tm.States["S"];

        ValidateStates(tm, visited, "S");

        return initialState;
    }

    private static void ValidateAlphabet(TuringMachine tm) {
        foreach (var state in tm.States.Values)
        {
            foreach (var trigger in state.transitions.Keys) {
                if (trigger != '*' && !tm.Alphabet.Contains(trigger))
                    throw new SemanticException(state.transitions[trigger].line, "State " + state.name + " has a transition for letter " + trigger + " which is not in the alphabet");
            }

            foreach (var effect in state.transitions.Values)
            {
                if (effect.write != '*' && !tm.Alphabet.Contains(effect.write))
                    throw new SemanticException(effect.line, "State " + state.name + " writes " + effect.write + " which is not in the alphabet");
            }
        }
    }

    public static State Validate(TuringMachine tm)
    {
        ValidateAlphabet(tm);
        return ValidateStates(tm);
    }
}