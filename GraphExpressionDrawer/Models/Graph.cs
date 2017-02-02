using System;
using System.Windows.Media;
using GraphExpressionEvaluator.Parser;

namespace GraphExpressionDrawer.Models
{
    /// <summary>
    /// A graph represented my a mathematical expression
    /// </summary>
    public class Graph
    {
        private readonly IExpressionCompiler _compiler;
        private string _expression;

        /// <summary>
        /// The expression. e.g. "2x-3"
        /// </summary>
        public string Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                TryCompile(value);
            }
        }

        /// <summary>
        /// If the expression is valid this is the compiled expression
        /// </summary>
        public byte[] ByteCode { get; private set; }

        /// <summary>
        /// Is the expression valid and compileable?
        /// </summary>
        public bool Valid => ByteCode != null;

        /// <summary>
        /// If the expression is not valid this gives a hint why the expression is not valid
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Creates a new empty graph
        /// </summary>
        /// <param name="compiler">A compiler for compiling math expressions</param>
        /// <exception cref="ArgumentNullException">Thrown if compiler is null</exception>
        public Graph(IExpressionCompiler compiler)
        {
            if (compiler == null)
            {
                throw new ArgumentNullException(nameof(compiler));
            }

            _compiler = compiler;
        }

        private void TryCompile(string expression)
        {
            try
            {
                ByteCode = _compiler.Compile(expression);
                Error = string.Empty;
            }
            catch (Parser.ParsingException exception)
            {
                Error = exception.Message;
                ByteCode = null;
            }
        }
    }

}
