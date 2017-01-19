using System;
using System.Linq;
using GraphExpressionEvaluator.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvaluatorTest
{
    [TestClass]
    public class LexerTest
    {
        private Lexer _lexer;

        [TestInitialize]
        public void Init()
        {
            _lexer = new Lexer();
        }

        [TestMethod]
        public void TestScan()
        {
            const string expression = "2x-3,14/2";
            var lexemes = _lexer.Scan(expression);

            Assert.IsTrue(lexemes.Count() == 6);
            Assert.IsTrue(lexemes[0].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[1].Item2 == Token.TokenType.Variable);
            Assert.IsTrue(lexemes[2].Item2 == Token.TokenType.Subtraction);
            Assert.IsTrue(lexemes[3].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[4].Item2 == Token.TokenType.Division);
            Assert.IsTrue(lexemes[5].Item2 == Token.TokenType.Number);
        }

        [TestMethod]
        public void TestScanExtended()
        {
            const string expression = "2x - 2(3,14 + x)/ 3 *3*X^2";
            var lexemes = _lexer.Scan(expression);

            Assert.IsTrue(lexemes.Count == 17);
            Assert.IsTrue(lexemes[0 ].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[1 ].Item2 == Token.TokenType.Variable);
            Assert.IsTrue(lexemes[2 ].Item2 == Token.TokenType.Subtraction);
            Assert.IsTrue(lexemes[3 ].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[4 ].Item2 == Token.TokenType.OpenParenthesis);
            Assert.IsTrue(lexemes[5 ].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[6 ].Item2 == Token.TokenType.Addition);
            Assert.IsTrue(lexemes[7 ].Item2 == Token.TokenType.Variable);
            Assert.IsTrue(lexemes[8 ].Item2 == Token.TokenType.CloseParenthesis);
            Assert.IsTrue(lexemes[9 ].Item2 == Token.TokenType.Division);
            Assert.IsTrue(lexemes[10].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[11].Item2 == Token.TokenType.Multiplication);
            Assert.IsTrue(lexemes[12].Item2 == Token.TokenType.Number);
            Assert.IsTrue(lexemes[13].Item2 == Token.TokenType.Multiplication);
            Assert.IsTrue(lexemes[14].Item2 == Token.TokenType.Variable);
            Assert.IsTrue(lexemes[15].Item2 == Token.TokenType.Power);
            Assert.IsTrue(lexemes[16].Item2 == Token.TokenType.Number);
        }

        [TestMethod]
        public void TestEvaluate()
        {
            const string expression = "2x-3,14/2";
            var lexemes = _lexer.Scan(expression);
            var tokens = _lexer.Evaluate(lexemes);

            Assert.IsTrue(tokens.Count() == 6);
            Assert.IsTrue(tokens[0].Type == Token.TokenType.Number);
            Assert.IsTrue(tokens[1].Type == Token.TokenType.Variable);
            Assert.IsTrue(tokens[2].Type == Token.TokenType.Subtraction);
            Assert.IsTrue(tokens[3].Type == Token.TokenType.Number);
            Assert.IsTrue(tokens[4].Type == Token.TokenType.Division);
            Assert.IsTrue(tokens[5].Type == Token.TokenType.Number);
        }

        [TestMethod]
        public void TestEvaluateExtended()
        {
            const string expression = "2x - 2(3,14 + x)/ 3 *3*X^2";
            var lexemes = _lexer.Scan(expression);
            var tokens = _lexer.Evaluate(lexemes);

            Assert.IsTrue(lexemes.Count == 17);
            Assert.IsTrue(tokens[0].Type== Token.TokenType.Number);
            Assert.IsTrue(tokens[1].Type== Token.TokenType.Variable);
            Assert.IsTrue(tokens[2].Type== Token.TokenType.Subtraction);
            Assert.IsTrue(tokens[3].Type== Token.TokenType.Number);
            Assert.IsTrue(tokens[4].Type== Token.TokenType.OpenParenthesis);
            Assert.IsTrue(tokens[5].Type== Token.TokenType.Number);
            Assert.IsTrue(tokens[6].Type== Token.TokenType.Addition);
            Assert.IsTrue(tokens[7].Type== Token.TokenType.Variable);
            Assert.IsTrue(tokens[8].Type== Token.TokenType.CloseParenthesis);
            Assert.IsTrue(tokens[9].Type== Token.TokenType.Division);
            Assert.IsTrue(tokens[10].Type == Token.TokenType.Number);
            Assert.IsTrue(tokens[11].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(tokens[12].Type == Token.TokenType.Number);
            Assert.IsTrue(tokens[13].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(tokens[14].Type == Token.TokenType.Variable);
            Assert.IsTrue(tokens[15].Type == Token.TokenType.Power);
            Assert.IsTrue(tokens[16].Type == Token.TokenType.Number);
        }
    }
}
