using System;
using GraphExpressionEvaluator.Compiler;
using GraphExpressionEvaluator.Interpreter;
using GraphExpressionEvaluator.Lexer;
using GraphExpressionEvaluator.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvaluatorTest
{
    [TestClass]
    public class InterPreterTest
    {
        private Lexer _lexer;
        private Parser _parser;
        private Compiler _compiler;
        private Interpreter _interpreter;

        [TestInitialize]
        public void Init()
        {
            _lexer = new Lexer();
            _parser = new Parser();
            _compiler = new Compiler();
            _interpreter = new Interpreter();
        }

        [TestMethod]
        public void TestEvaluate()
        {
            const string expression = "x^2+2x+4";
            var tokens = _lexer.Tokenize(expression);
            var rpn = _parser.Parse(tokens);
            var code = _compiler.Compile(rpn);

            const float tolerance = float.Epsilon;
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code, 0) - 4) < tolerance);
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code, 1) - 7) < tolerance);
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code, 2) - 12) < tolerance);
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code, 3) - 19) < tolerance);
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code,-1) - 3) < tolerance);
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code,-2) - 4) < tolerance);
            Assert.IsTrue(Math.Abs(_interpreter.Evaluate(code,-3) - 7) < tolerance);
        }

        [TestMethod]
        public void TestDivisionByZero()
        {
            string expression = "2/x";
            var tokens = _lexer.Tokenize(expression);
            var rpn = _parser.Parse(tokens);
            var code = _compiler.Compile(rpn);

            try
            {
                _interpreter.Evaluate(code, 0);
                Assert.Fail();
            }
            catch (Interpreter.InterpreterDivideByZeroException)
            {
                
            }

            expression = "x/(2-x)";
            tokens = _lexer.Tokenize(expression);
            rpn = _parser.Parse(tokens);
            code = _compiler.Compile(rpn);

            try
            {
                _interpreter.Evaluate(code, 2);
                Assert.Fail();
            }
            catch (Interpreter.InterpreterDivideByZeroException)
            {

            }
        }
    }
}
