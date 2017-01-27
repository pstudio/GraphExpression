using System;
using System.Collections.Generic;
using System.Linq;
using GraphExpressionEvaluator.Lexer;

namespace GraphExpressionEvaluator.Compiler
{
    /// <summary>
    /// Compiles a math expression in Reverse Polish Notation into 'bytecode'
    /// </summary>
    public class Compiler
    {
        // Currently not used - may be used if we introduce code optimization
        #region Compiler Exceptions 

        public class CompilerException : Exception
        {
        }

        public class TooFewArgsException : CompilerException
        {
        }

        public class TooManyArgsException : CompilerException
        {
        }

        public class DivisionByZeroException : CompilerException
        {
        }

        #endregion

        /// <summary>
        /// Compiles the token list in Reverse Polish Notation into byte code
        /// </summary>
        /// <param name="rpn">Token list in Reverse Polish Notation</param>
        /// <returns>The compiled expression in custom bytecode</returns>
        public byte[] Compile(IList<Token> rpn)
        {
            // We estimate the size of our byte code array to be #tokens * 4.
            // All tokens require 1 byte except numbers which require 5 bytes
            var bytes = new List<byte>(rpn.Count*4);

            // We use the Postfix algorithm for Reverse Polish Notation when evaluating the expression.
            // https://en.wikipedia.org/wiki/Reverse_Polish_notation
            // Since the input tokens are already in RPN we simply translate the tokens to opcodes for our bytecode language

            // While there are input tokens left
            // Read the next token from input.
            foreach (var token in rpn)
            {
                switch (token.Type)
                {
                    case Token.TokenType.Addition:
                    case Token.TokenType.Subtraction:
                    case Token.TokenType.Multiplication:
                    case Token.TokenType.Division:
                    case Token.TokenType.Power:
                    case Token.TokenType.Variable:
                        bytes.Add((byte)token.Type.ToByteCodeOp());
                        break;
                    case Token.TokenType.Number:
                        bytes.Add((byte)token.Type.ToByteCodeOp());
                        var numArray = BitConverter.GetBytes(token.RealValue);
                        if (!BitConverter.IsLittleEndian) // We ensure that numbers in our compiled expression are always using little endianess
                        {
                            Array.Reverse(numArray);
                        }
                        bytes.AddRange(numArray);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return bytes.ToArray();
        }
    }

    internal static class Extensions
    {
        internal static Interpreter.Interpreter.ByteCodeOp ToByteCodeOp(this Token.TokenType tokenType)
        {
            switch (tokenType)
            {
                case Token.TokenType.Addition:
                    return Interpreter.Interpreter.ByteCodeOp.Add;
                case Token.TokenType.Subtraction:
                    return Interpreter.Interpreter.ByteCodeOp.Sub;
                case Token.TokenType.Multiplication:
                    return Interpreter.Interpreter.ByteCodeOp.Mul;
                case Token.TokenType.Division:
                    return Interpreter.Interpreter.ByteCodeOp.Div;
                case Token.TokenType.Power:
                    return Interpreter.Interpreter.ByteCodeOp.Pow;
                case Token.TokenType.Variable:
                    return Interpreter.Interpreter.ByteCodeOp.PushX;
                case Token.TokenType.Number:
                    return Interpreter.Interpreter.ByteCodeOp.Push;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
            }
        }
    }
}