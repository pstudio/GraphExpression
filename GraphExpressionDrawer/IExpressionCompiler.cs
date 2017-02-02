namespace GraphExpressionDrawer
{
    public interface IExpressionCompiler
    {
        byte[] Compile(string expression);
    }
}