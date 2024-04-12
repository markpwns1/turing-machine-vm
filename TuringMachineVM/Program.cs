using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TuringMachineVM
{
    class Program
    {
        // Tape size for all programs
        public const int DEFAULT_MEM_SIZE = 1024;

        // List of commands. See their descriptions for what they do. The final argument is the code they run
        static List<Command> commands = new List<Command>() {

            new Command("load", 2, 2, "load <filename> <alphabet>", "loads a program to later be run or debugged", args => {
                var source = Command.FileArgument(args[0]);
                if(source == null) return;

                try {
                    TM = new TuringMachine(source, args[1]);
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                }
            }),

            new Command("run", 0, 2, "run [tape contents] [memory size]", "runs the currently loaded program", args => {
                
                if(TM == null) {
                    Console.WriteLine("No Turing machine loaded");
                    return;
                }

                if(args.Length < 1) {
                    lastResult = TM.Run();
                }
                else if(args.Length < 2) {
                    lastResult = TM.Run(args[0], DEFAULT_MEM_SIZE);
                }
                else {
                    var memSize = Command.LongArgument(args[1], "memory size");
                    if(memSize == -1) return;
                    lastResult = TM.Run(args[0], memSize);
                }

                Console.WriteLine(lastResult.Value.ToString());
            }),

            new Command("debug", 1, 1, "debug <on|off>", "toggles debug mode", args => {
                if(args[0] == "on") {
                    TuringMachine.verbose = true;
                }
                else if(args[0] == "off") {
                    TuringMachine.verbose = false;
                }
                else {
                    Console.WriteLine("Usage: debug <on|off>");
                }
            }),

            new Command("dump", 0, 0, "dump", "outputs the current program to a string", args => {
                if(TM == null) {
                    Console.WriteLine("No program to dump");
                    return;
                }

                Console.WriteLine(TM.ToString());
            }),

            new Command("help", 0, 0, "help", "displays all the available commands", args => {
                foreach(var command in commands)
                {
                    Console.WriteLine(command.Usage + "\n  " + command.Description);
                }
            }),

            new Command("exit", 0, 0, "exit", "exits this virtual machine", args => {
                Environment.Exit(0);
            })
        };

        // Current Turing machine and last execution result
        static TuringMachine TM = null;
        static ExecutionResult? lastResult = null;

        // Entry point
        static void Main(string[] args)
        {
            // The TMVM can be run with -c <command> arguments to automatically execute commands
            if(args.Length == 1)
            {
                Console.WriteLine("Usage: tmvm [-c <command>+]");
                return;
            }

            Console.WriteLine("Turing Machine VM");

            // Automatically execute the commands given as arguments
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    Console.WriteLine(">>> " + args[i]);
                    Command.Dispatch(args[i], commands);
                }
                return;
            }

            // REPL
            while(true)
            {
                Console.Write(">>> ");
                var input = Console.ReadLine().Trim();
                Command.Dispatch(input, commands);
            }
        }
    }
}
