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

## Refactors
I'm not sure the best way to go about explaining these, so I'll just write some detailed information about each commit.