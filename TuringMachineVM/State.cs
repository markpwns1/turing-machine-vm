using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    // Represents a state in the Turing Machine
    public class State
    {
        public string name;

        // Represents a state's list of <character encountered> -> <transition action>
        public readonly Dictionary<char, Transition> transitions = new Dictionary<char, Transition>();

        public State(string name)
        {
            this.name = name;
        }

        // Given a trigger character, outputs the part of this state containing the transition for it 
        // For example: S, * -> ha, *, s
        public string ToString(char trigger)
        {
            return name + ", " + trigger + " -> " + transitions[trigger].ToString();
        }

        // Returns the entire state and all its transitions in a string formatted as valid source code
        public override string ToString()
        {
            string str = "";

            foreach (var trigger in transitions.Keys)
            {
                str += ToString(trigger) + "\n";
            }

            return str;
        }

        // Parses a line in the format of "name, trigger -> next_state, to_write, movement", adds the
        // transition to the the state if it already exists (otherwise creates a new state), and returns
        // the state
        public static State FromString(Dictionary<string, State> states, string str, int line)
        {
            var badLeftHand = new ParsingException(line, "Invalid state format: " + str + ". Expected format <state>, <character> -> <next state>, <write>, <movement>");

            var parts = str.Split("->");
            
            if (parts.Length != 2) throw badLeftHand;

            var leftHand = parts[0].Trim().Split(",");

            if (leftHand.Length != 2) throw badLeftHand;
            
            var stateName = leftHand[0].Trim();
            var trigger = leftHand[1].Trim();

            if (trigger.Length != 1) throw new ParsingException(line, "Invalid state format: " + str + ". Trigger must be a single character");

            State state;
            if (states.ContainsKey(stateName)) {
                state = states[stateName];
            } else {
                state = new State(stateName);
                states.Add(stateName, state);
            }

            state.transitions.Add(trigger[0], Transition.FromString(parts[1].Trim(), line));

            return states[stateName];
        } 
    }
}
