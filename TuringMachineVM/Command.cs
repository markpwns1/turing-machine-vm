using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class Command {
    public string Name { get; private set; }
    public int MinArgs { get; private set; }
    public int MaxArgs { get; private set; }
    public string Usage { get; private set; }

    public Action<string[]> Action { get; private set; }

    public Command(string name, int minArgs, int maxArgs, string usage, Action<string[]> action) {
        Name = name;
        MinArgs = minArgs;
        MaxArgs = maxArgs;
        Usage = usage;
        Action = action;
    }

    public void Run(string[] args) {
        if (args.Length < MinArgs || args.Length > MaxArgs) {
            Console.WriteLine("Usage: " + Usage);
            return;
        }

        Action(args);
    }

    public static List<string> ParseLine(string command) {
        var tokens = new List<string>();

        var i = 0;
        string token = "";
        bool stringMode = false;

        while(i < command.Length)
        {
            var c = command[i];
            if (stringMode)
            {
                if(c == '"')
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
                if(c == '"')
                {
                    stringMode = true;
                }
                else if(char.IsWhiteSpace(command[i]))
                {
                    if(token.Length > 0)
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

    public static void Dispatch(string command, List<Command> availableCommands) {
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

    public static string FileArgument(string filename, string argName = "filename") {
        if (!File.Exists(filename))
        {
            Console.WriteLine("Error in argument `" + argName + "` - File not found: " + filename);
            return null;
        }

        return File.ReadAllText(filename);
    }

    public static long LongArgument(string arg, string argName) {
        long result;
        if (!long.TryParse(arg, out result))
        {
            Console.WriteLine("Error in argument `" + argName + "` - Not an integer: " + arg);
            return -1;
        }

        return result;
    }
}