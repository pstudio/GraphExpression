using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using GraphExpressionEvaluator.Lexer;

namespace GraphExpressionEvaluator.Parser
{
    /// <summary>
    /// A class for parsing tokens in a math expression.
    /// </summary>
    public class Parser
    {
        /*
        * Grammar:
        * 
        * '(' - Open parenthesis
        * ')' - Close parenthesis
        * '+' - Addition
        * '-' - Subtraction
        * '*' - Multiplication
        * '/' - Division
        * '^' - Power
        * 'x' || 'X' - Variable X
        * '3' || '3.14' - Real number
        * ' ' - Whitespaces (Ignored)
        * 
        */

        #region Custom Exceptions

        public class ParsingException : Exception
        {
            public ParsingException(string message) : base(message) { }
        }

        public class UnknownTokensException : ParsingException
        {
            public UnknownTokensException(string message) : base(message) { }
        }

        public class MismatchedParenthesisException : ParsingException
        {
            public MismatchedParenthesisException(string message) : base(message) { }
        }

        public class ConsecutiveOperatorsException : ParsingException
        {
            public Token.TokenType Operator1 { get; set; }
            public Token.TokenType Operator2 { get; set; }

            public ConsecutiveOperatorsException(string message) : base(message) { }

            public ConsecutiveOperatorsException(string message, Token.TokenType op1, Token.TokenType op2) : base(message)
            {
                Operator1 = op1;
                Operator2 = op2;
            }
        }

        public class MissingOperandException : ParsingException
        {
            public Token.TokenType Operator { get; set; }

            public MissingOperandException(string message) : base(message) { }

            public MissingOperandException(string message, Token.TokenType op) : base(message)
            {
                Operator = op;
            }
        }

        #endregion

        /// <summary>
        /// The result of a parse attempt.
        /// Will contain the parsed tree and any errors encountered.
        /// </summary>
        public class ParseResult
        {
            public IList<Token> Expression { get; internal set; }
            public string Error { get; internal set; }

            public bool ParsedWithoutErrors => string.IsNullOrEmpty(Error);
        }

        /// <summary>
        /// Will attempt to parse a list of tokens into a compileable form. More specific into Reverse Polish Notation.
        /// </summary>
        /// <param name="tokens">The list of tokens to be parsed</param>
        /// <returns>A ParseResult which will contain the RPN if parsing was successful or otherwise the error encountered</returns>
        /// <exception cref="UnknownTokensException">Thrown if unknown tokens are encountered in the token list</exception>
        /// <exception cref="ConsecutiveOperatorsException">Thrown if two operators are right next to each other</exception>
        /// <exception cref="MissingOperandException">Thrown if an operator is missing an argument</exception>
        /// <exception cref="MismatchedParenthesisException">Thrown if there is missing one or more parenthesises</exception>
        public IList<Token> Parse(IList<Token> tokens)
        {
            //var result = new ParseResult();

            // Preprocessing steps

            // We will not bother parsing the list if it contains unknown tokens
            if (ContainsUnknownTokens(tokens))
            {
                throw new UnknownTokensException("Expression contains token(s) of Unknown type. Expression cannot be parsed");
                //result.Error = "Expression contains token(s) of Unknown type. Expression cannot be parsed";
                //return result;
            }

            // Expand the token list for any implicit (multiplication) tokens
            var expandedTokens = ExpandImplicitTokens(tokens);

            // Parsing

            // Check for unary plusses/minusses and handle them
            var handledTokens = HandleUnaryPlusMinus(expandedTokens);

            // Validate that there are no illegal sequence of tokens
            //try
            //{
            //    ValidateLegalTokenSequence(handledTokens);
            //    //if (!ValidateLegalTokenSequence(handledTokens))
            //    //{
            //    //    // never happens currently
            //    //    result.Error = "Expression contains illegal sequence of tokens";
            //    //    return result;
            //    //}
            //}
            //catch (ConsecutiveOperatorsException e)
            //{
            //    result.Error = $"Expression contains consecutive operators: '{e.Operator1.OperatorToString()},{e.Operator2.OperatorToString()}'";
            //    return result;
            //}
            //catch (MissingOperandException e)
            //{
            //    result.Error = $"Operator '{e.Operator.OperatorToString()}' is missing an operand";
            //    return result;
            //}

            //// Parse the expression from infix form to RPN
            //try
            //{
            //    result.Expression = ShuntingYard(handledTokens);
            //}
            //catch (MismatchedParenthesisException)
            //{
            //    result.Error = "There is a mismatch of parenthesis";
            //    return result;
            //}

            ValidateLegalTokenSequence(handledTokens);
            return ShuntingYard(handledTokens);


            //return result;
        }

