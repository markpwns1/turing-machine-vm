using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TuringMachineVM
{
    class Program
    {
        static List<string> Parse(string command)
        {
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

        static TuringMachine TM = null;
        static TuringMachine.Result? lastResult = null;

        static void Main(string[] args)
        {
            if(args.Length == 1)
            {
                Console.WriteLine("Usage: tmvm [-c <command>+]");
                return;
            }

            Console.WriteLine("Turing Machine VM");
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    Console.WriteLine(">>> " + args[i]);
                    ExecuteCommand(Parse(args[i]));
                }
                return;
            }

            while(true)
            {
                Console.Write(">>> ");
                var input = Console.ReadLine().Trim();
                var segments = Parse(input);

                ExecuteCommand(segments);
            }
        }

        private static void ExecuteCommand(List<string> segments)
        {
            if (segments[0].ToLower() == "load")
            {
                if (segments.Count < 3)
                {
                    Console.WriteLine("Usage: load <file> <alphabet>");
                    return;
                }

                if (!File.Exists(segments[1]))
                {
                    Console.WriteLine("File not found: " + segments[1]);
                    return;
                }

                try
                {
                    TM = new TuringMachine(File.ReadAllText(segments[1]), segments[2]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            else if (segments[0].ToLower() == "run")
            {
                if (TM == null)
                {
                    Console.WriteLine("Load a program first");
                    return;
                }

                try
                {
                    if (segments.Count < 2)
                    {
                        lastResult = TM.Run();
                    }
                    else if (segments.Count < 3)
                    {
                        lastResult = TM.Run(segments[1], 4096);
                    }
                    else
                    {
                        if (long.TryParse(segments[2], out long memsize))
                        {
                            lastResult = TM.Run(segments[1], memsize);
                        }
                        else
                        {
                            Console.WriteLine("Memory size must be a long");
                            return;
                        }
                    }

                    Console.WriteLine(lastResult.Value.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (segments[0].ToLower() == "debug")
            {
                if (TM == null)
                {
                    Console.WriteLine("Load a program first");
                    return;
                }

                if (segments.Count < 2)
                {
                    Console.WriteLine("Usage: debug <on|off>");
                    return;
                }

                if (segments[1].ToLower() == "on")
                {
                    TuringMachine.verbose = true;
                }
                else if (segments[1].ToLower() == "off")
                {
                    TuringMachine.verbose = false;
                }
                else
                {
                    Console.WriteLine("Usage: debug <on|off>");
                    return;
                }
            }
            else if (segments[0].ToLower() == "dump")
            {
                if (TM == null)
                {
                    Console.WriteLine("Load a program first");
                    return;
                }

                Console.WriteLine(TM.ToString());
            }
            else if (segments[0].ToLower() == "quit")
            {
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Unknown command");
            }
        }

    }
}
