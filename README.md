# Turing Machine VM

This is a Turing machine simulator I wrote which uses the same syntax as a similar simulator used for my Introduction to Computability class a few years ago. It takes in a program and an alphabet, performs rigourous static analysis to detect bugs ahead of time, and then runs it. It takes in programs in the following format:
```
state_name, current_letter -> next_state, letter_to_write, movement
```
If `current_letter` is `*`, then that means "any letter". If `letter_to_write` is `*` then that means it will write the same letter that it recieved (basically meaning that it won't write anything).

The static analysis performed ensures that the program conforms with the following rules:
- There may not be any syntax errors
- The only valid fields for `movement` are `l` (left), `r` (right), and `s` (stay)
- Every state must have a transition for every letter in the alphabet
- A state may not have a transition to a state that does not exist
- A state may not write a letter that is not part of the alphabet
- A program must contain a starting state "S"

### Example
The following program runs with the alphabet `ab`, takes in any string comprised of the letters `a` or `b`, and inserts an underscore between each letter.
```
S,* -> A,*,r
A,_ -> L,*,l
A,* -> A,*,r
B,a -> C,_,r
B,b -> F,_,r
B,_ -> hr,*,s
C,* -> D,a,l
F,* -> D,b,l
D,* -> G,*,l
G,* -> H,*,l
H,_ -> I,*,r
H,* -> A,*,s
I,* -> J,*,r
J,* -> A,*,r
E,_ -> A,*,r
E,* -> E,*,l
L,* -> M,*,l
M,_ -> ha,*,s
M,* -> B,*,r
```

### CLI

The Turing Machine VM works a bit like a debugger, and upon starting the program, you are shown a prompt to type commands. The commands are as follows:
- `load <filename> <alphabet>` - Loads a program at `filename` with a certain `alphabet`. Note that `_` is always implicitly part of the alphabet and need not be included.
- `run [tape contents] [memory size]` - Runs the currently loaded program, filling the tape with `tape contents` starting at **position one**. Note that the read-write head starts at position 0. The tape's size will be equal to `memory size`. All these arguments are optional.
- `debug <on|off>` - If `debug` is `on`, each step of running the Turing machine will be printed out. 
- `dump` - Prints the currently loaded program, *not* including any unused branches.
- `help` - Displays the in-program help message.
- `exit` - Exits the program.

From the command line, you can run `TuringMachineVM.exe [line1] [line2] ...` and it will automatically run each argument as though you wrote it yourself in the prompt.

## Building
Since it is a C# project (running .NET Core 3.1) it is recommended that you load the solution in Visual Studio and build it. Otherwise, you can run `msbuild.exe TuringMachineVM.sln`. Afterwards, you will find the binaries in `TuringMachineVM/bin/Debug/netcoreapp3.1/TuringMachineVM.exe`. If you create a release build, it will be in `TuringMachineVM/bin/Release/netcoreapp3.1/TuringMachineVM.exe`
