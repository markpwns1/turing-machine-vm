using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TuringMachineVM
{
    // Represents a command that can be run in the terminal
    public class Command
    {
        public string Name { get; private set; }
        public int MinArgs { get; private set; }
        public int MaxArgs { get; private set; }
        public string Usage { get; private set; }
        public string Description { get; private set; }

        // The action run when the command is called, with the arguments passed to it
        public Action<string[]> Action { get; private set; }

        public Command(string name, int minArgs, int maxArgs, string usage, string description, Action<string[]> action)
        {
            Name = name;
            MinArgs = minArgs;
            MaxArgs = maxArgs;
            Usage = usage;
            Action = action;
            Description = description;
        }

        // Runs the command with the given arguments
        public void Run(string[] args)
        {
            if (args.Length < MinArgs || args.Length > MaxArgs)
            {
                Console.WriteLine("Usage: " + Usage);
                return;
            }

            Action(args);
        }

        // Parses a line in the terminal, properly obeying things like quotes
        public static List<string> ParseLine(string command)
        {
            var tokens = new List<string>();

            var i = 0;
            string token = "";
            bool stringMode = false;

            // Standard tokenization loop, Google it if you're not familiar
            while (i < command.Length)
            {
                var c = command[i];
                if (stringMode)
                {
                    if (c == '"')
                    {
                        stringMode = false;
                        tokens.Add(token);
                        token = "";
                    }
                    else
                    {
                        token += c;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        stringMode = true;
                    }
                    else if (char.IsWhiteSpace(command[i]))
                    {
                        if (token.Length > 0)
                        {
                            tokens.Add(token);
                            token = "";
                        }
                    }
                    else
                    {
                        token += command[i];
                    }
                }

                i++;
            }

            if (token.Length > 0)
            {
                tokens.Add(token);
            }

            return tokens;

        }

        // Dispatches a terminal line to the appropriate command and runs it
        public static void Dispatch(string command, List<Command> availableCommands)
        {
            var tokens = ParseLine(command);
            if (tokens.Count == 0)
            {
                return;
            }

            var commandName = tokens[0];
            var args = tokens.Skip(1).ToArray();

            var found = availableCommands.FirstOrDefault(c => c.Name == commandName);
            if (found == null)
            {
                Console.WriteLine("Unknown command: " + commandName);
                return;
            }

            found.Run(args);
        }

        // The following are utility functions meant to be used in the action of a Command

        // Interprets an argument as a file and reads it
        public static string FileArgument(string filename, string argName = "filename")
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Error in argument `" + argName + "` - File not found: " + filename);
                return null;
            }

            return File.ReadAllText(filename);
        }

        // Interprets an argument as a long
        public static long LongArgument(string arg, string argName)
        {
            long result;
            if (!long.TryParse(arg, out result))
            {
                Console.WriteLine("Error in argument `" + argName + "` - Not an integer: " + arg);
                return -1;
            }

            return result;
        }
    }
}