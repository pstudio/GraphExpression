# GraphExpression
A small lib for compiling and evaluating simple math expressions with a variable 'x'

A small project made for fun. I felt like writing a compiler having watched some of [Jontahan Blow's](https://www.youtube.com/channel/UCCuoqzrsHlwv1YyPKLuMDUQ) talks about his own programming language.
However writing a full language compiler is a big task, and all I really wanted to do was handwriting a simple parser for fun.
So I decided to write a 'compiler' for simple mathematical expressions. 

The expressions can contain the following symbols:

- Real numbers
- The variable X
- Parenthesis ( )
- + - * / and ^ (power)

Expressions like "2x^2+3x-4" are compiled into a simple bytecode which can then be evaluated with a specific value for the variable X.
