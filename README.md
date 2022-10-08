# CPSC 501 Assignment 1 
by Mark Kaldas

## Context
This is a Turing machine simulator, which uses the same syntax as a similar simulator used for my Introduction to Computability class a few years ago. It takes in a program and an alphabet, performs rigourous static analysis to detect bugs ahead of time, and then runs it. It takes in programs in the following format:
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

## Refactors
I'm not sure the best way to go about explaining these, so I'll just write some detailed information about each commit. These will be in order, and I will write them as I'm doing the assignment, so consider them a kind of "log". My preferred way of writing code is making a branch for any kind of major change, so expect lots of branches and merges.

### Branch `unit-test`
Here I created and checked out a new branch, `unit-test`

- `commit 8b0502e83a9f7219632fada8c8f64489363cfc50` - Here I wrote some unit tests to prepare for the refactoring. Most of the unit tests had to do with the static analysis, and after writing some unit tests, I actually discovered some bugs with the original implementation and fixed those so that the tests pass. I still haven't written many tests running the actual VM though. This was not a refactoring, so most of the questions outlined in the assignment document do not apply. I did however, mostly fix bugs related to static analysis in `TuringMachine.cs`

- `commit 68848604915229ccb1aa70acfae52570ed48774e` - I just finished the tests, specifically the part where you actually run the turing machine. I also had to add a way to run the turing machine one step at a time, so I moved out some code within `TuringMachine.Run` to `TuringMachine.Step`. I suppose this technically counts as the "long code" smell, which I fixed by extracting some of the method into a (very slightly) smaller method, but that wasn't really my intention. It was primarily to enable more effective unit testing, since this code would have to be used in two places: the `Run` function, and the unit tests, and I needed a way to isolate the body of the loop. So again, this does not really count as a refactoring, nor was it my intention, so forgive me for not writing half a page for this one.

### Branch `master`
Here I went back to the master branch for what I expected to be a few small changes.

- `commit 833cde4bcf123a1e8370d352f2ee58c3801472e8` - Here is the first real refactor I've done. There was a pretty big code smell permeating this entire project, and it's that the variables `tape`, `memPos`, and `state` are literally always used together, but for some reason were always separate. This would be considered a "data clump" code smell. You can see examples of this in `TuringMachine.Run`, `TuringMachine.Step`, and in `InterpreterTest` as well. One thing you might see is that I actually found it so convenient to group these 3 variables together, that I actually already wrote a struct to package them up in `InterpreterTest`, but of course, this was an ad-hoc change. The fact that this was in the test suite but not in the actual code could also be considered feature envy. So to remedy this, I wrote a class `ExecutionState` with a few helper functions as well, that package up these 3 variables into an easier to handle unit, which greatly simplified calling functions and passing the data. First and foremost, this would be an "introduce parameter object" refactor, and in some places where I store the fields, like in `TuringMachine.Result`, it might as well be "extract class". The code was tested afterwards against the test suite I already made to ensure that the program didn't break (but of course it did, and I had to fix it). As to whether this could enable more refactorings, I think maybe I could make it so that it's not even necessary for the user of the VM to manage the VM's state. It would probably be better for the VM to manage its own state and hide it from the user.

### Branch `parsing-overhaul`
After that I created a branch `parsing-overhaul`, which actually ended up being a parsing and semantic analysis overhaul.

#### `commit 2a560c961ca3fad1647d5da7a241ae12e359684b`

Consider this the first "large" refactor that's required according to the assignment spec. I first noticed a pretty large code smell, being that `TuringMachine` handles everything from parsing to static analysis to execution, so my first mission was to cut down on this. What I then did is essentially "extract class" on pretty much the entire parsing code contained in the constructor of `TuringMachine`, except there happened to already be good candidates for classes that could take on this complexity instead, so I moved that code to the functions `State.FromString` and `Transition.FromString` that I created, which parse a line and convert it to a State object (the definition of parsing). I also strictly split the parsing part and semantic analysis part of that code so that the `State` and `Transition` classes only do parsing, since those classes should not have any context that would allow them to do semantic analysis. That means semantic analysis is the domain of `TuringMachine`. 

Once again, the code was tested against the existing test suite, however some of my changes changed the front-facing interface, so the tests had to be altered to compile at all. While doing my refactor, I noticed that I made a mistake in one of my tests: I tested that an empty program successfully loaded, even though that should not be the case, since it lacks a starting state S. So I erased that test and fixed that bug. The code is now structered better because the parsing work is now done by the classes that specifically deal with parsing, rather than just `TuringMachine`, which not only makes more sense, but reduces the size of that massive `TuringMachine` class. 

As part of splitting parsing and semantic analysis, I also wrote some new classes, `TuringMachineException`, `ParsingException`, and `SemanticException`--the latter 2 being subclasses of `TuringMachineException`. This would make testing more accurate as I can identify that I'm getting the exact error that I should be.

### Branch `cli-overhaul`
After that I created a branch called `cli-overhaul` where I overhauled how commands were handled.

#### `commit ?`

Consider this the second "large" refactor. I noticed a horrible code smell, which was the fact that there was a giant method in `Program` that was just a giant if statement. This would count as a "switch statement" code smell according to Fowler (my if statement was pretty much equivalent to a switch statement). What I did to fix it was replace this statement with a list of `Command` objects, each one with a name, usage help string, minimum and maximum number of arguments, and a function that gets called whenever the command gets activated. Here is an example of one of the commands:

```cs
// new Command(name, minArgs, maxArgs, usage, action)
new Command("load", 2, 2, "load <filename> <alphabet>", args => {
    var source = Command.FileArgument(args[0]);
    if(source == null) return;

    try {
        TM = new TuringMachine(source, args[1]);
    } catch(Exception e) {
        Console.WriteLine(e.Message);
    }
}),
```
The program will then call `Command.Dispatch` which will automatically call a command given an input such a `load "abc.txt" "ab"`, and handle the cases where a command is not found, or there are too few or too many arguments. Furthermore, I "moved method" the command parsing code from `Program` to `Command`, because it was contributing to the "large class" code smell in `Program`. Note that I used a more functional programming-style approach to an object-oriented approach because I figured that if I made each command its own subclass of `Command`, it would clutter the filesystem, not be very appropriate since the commands aren't really different *types* of commands as much as different *instances* of the same kind of thing (a "command"). Also, in case it wasn't obvious, the entire `Command` class was the result of refactoring.

I ended up expanding the test suite to cover commands (see `CLITest.cs`), as well as ensuring that all the existing tests still pass. The code is better structered now because there is no longer a giant method with several branches and depths of if statements, and it is much more readable this way, as well as much easier to add new commands. I think this has been refactored quite well, so I can't think of any other refactorings this might enable.