using GraphExpressionEvaluator.Compiler;
using GraphExpressionEvaluator.Lexer;
using GraphExpressionEvaluator.Parser;

namespace GraphExpressionDrawer
{
    public class ExpressionCompiler : IExpressionCompiler
    {
        private readonly Lexer _lexer;
        private readonly Parser _parser;
        private readonly Compiler _compiler;

        public ExpressionCompiler()
        {
            _lexer = new Lexer();
            _parser = new Parser();
            _compiler = new Compiler();
        }

        public byte[] Compile(string expression)
        {
            var tokens = _lexer.Tokenize(expression);
            var result = _parser.Parse(tokens);
            var bytes = _compiler.Compile(result);

            return bytes;
            
        }
    }

}
