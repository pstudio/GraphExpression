using System;

namespace GraphExpressionEvaluator.Lexer
{
    /// <summary>
    /// A token in a simple mathematical polynomial expression language
    /// </summary>
    public class Token
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

        public enum TokenType
        {
            Unknown,
            OpenParenthesis,
            CloseParenthesis,
            Addition,
            Subtraction,
            Multiplication,
            Division,
            Power,
            Variable,
            Number,
        }

        public static TokenType GetOperandTypeFromChar(char c)
        {
            switch (c)
            {
                case '(':
                    return TokenType.OpenParenthesis;
                case ')':
                    return TokenType.CloseParenthesis;
                case '+':
                    return TokenType.Addition;
                case '-':
                    return TokenType.Subtraction;
                case '*':
                    return TokenType.Multiplication;
                case '/':
                    return TokenType.Division;
                case '^':
                    return TokenType.Power;
                case 'x':
                case 'X': return TokenType.Variable;
                default: return TokenType.Unknown;
            }
        }

        /// <summary>
        /// The type of the token. E.g. addition or number
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// The string literal for the token if applicable
        /// </summary>
        public string Literal { get; }

        /// <summary>
        /// If the token is a number this is the real value
        /// </summary>
        public float RealValue { get; }

        public Token(TokenType type, string literal = null, float value = 0f)
        {
            Type = type;
            Literal = literal;
            RealValue = value;
        }

        public Token(Token token)
        {
            Type = token.Type;
            Literal = token.Literal;
            RealValue = token.RealValue;
        }
    }

    public static class TokenExtensions
    {
        public static int Precedence(this Token.TokenType token)
        {
            switch (token)
            {
                case Token.TokenType.Addition:
                case Token.TokenType.Subtraction:
                    return 2;
                case Token.TokenType.Multiplication:
                case Token.TokenType.Division:
                    return 3;
                case Token.TokenType.Power:
                    return 4;
                default:
                    return 0;
            }
        }

        public static string OperatorToString(this Token.TokenType token)
        {
            switch (token)
            {
                case Token.TokenType.Addition:
                    return "+";
                case Token.TokenType.Subtraction:
                    return "-";
                case Token.TokenType.Multiplication:
                    return "*";
                case Token.TokenType.Division:
                    return "/";
                case Token.TokenType.Power:
                    return "^";
                default:
                    return string.Empty;
            }
        }
    }
}