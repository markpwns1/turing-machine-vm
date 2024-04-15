using System.Collections.Generic;

namespace TuringMachineVM
{
    // A class to perform static analysis on Turing machine programs
    public static class TuringMachineValidation
    {
        // Name of the starting state of a Turing machine program
        private const string STARTING_STATE = "S";

        // Validates a given state in the Turing machine and any other state it could possible transition to
        // The 'visited' list is a list of all states that will not be validated again (to prevent stack overflow)
        private static void ValidateStates(TuringMachine tm, List<string> visited, string name)
        {
            var next = tm.States[name]; // The state to validate

            // If the state doesn't have a catch-all trigger, *, then check all its triggers to make sure
            // no letter of the alphabet is missing

            if (!next.transitions.ContainsKey('*'))
            {
                foreach (var letter in tm.Alphabet)
                {
                    if (!next.transitions.ContainsKey(letter))
                        throw new SemanticException("State " + name + " does not have a transition for letter " + letter);
                }
            }

            visited.Add(name); // Add to the visited list to prevent re-validation

            // Ensure that any states transitioned to actually exist, and then validate them

            foreach (var effect in next.transitions.Values)
            {
                if (!visited.Contains(effect.next) && effect.next != TuringMachine.ACCEPT && effect.next != TuringMachine.REJECT)
                {
                    if (!tm.States.ContainsKey(effect.next))
                        throw new SemanticException(effect.line, "State " + effect.next + " does not exist");

                    ValidateStates(tm, visited, effect.next);
                }
            }
        }

        // Ensures that all states transition to valid states, and that all states handle all characters in the alphabet.
        // Returns the initial state of the Turing machine, that being the state named after the value of STARTING_STATE
        private static State ValidateStates(TuringMachine tm)
        {
            // Gets the starting state if it exists, then passes execution to 
            // the other ValidateStates, branching off from the starting state

            var visited = new List<string>();

            if (!tm.States.ContainsKey(STARTING_STATE))
                throw new SemanticException("VM does not contain a start state");

            var initialState = tm.States[STARTING_STATE];

            ValidateStates(tm, visited, STARTING_STATE);

            return initialState;
        }

        // Ensures that no state writes a character that is not in the alphabet
        private static void ValidateAlphabet(TuringMachine tm)
        {
            foreach (var state in tm.States.Values)
            {
                foreach (var trigger in state.transitions.Keys)
                {
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

        // Validates a Turing Machine using a variety of static analysis methods
        public static State Validate(TuringMachine tm)
        {
            ValidateAlphabet(tm);
            return ValidateStates(tm);
        }
    }
}