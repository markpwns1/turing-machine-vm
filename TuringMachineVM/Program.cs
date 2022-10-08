using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TuringMachineVM
{
    class Program
    {
        public const int DEFAULT_MEM_SIZE = 1024;
        static List<Command> commands = new List<Command>() {

            new Command("load", 2, 2, "load <filename> <alphabet>", args => {
                var source = Command.FileArgument(args[0]);
                if(source == null) return;

                try {
                    TM = new TuringMachine(source, args[1]);
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                }
            }),

            new Command("run", 0, 2, "run [tape contents] [memory size]", args => {
                
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

            new Command("debug", 1, 1, "debug <on|off>", args => {
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

            new Command("dump", 0, 0, "dump", args => {
                if(TM == null) {
                    Console.WriteLine("No program to dump");
                    return;
                }

                Console.WriteLine(TM.ToString());
            }),

            new Command("help", 0, 0, "help", args => {
                foreach(var command in commands)
                {
                    Console.WriteLine(command.Usage);
                }
            }),

            new Command("exit", 0, 0, "exit", args => {
                Environment.Exit(0);
            })
        };

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
                    Command.Dispatch(args[i], commands);
                }
                return;
            }

            while(true)
            {
                Console.Write(">>> ");
                var input = Console.ReadLine().Trim();
                Command.Dispatch(input, commands);
            }
        }
    }
}
