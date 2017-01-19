using System;
using System.Collections.Generic;

namespace GraphExpressionEvaluator.Interpreter
{
    /// <summary>
    /// A class for running compiled math expressions
    /// </summary>
    public class Interpreter
    {
        #region Byte Code Op

/**
         * The interpreter has a stack for storing values.
         * Byte code operations work on that stack to represent the execution of an expression
         */

        public enum ByteCodeOp : byte
        {
            Push, // Push a real value to the stack
            //Pop,    // Pop a real value from the stack
            PushX, // Push X to the stack - Value of X is known at execution time, so a real value is added to the stack
            //PopX,   // Pop X from the stack
            Add, // Pop two top values, add them and push result on stack
            Sub, // Pop two top values, subtract them and push result on stack
            Mul, // Pop two top values, multiply them and push result on stack
            Div, // Pop two top values, divide them and push result on stack
            Pow // Pop two top values, power them and push result on stack
        }

        #endregion


        #region Exceptions

        public class InterpreterException : Exception
        {
        }

        public class InterpreterDivideByZeroException : InterpreterException
        {
        }

        public class InterpreterUnknownOpCodeException : InterpreterException
        {
        }

        public class InterpreterTooFewArgsException : InterpreterException
        {
        }

        public class InterpreterTooManyArgsException : InterpreterException
        {
        }

        #endregion


        private readonly Stack<float> _stack; 
        private readonly byte[] _numArray;

        public Interpreter()
        {
            _stack = new Stack<float>(); // Since Evaluate() could potentially be called 1000's of times per second, we move the allocation of the stack to the constructor, so that we don't create insane amounts of garbage
            _numArray = new byte[4];
        } 

        public float Evaluate(byte[] code, float x)
        {
            // We use the Postfix algorithm for Reverse Polish Notation to 'compile' the input
            // https://en.wikipedia.org/wiki/Reverse_Polish_notation

            _stack.Clear(); 

            for (var i = 0; i < code.Length; i++)
            {
                var opcode = (ByteCodeOp) code[i];


                float op1, op2;
                
                switch (opcode)
                {
                    case ByteCodeOp.Push:
                        Array.Copy(code, i + 1, _numArray, 0, 4);
                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(_numArray);
                        }

                        op1 = BitConverter.ToSingle(_numArray, 0);
                        _stack.Push(op1);
                        i += 4;
                        break;
                    case ByteCodeOp.PushX:
                        _stack.Push(x);
                        break;
                    case ByteCodeOp.Add:
                        GetOperands(out op1, out op2);
                        _stack.Push(op1 + op2);
                        break;
                    case ByteCodeOp.Sub:
                        GetOperands(out op1, out op2);
                        _stack.Push(op1 - op2);
                        break;
                    case ByteCodeOp.Mul:
                        GetOperands(out op1, out op2);
                        _stack.Push(op1 * op2);
                        break;
                    case ByteCodeOp.Div:
                        GetOperands(out op1, out op2);
                        if (op2 == 0f) // possible div by 0
                        {
                            throw new InterpreterDivideByZeroException();
                        }
                        _stack.Push(op1 / op2);
                        break;
                    case ByteCodeOp.Pow:
                        GetOperands(out op1, out op2);
                        _stack.Push((float)Math.Pow(op1, op2));
                        break;
                    default:
                        throw new InterpreterUnknownOpCodeException();
                }
            }

            if (_stack.Count > 1)
            {
                throw new InterpreterTooManyArgsException();
            }

            return _stack.Pop();
        }

        private void GetOperands(out float op1, out float op2)
        {
            if (_stack.Count < 2)
            {
                throw new InterpreterTooFewArgsException();
            }

            op2 = _stack.Pop();
            op1 = _stack.Pop();
        }
    }
}
