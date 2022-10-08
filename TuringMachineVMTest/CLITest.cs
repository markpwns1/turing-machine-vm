
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TuringMachineVM;

namespace TuringMachineVMTest
{
    [TestClass]
    public class CLITest
    {
        private static string buffer = "";
        public static List<Command> commands = new List<Command>() {
            new Command("a", 0, 0, "a", args => {
                buffer = "a";
            }),

            new Command("b", 1, 1, "b <arg>", args => {
                buffer = "b " + args[0];
            }),

            new Command("c", 0, 2, "c [arg1] [arg2]", args => {
                buffer = "c";
                if(args.Length > 0) buffer += " " + args[0];
                if(args.Length > 1) buffer += " " + args[1];
            }),
        };

        [TestInitialize]
        public void Init()
        {
            buffer = "";
        }

        [TestMethod]
        public void CommandA()
        {
            Command.Dispatch("a", commands);
            Assert.AreEqual("a", buffer);
        }

        [TestMethod]
        public void CommandB()
        {
            Command.Dispatch("b \"hello world\"", commands);
            Assert.AreEqual("b hello world", buffer);
        }

        [TestMethod]
        public void CommandC1()
        {
            Command.Dispatch("c \"hello world\" \"goodbye world\"", commands);
            Assert.AreEqual("c hello world goodbye world", buffer);
        }

        [TestMethod]
        public void CommandC2()
        {
            Command.Dispatch("c \"hello world\"", commands);
            Assert.AreEqual("c hello world", buffer);
        }

        [TestMethod]
        public void CommandC3()
        {
            Command.Dispatch("c", commands);
            Assert.AreEqual("c", buffer);
        }

        [TestMethod]
        public void Parse1() {
            CollectionAssert.AreEqual(Command.ParseLine("a"), new List<string> { "a" });
        }

        [TestMethod]
        public void Parse2() {
            CollectionAssert.AreEqual(Command.ParseLine(""), new List<string> { });
        }

        [TestMethod]
        public void Parse3() {
            CollectionAssert.AreEqual(Command.ParseLine("a b"), new List<string> { "a", "b" });
        }

        [TestMethod]
        public void Parse4() {
            CollectionAssert.AreEqual(Command.ParseLine("a \"b c\""), new List<string> { "a", "b c" });
        }
    }
}