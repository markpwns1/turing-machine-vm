using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TuringMachineVM;

namespace TuringMachineVMTest
{
    [TestClass]
    public class InterpreterTest
    {
        // Accepts even-length inputs and rejects odd-length inputs
        private const string ACCEPT_EVENS = @"
            S,* -> E,*,r
            E,_ -> ha,*,s
            E,* -> O,*,r
            O,_ -> hr,*,s
            O,* -> E,*,r
        ";

        // Inserts a space between every character of the input
        private const string SPACES = @"
            S,* -> A,*,r
            A,_ -> L,*,l
            A,* -> A,*,r
            B,a -> C,_,r
            B,b -> F,_,r
            B,* -> hr,*,s
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
        ";

        // Default turing machine tape size
        private const long TAPE_SIZE = 1024;

        // Run a turing machine with the following source code, input, and alphabet
        // and return the result
        private TuringMachine.Result Run(string source, string input, string alphabet) {
            var tm = new TuringMachine(source, alphabet);
            return tm.Run(input, TAPE_SIZE);
        }

        // Run a turing machine and assert that it accepts
        private void AssertAccepts(string source, string input, string alphabet) {
            Assert.IsTrue(Run(source, input, alphabet).Accepted, "Expected to accept");
        }

        // Run a turing machine and assert that it rejects
        private void AssertRejects(string source, string input, string alphabet) {
            Assert.IsFalse(Run(source, input, alphabet).Accepted, "Expected to reject");
        }

        [TestMethod]
        public void InstantAccept()
        {
            AssertAccepts("S,* -> ha,*,s", "", "");
        }

        [TestMethod]
        public void InstantReject()
        {
            AssertRejects("S,* -> hr,*,s", "", "");
        }

        [TestMethod]
        public void NonTrivialAccept()
        {
            AssertAccepts(ACCEPT_EVENS, "aaaa", "a");
            AssertAccepts(ACCEPT_EVENS, "", "a");
            AssertAccepts(ACCEPT_EVENS, "aaaaaaaaaa", "a");
        }

        [TestMethod]
        public void NonTrivialReject()
        {
            AssertRejects(ACCEPT_EVENS, "aaa", "a");
            AssertRejects(ACCEPT_EVENS, "a", "a");
            AssertRejects(ACCEPT_EVENS, "aaaaaaaaa", "a");
        }

        [TestMethod]
        public void TapeModification() {
            var tm = new TuringMachine(SPACES, "ab");
            var result = tm.Run("aaaaa", 16);
            Assert.AreEqual(result.DumpTape(), "_a_a_a_a_a______");
        }

        [TestMethod]
        public void Movement1() {
            var tm = new TuringMachine(@"
                S,* -> A,*,r
                A,_ -> ha,*,s
            ", "");

            var state = ExecutionState.Begin(tm, TAPE_SIZE);
            tm.Step(state);

            Assert.AreEqual(state.ReadWriteHead, 1);
        }

        [TestMethod]
        public void Movement2() {
            var tm = new TuringMachine(@"
                S,* -> A,*,l
                A,_ -> ha,*,s
            ", "");

            var state = ExecutionState.Begin(tm, TAPE_SIZE, 1);
            tm.Step(state);

            Assert.AreEqual(state.ReadWriteHead, 0);
        }

        [TestMethod]
        public void Movement3() {
            var tm = new TuringMachine(@"
                S,* -> A,*,s
                A,_ -> ha,*,s
            ", "");

            var state = ExecutionState.Begin(tm, TAPE_SIZE);
            tm.Step(state);

            Assert.AreEqual(state.ReadWriteHead, 0);
        }

        [TestMethod]
        public void Write1() {
            var tm = new TuringMachine("S,* -> ha,a,r", "a");

            var state = ExecutionState.Begin(tm, TAPE_SIZE);
            tm.Step(state);

            Assert.AreEqual(state.Tape[0], 'a');
        }

        [TestMethod]
        public void Write2() {
            var tm = new TuringMachine("S,* -> ha,*,r", "a");

            var state = ExecutionState.Begin(tm, TAPE_SIZE, "a", 1);
            tm.Step(state);

            Assert.AreEqual(state.Tape[1], 'a');
        }

        [TestMethod]
        public void Transition1() {
            var tm = new TuringMachine(@"
                S,* -> A,*,r
                A,_ -> ha,*,s
            ", "");

            var state = ExecutionState.Begin(tm, TAPE_SIZE);
            tm.Step(state);

            Assert.AreEqual(state.State.name, "A");
        }

        // Recursive transition
        [TestMethod]
        public void Transition2() {
            var tm = new TuringMachine(@"
                S,* -> S,*,s
            ", "");

            var state = ExecutionState.Begin(tm, TAPE_SIZE);
            tm.Step(state);

            Assert.AreEqual(state.State.name, "S");
        }
    }
}
