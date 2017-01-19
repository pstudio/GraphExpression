using System;
using GraphExpressionEvaluator.Compiler;
using GraphExpressionEvaluator.Interpreter;
using GraphExpressionEvaluator.Lexer;
using GraphExpressionEvaluator.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvaluatorTest
{
    [TestClass]
    public class CompilerTest
    {
        private Lexer _lexer;
        private Parser _parser;
        private Compiler _compiler;

        [TestInitialize]
        public void Init()
        {
            _lexer = new Lexer();
            _parser = new Parser();
            _compiler = new Compiler();
        }

        [TestMethod]
        public void TestCompile()
        {
            const string expression = "2+x";
            var tokens = _lexer.Tokenize(expression);
            var rpn = _parser.Parse(tokens);
            var bytecode = _compiler.Compile(rpn.Expression);

            Assert.IsTrue(bytecode.Length == 7);
            Assert.IsTrue(bytecode[0] == (byte)Interpreter.ByteCodeOp.Push);
            Assert.IsTrue(bytecode[5] == (byte)Interpreter.ByteCodeOp.PushX);
            Assert.IsTrue(bytecode[6] == (byte)Interpreter.ByteCodeOp.Add);

            if (BitConverter.IsLittleEndian)
            {
                Assert.IsTrue(BitConverter.ToSingle(bytecode, 1) == 2);
            }
        }
    }
}
