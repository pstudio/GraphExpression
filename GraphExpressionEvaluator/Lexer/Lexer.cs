using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GraphExpressionEvaluator.Lexer
{
    /// <summary>
    /// A class that can tokenize simple mathematical polynomial expressions.
    /// </summary>
    public class Lexer
    {
        private const string OperatorChars = "()+-*/^xX";
        private const string NumberChars = "1234567890.,";

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

        // Sort of a state machine that dependent on the input character returns a lexeme plus the token type
        private static readonly Dictionary<Func<char, bool>, Func<string, Tuple<string, Token.TokenType>>> Grammar = new Dictionary<Func<char, bool>, Func<string, Tuple<string, Token.TokenType>>>
        {
            { c => OperatorChars.Contains(c.ToString()), e => new Tuple<string, Token.TokenType> (e.Substring(0, 1), Token.GetOperandTypeFromChar(e[0])) },
            { c => NumberChars.Contains(c.ToString()), e => new Tuple<string, Token.TokenType>(ScanNumber(e), Token.TokenType.Number) },
            { c => true, e => new Tuple<string, Token.TokenType>(ScanGarbage(e), Token.TokenType.Unknown) }
        };

        public IList<Token> Tokenize(string expression)
        {
            var lexemes = Scan(expression);
            var tokens = Evaluate(lexemes);

            return tokens;
        }

        #region Scan

        /// <summary>
        /// Splits an expression into lexemes (e.g. "+", "x", "3.14"). Ignores spaces but is otherwise 'dumb'. 
        /// It will not do any sort of validation of the corectness of the lexemes.
        /// </summary>
        /// <param name="expression">The mathematical expression to scan</param>
        /// <returns>A list of lexemes. Each lexeme is a tuple with the string literal and a Token.Type to hint the type of the literal</returns>
        public IList<Tuple<string, Token.TokenType>> Scan(string expression)
        {
            var lexemes = new List<Tuple<string, Token.TokenType>>();

            expression = new string(expression?.Where(c => !char.IsWhiteSpace(c)).ToArray()); // remove whitespaces

            while (!string.IsNullOrEmpty(expression))
            {
                var lexeme = ScanLexeme(expression);
                expression = expression.Substring(lexeme?.Item1.Length ?? 0);

                if (lexeme == null)
                {
                    continue;
                }

                lexemes.Add(lexeme);
            }

            return lexemes;
        }

        private static Tuple<string, Token.TokenType> ScanLexeme(string expression)
        {
            return Grammar.First(grammar => grammar.Key(expression[0])).Value(expression);
        }

        private static string ScanNumber(string expression)
        {
            var pos = 1;
            while (pos < expression.Length && NumberChars.Contains(expression[pos].ToString()))
            {
                ++pos;
            }

            return expression.Substring(0, pos);
        }

        private static string ScanGarbage(string expression)
        {
            var pos = 1;
            while (pos < expression.Length && !NumberChars.Contains(expression[pos].ToString()) &&
                   !OperatorChars.Contains(expression[pos].ToString()) && expression[pos] != ' ')
            {
                ++pos;
            }

            return expression.Substring(0, pos);
        }

        #endregion

        #region Evaluate

        /// <summary>
        /// Evaluates a list of lexems and turns them into a list of valid tokens. 
        /// Note that a token itself may contain an invalid value. i.e. the token is of type unknown
        /// </summary>
        /// <param name="lexems">A list of lexemes</param>
        /// <returns>A list of tokens</returns>
        public IList<Token> Evaluate(IList<Tuple<string, Token.TokenType>> lexems)
        {
            return lexems.Select(EvaluateToken).ToList();
        }

        private static Token EvaluateToken(Tuple<string, Token.TokenType> lexem)
        {
            switch (lexem.Item2)
            {
                case Token.TokenType.Unknown:
                    return new Token(Token.TokenType.Unknown, lexem.Item1);
                case Token.TokenType.OpenParenthesis:
                case Token.TokenType.CloseParenthesis:
                case Token.TokenType.Addition:
                case Token.TokenType.Subtraction:
                case Token.TokenType.Multiplication:
                case Token.TokenType.Division:
                case Token.TokenType.Power:
                case Token.TokenType.Variable:
                    return new Token(lexem.Item2);
                case Token.TokenType.Number:
                    return EvaluateNumber(lexem.Item1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Token EvaluateNumber(string lexem)
        {
            float val;

            if (float.TryParse(lexem, NumberStyles.Float, CultureInfo.CurrentCulture, out val))
            {
                return new Token(Token.TokenType.Number, lexem, val);
            }

            return new Token(Token.TokenType.Unknown, lexem);
        }

        #endregion
    }
}