        #region Preprocessing

        private bool ContainsUnknownTokens(IList<Token> tokens) => tokens.Any(t => t.Type == Token.TokenType.Unknown);

        private static readonly Token.TokenType[] ExpandCandidates =
        {
            Token.TokenType.Number,
            Token.TokenType.Variable,
            Token.TokenType.OpenParenthesis,
            Token.TokenType.CloseParenthesis
        };

        private IList<Token> ExpandImplicitTokens(IList<Token> tokens)
        {
            var expandedTokens = new List<Token>();

            var possiblyImplicit = false;

            foreach (var token in tokens)
            {
                if (possiblyImplicit && token.Type != Token.TokenType.CloseParenthesis &&
                    ExpandCandidates.Contains(token.Type))
                {
                    expandedTokens.Add(new Token(Token.TokenType.Multiplication));
                }

                possiblyImplicit = token.Type != Token.TokenType.OpenParenthesis &&
                                   ExpandCandidates.Contains(token.Type);

                expandedTokens.Add(new Token(token));
            }

            return expandedTokens;
        }

        #endregion

        #region Parsing

        private static readonly Token.TokenType[] Operators =
        {
            Token.TokenType.Addition,
            Token.TokenType.Subtraction,
            Token.TokenType.Multiplication,
            Token.TokenType.Division,
            Token.TokenType.Power,

        };

        // Look for and handle any unary plus/minuses
        // If minus is for number, then remove minus token and negate real value of number token
        // If minus is for variable, then expand from -x to (-1)*x
        // If unary plus, then remove plus token
        private IList<Token> HandleUnaryPlusMinus(IList<Token> tokens)
        {
            // -2*x*(-x+2)-4
            var output = new List<Token>();

            var possiblyUnary = true;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if (possiblyUnary && token.Type == Token.TokenType.Subtraction)
                {
                    // If minus is for number, then remove minus token and negate real value of number token
                    if (i+1 < tokens.Count && tokens[i+1].Type == Token.TokenType.Number)
                    {
                        ++i;
                        var number = tokens[i];
                        output.Add(new Token(Token.TokenType.Number, $"-{number.Literal}", number.RealValue * -1));
                        possiblyUnary = false;
                        continue;
                    }

                    // If minus is for variable, then expand from -x to (-1)*x
                    if (i+1 < tokens.Count && tokens[i+1].Type == Token.TokenType.Variable)
                    {
                        output.Add(new Token(Token.TokenType.Number, "-1", -1));
                        output.Add(new Token(Token.TokenType.Multiplication));
                        ++i;
                        output.Add(tokens[i]);
                        possiblyUnary = false;
                        continue;
                    }
                }

                if (possiblyUnary && token.Type == Token.TokenType.Addition)
                {
                    // If unary plus, then remove plus token
                    if (i + 1 < tokens.Count && (tokens[i + 1].Type == Token.TokenType.Number || tokens[i + 1].Type == Token.TokenType.Variable))
                    {
                        ++i;
                        output.Add(tokens[i]);
                        possiblyUnary = false;
                        continue;
                    }
                }

                output.Add(token);
                possiblyUnary = token.Type == Token.TokenType.OpenParenthesis || Operators.Contains(token.Type);
            }

            return output;
        }

