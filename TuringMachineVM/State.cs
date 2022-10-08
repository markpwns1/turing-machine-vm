using System;
using System.Collections.Generic;
using System.Text;

namespace TuringMachineVM
{
    public class State
    {
        public string name;
        public readonly Dictionary<char, Transition> transitions = new Dictionary<char, Transition>();

        public State(string name)
        {
            this.name = name;
        }

        public string ToString(char trigger)
        {
            return name + ", " + trigger + " -> " + transitions[trigger].ToString();
        }

        public override string ToString()
        {
            string str = "";

            foreach (var trigger in transitions.Keys)
            {
                str += ToString(trigger) + "\n";
            }

            return str;
        }

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
