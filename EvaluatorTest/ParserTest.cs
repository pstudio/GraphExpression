using System;
using System.Collections.Generic;
using GraphExpressionEvaluator.Lexer;
using GraphExpressionEvaluator.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvaluatorTest
{
    [TestClass]
    public class ParserTest
    {
        private Lexer _lexer;
        private Parser _parser;

        [TestInitialize]
        public void Init()
        {
            _lexer = new Lexer();
            _parser = new Parser();
        }

        [TestMethod]
        public void TestImplicitMultiplicationExpansion()
        {
            const string expression = "2x(2+x)3+1";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);
            var expandedTokens = privateParser.Invoke("ExpandImplicitTokens", tokens) as IList<Token>;

            // expected expanded expression = 2*x*(2+x)*3+1
            Assert.IsTrue(expandedTokens?.Count == 13);
            Assert.IsTrue(expandedTokens[0 ].Type == Token.TokenType.Number);
            Assert.IsTrue(expandedTokens[1 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(expandedTokens[2 ].Type == Token.TokenType.Variable);
            Assert.IsTrue(expandedTokens[3 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(expandedTokens[4 ].Type == Token.TokenType.OpenParenthesis);
            Assert.IsTrue(expandedTokens[5 ].Type == Token.TokenType.Number);
            Assert.IsTrue(expandedTokens[6 ].Type == Token.TokenType.Addition);
            Assert.IsTrue(expandedTokens[7 ].Type == Token.TokenType.Variable);
            Assert.IsTrue(expandedTokens[8 ].Type == Token.TokenType.CloseParenthesis);
            Assert.IsTrue(expandedTokens[9 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(expandedTokens[10].Type == Token.TokenType.Number);
            Assert.IsTrue(expandedTokens[11].Type == Token.TokenType.Addition);
            Assert.IsTrue(expandedTokens[12].Type == Token.TokenType.Number);
        }

        [TestMethod]
        public void TestUnaryMinusHandling()
        {
            const string expression = "-2*x*(-x+2)-4";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);
            var handledTokens = privateParser.Invoke("HandleUnaryPlusMinus", tokens) as IList<Token>;

            // expected expanded expression = {-2}*x*({-1}*x+2)-4
            Assert.IsTrue(handledTokens.Count == 13);
            Assert.IsTrue(handledTokens[0 ].Type == Token.TokenType.Number);
            Assert.IsTrue(handledTokens[1 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(handledTokens[2 ].Type == Token.TokenType.Variable);
            Assert.IsTrue(handledTokens[3 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(handledTokens[4 ].Type == Token.TokenType.OpenParenthesis);
            Assert.IsTrue(handledTokens[5 ].Type == Token.TokenType.Number);
            Assert.IsTrue(handledTokens[6 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(handledTokens[7 ].Type == Token.TokenType.Variable);
            Assert.IsTrue(handledTokens[8 ].Type == Token.TokenType.Addition);
            Assert.IsTrue(handledTokens[9 ].Type == Token.TokenType.Number);
            Assert.IsTrue(handledTokens[10].Type == Token.TokenType.CloseParenthesis);
            Assert.IsTrue(handledTokens[11].Type == Token.TokenType.Subtraction);
            Assert.IsTrue(handledTokens[12].Type == Token.TokenType.Number);
        }

        [TestMethod]
        public void TestUnaryPlusHandling()
        {
            const string expression = "+2*x*(+x+2)-4";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);
            var handledTokens = privateParser.Invoke("HandleUnaryPlusMinus", tokens) as IList<Token>;

            // expected expanded expression = 2*x*(x+2)-4
            Assert.IsTrue(handledTokens.Count == 11);
            Assert.IsTrue(handledTokens[0 ].Type == Token.TokenType.Number);
            Assert.IsTrue(handledTokens[1 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(handledTokens[2 ].Type == Token.TokenType.Variable);
            Assert.IsTrue(handledTokens[3 ].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(handledTokens[4 ].Type == Token.TokenType.OpenParenthesis);
            Assert.IsTrue(handledTokens[5 ].Type == Token.TokenType.Variable);
            Assert.IsTrue(handledTokens[6 ].Type == Token.TokenType.Addition);
            Assert.IsTrue(handledTokens[7 ].Type == Token.TokenType.Number);
            Assert.IsTrue(handledTokens[8 ].Type == Token.TokenType.CloseParenthesis);
            Assert.IsTrue(handledTokens[9 ].Type == Token.TokenType.Subtraction);
            Assert.IsTrue(handledTokens[10].Type == Token.TokenType.Number);
        }

        [TestMethod]
        public void TestConsectuveOperatorsValidation()
        {
            const string expression = "5+*x";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);


            try
            {
                privateParser.Invoke("ValidateLegalTokenSequence", tokens);
                Assert.Fail();
            }
            catch (Parser.ConsecutiveOperatorsException e)
            {
                Assert.IsTrue(e.Operator1 == Token.TokenType.Addition);
                Assert.IsTrue(e.Operator2 == Token.TokenType.Multiplication);
            }
        }

        [TestMethod]
        public void TestMissingOperandValidation()
        {
            string expression = "5+x-";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);


            try
            {
                privateParser.Invoke("ValidateLegalTokenSequence", tokens);
                Assert.Fail();
            }
            catch (Parser.MissingOperandException e)
            {
                Assert.IsTrue(e.Operator == Token.TokenType.Subtraction);
            }

            expression = "/5+x";
            tokens = _lexer.Tokenize(expression);

            try
            {
                privateParser.Invoke("ValidateLegalTokenSequence", tokens);
                Assert.Fail();
            }
            catch (Parser.MissingOperandException e)
            {
                Assert.IsTrue(e.Operator == Token.TokenType.Division);
            }
        }

        [TestMethod]
        public void TestShuntingYardMismatchedParenthesis()
        {
            const string expression = "2*(x+3";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);

            try
            {
                privateParser.Invoke("ShuntingYard", tokens);
                Assert.Fail();
            }
            catch (Parser.MismatchedParenthesisException)
            {
                
            }
        }

        [TestMethod]
        public void TestShuntingYard()
        {
            const string expression = "3+4*x/(1-x)^2^5";
            var tokens = _lexer.Tokenize(expression);
            var privateParser = new PrivateObject(_parser);

            try
            {
                var rpn = privateParser.Invoke("ShuntingYard", tokens) as IList<Token>;

                // expected result = 3 4 x * 1 x - 2 5 ^ ^ / +
                Assert.IsTrue(rpn.Count == 13);
                Assert.IsTrue(rpn[0 ].Type == Token.TokenType.Number);
                Assert.IsTrue(rpn[1 ].Type == Token.TokenType.Number);
                Assert.IsTrue(rpn[2 ].Type == Token.TokenType.Variable);
                Assert.IsTrue(rpn[3 ].Type == Token.TokenType.Multiplication);
                Assert.IsTrue(rpn[4 ].Type == Token.TokenType.Number);
                Assert.IsTrue(rpn[5 ].Type == Token.TokenType.Variable);
                Assert.IsTrue(rpn[6 ].Type == Token.TokenType.Subtraction);
                Assert.IsTrue(rpn[7 ].Type == Token.TokenType.Number);
                Assert.IsTrue(rpn[8 ].Type == Token.TokenType.Number);
                Assert.IsTrue(rpn[9 ].Type == Token.TokenType.Power);
                Assert.IsTrue(rpn[10].Type == Token.TokenType.Power);
                Assert.IsTrue(rpn[11].Type == Token.TokenType.Division);
                Assert.IsTrue(rpn[12].Type == Token.TokenType.Addition);
            }
            catch (Parser.MismatchedParenthesisException)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestParser()
        {
            const string expression = "3+4x/(1-x)^2^5";
            var tokens = _lexer.Tokenize(expression);
            var result = _parser.Parse(tokens);

            Assert.IsTrue(result.ParsedWithoutErrors);
            Assert.IsTrue(result.Expression.Count == 13);
            Assert.IsTrue(result.Expression[0].Type == Token.TokenType.Number);
            Assert.IsTrue(result.Expression[1].Type == Token.TokenType.Number);
            Assert.IsTrue(result.Expression[2].Type == Token.TokenType.Variable);
            Assert.IsTrue(result.Expression[3].Type == Token.TokenType.Multiplication);
            Assert.IsTrue(result.Expression[4].Type == Token.TokenType.Number);
            Assert.IsTrue(result.Expression[5].Type == Token.TokenType.Variable);
            Assert.IsTrue(result.Expression[6].Type == Token.TokenType.Subtraction);
            Assert.IsTrue(result.Expression[7].Type == Token.TokenType.Number);
            Assert.IsTrue(result.Expression[8].Type == Token.TokenType.Number);
            Assert.IsTrue(result.Expression[9].Type == Token.TokenType.Power);
            Assert.IsTrue(result.Expression[10].Type == Token.TokenType.Power);
            Assert.IsTrue(result.Expression[11].Type == Token.TokenType.Division);
            Assert.IsTrue(result.Expression[12].Type == Token.TokenType.Addition);
        }
    }

}