        // If a sequence contains consecutive operators it is invalid
        // If a sequence contains an operator with a missing operand it is invalid
        private void ValidateLegalTokenSequence(IList<Token> tokens)
        {
            // 2-3*x*(5+x)^2/2
            for (int i = 0; i < tokens.Count; i++)
            {
                if (!Operators.Contains(tokens[i].Type))
                {
                    continue;
                }

                // If a sequence contains consecutive operators it is invalid
                if (i > 0 && Operators.Contains(tokens[i-1].Type))
                {
                    var op1 = tokens[i - 1].Type;
                    var op2 = tokens[i].Type;
                    throw new ConsecutiveOperatorsException(
                        $"Expression contains consecutive operators: '{op1.OperatorToString()},{op2.OperatorToString()}'", op1, op2);
                    //{
                    //    Operator1 = tokens[i-1].Type,
                    //    Operator2 = tokens[i].Type
                    //};
                }

                // If a sequnece contains an operator with a missing operand it is invalid
                // We've already made sure that there are no consecutive operators, so they only cases where there can be a missing operand
                // is if the operator is at the beginning or end of the expression
                if (i == 0 || i == tokens.Count - 1)
                {
                    var op = tokens[i].Type;
                    throw new MissingOperandException($"Operator '{op.OperatorToString()}' is missing an operand", op);
                    //{
                    //    Operator = tokens[i].Type
                    //};
                }
            }
        }

        // Shunting Yard Algorithm - https://en.wikipedia.org/wiki/Shunting-yard_algorithm
        // Parse the tokens from Infix form to Reverse Polish Notation
        private IList<Token> ShuntingYard(IList<Token> tokens)
        {
            var output = new List<Token>();
            var stack = new List<Token>();

            foreach (var token in tokens)
            {
                // If the token is a number/variable, then push it to the output queue.
                if (token.Type == Token.TokenType.Number || token.Type == Token.TokenType.Variable)
                {
                    output.Add(token);
                    continue;
                }

                // If the token is an operator, o1, then:
                if (Operators.Contains(token.Type))
                {
                    // while there is an operator token o2, at the top of the operator stack and either
                    while (stack.Count > 0 && Operators.Contains(stack[stack.Count-1].Type))
                    {
                        Token op = stack[stack.Count - 1];
                        // o1 is left-associative and its precedence is less than or equal to that of o2, or
                        // o1 is right associative, and has precedence less than that of o2,
                        if ((token.Type != Token.TokenType.Power && token.Type.Precedence() <= op.Type.Precedence()) ||
                            (token.Type == Token.TokenType.Power && token.Type.Precedence() < op.Type.Precedence()))
                        {
                            // pop o2 off the operator stack, onto the output queue;
                            output.Add(op);
                            stack.Remove(op);
                        }
                        else
                        {
                            break;
                        }
                    }

                    // at the end of iteration push o1 onto the operator stack.
                    stack.Add(token);
                    continue;
                }

                // If the token is a left parenthesis (i.e. "("), then push it onto the stack.
                if (token.Type == Token.TokenType.OpenParenthesis)
                {
                    stack.Add(token);
                    continue;
                }

                // If the token is a right parenthesis (i.e. ")"):
                if (token.Type == Token.TokenType.CloseParenthesis)
                {
                    // Until the token at the top of the stack is a left parenthesis, pop operators off the stack onto the output queue.
                    while (stack.Count > 0 && stack[stack.Count-1].Type != Token.TokenType.OpenParenthesis)
                    {
                        output.Add(stack[stack.Count-1]);
                        stack.RemoveAt(stack.Count-1);

                        // If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
                        if (stack.Count == 0)
                        {
                            throw new MismatchedParenthesisException("There is a mismatch of parenthesis");
                        }
                    }

                    // Pop the left parenthesis from the stack, but not onto the output queue.
                    stack.RemoveAt(stack.Count-1);
                }
            }

            // When there are no more tokens to read:
            // While there are still operator tokens in the stack:
            while (stack.Count > 0)
            {
                var op = stack[stack.Count - 1];

                // If the operator token on the top of the stack is a parenthesis, then there are mismatched parentheses
                if (op.Type == Token.TokenType.OpenParenthesis)
                {
                    throw new MismatchedParenthesisException("There is a mismatch of parenthesis");
                }

                // Pop the operator onto the output queue.
                output.Add(stack[stack.Count - 1]);
                stack.RemoveAt(stack.Count - 1);
            }

            // Exit.
            return output;
        }

        #endregion

    }

}
