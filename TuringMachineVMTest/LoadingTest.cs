using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TuringMachineVM;

namespace TuringMachineVMTest
{
    // Tests the Turing Machine VM as it relates to loading a program.
    // This includes parsing and static analysis
    
    [TestClass]
    public class LoadingTest
    {

        [TestMethod]
        public void SyntaxError1() {
            Assert.ThrowsException<Exception>(() => new TuringMachine("a", ""));
        }

        [TestMethod]
        public void SyntaxError2() {
            // 3 arguments on left side
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,*,a -> ha,*,s", ""));
        }

        [TestMethod]
        public void SyntaxError3() {
            // 4 arguments on right side
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,* -> ha,*,s,o", ""));
        }

        [TestMethod]
        public void SyntaxError4() {
            Assert.ThrowsException<Exception>(() => new TuringMachine("a -> b", ""));
        }

        [TestMethod]
        public void SyntaxError5() {
            // "q" is not a valid movement
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,* -> ha,*,q", ""));
        }

        [TestMethod]
        public void SyntaxSuccess1() {
            new TuringMachine("", "");
        }

        [TestMethod]
        public void SyntaxSuccess2() {
            new TuringMachine("S,* -> ha,*,s", "");
        }

        [TestMethod]
        public void InvalidAlphabet1() {
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,* -> ha,a,s", ""));
        }

        [TestMethod]
        public void InvalidAlphabet2() {
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,* -> ha,a,s", "bc"));
        }

        [TestMethod]
        public void InvalidAlphabet3() {
            Assert.ThrowsException<Exception>(() => new TuringMachine(@"
                S,a -> ha,*,s
                S,* -> ha,*,s
            ", ""));
        }

        [TestMethod]
        public void InvalidAlphabet4() {
            Assert.ThrowsException<Exception>(() => new TuringMachine(@"
                S,a -> ha,*,s
                S,* -> ha,*,s
            ", "bc"));
        }

        public void AlphabetSuccess() {
            new TuringMachine("S,* -> ha,a,s", "a");
        }

        [TestMethod]
        public void NoStartState() {
            Assert.ThrowsException<Exception>(() => new TuringMachine("A,* -> ha,*,s", "").Verify());
        }

        [TestMethod]
        public void InvalidState()
        {
            // State A does not exist
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,* -> A,*,s", "").Verify());
        }

        [TestMethod]
        public void IncompleteState() {
            // State S does not have a transition for input '_'
            Assert.ThrowsException<Exception>(() => new TuringMachine("S,a -> ha,*,s", "a").Verify());
        }

        [TestMethod]
        public void CompleteState1() {
            new TuringMachine(@"
                S,a -> ha,*,s
                S,_ -> ha,*,s
            ", "a").Verify();
        }

        [TestMethod]
        public void CompleteState2() {
            new TuringMachine("S,* -> ha,*,s", "abcefg").Verify();
        }

        [TestMethod]
        public void CompleteState3() {
            new TuringMachine(@"
                S,a -> hr,*,s
                S,b -> hr,*,s
                S,c -> hr,*,s
                S,_ -> hr,*,s
            ", "abc").Verify();
        }
    }
}
