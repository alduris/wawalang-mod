using System;
using System.Collections.Generic;
using System.Text;

namespace Wawalang
{
    public class Interpreter
    {
        public readonly string program;
        public List<byte> stack = [0];
        public List<byte> other = [0];

        public int index = 0;
        public int label = 0;
        public int steps = 0;

        public InterpreterState State { get; private set; }
        public bool strict = false;

        public string Output { get; private set; }
        public string Error { get; private set; }

        public Interpreter(string program)
        {
            this.program = program;
            Reset();
        }

        public void Reset()
        {
            index = 0;
            label = 0;
            stack.Clear();
            other.Clear();
            State = program.Length > 0 ? InterpreterState.Ready : InterpreterState.Finished;
            Output = "";
            Error = "";
        }

        public void Step()
        {
            if (State == InterpreterState.Ready && index < program.Length)
            {
                steps++;
                char next = program[index++];

                switch (next)
                {
                    case ' ':
                        // Push 0 onto stack
                        stack.Add(0);
                        break;
                    case '.' or '!':
                        // Print stack as string. Exclamation point additionally clears the stack.
                        try
                        {
                            var encoder = new UTF8Encoding(true, true);
                            Output += encoder.GetString([.. stack]);

                            if (next == '!')
                            {
                                stack.Clear();
                            }
                        }
                        catch (Exception e)
                        {
                            State = InterpreterState.Error;
                            Error = e.Message;
                        }
                        break;
                    case '?':
                        // Await input
                        State = InterpreterState.AwaitingInput;
                        break;
                    case ',':
                        // Print top of stack as integer
                        Output += ((int)stack[stack.Count - 1]).ToString();
                        break;
                    case '~':
                        // Print top of stack as char
                        try
                        {
                            Output += Encoding.UTF8.GetString([stack[stack.Count - 1]]);
                        }
                        catch (Exception e)
                        {
                            State = InterpreterState.Error;
                            Error = e.Message;
                        }
                        break;
                    case '-':
                        // Swap stacks
                        (stack, other) = (other, stack);
                        break;
                    case '(':
                        // Comment (skip to closing parenthesis)
                        do
                        {
                            index++;
                        } while (index < program.Length && program[index] != ')');
                        index++; // this sets us past the closing parenthesis
                        break;
                    case 'w' or 'a' or 'W' or 'A':
                        // Two letter instructions
                        if (index < program.Length)
                        {
                            char after = program[index++];
                            if (next == 'w')
                            {
                                // Basic operations
                                if (stack.Count == 0)
                                {
                                    if (strict)
                                    {
                                        State = InterpreterState.Error;
                                        Error = "Stack empty!";
                                    }
                                }
                                else
                                {
                                    if (after == 'w') stack[stack.Count - 1]--;
                                    else if (after == 'a') stack[stack.Count - 1]++;
                                    else if (after == 'W') stack[stack.Count - 1] >>= 1;
                                    else if (after == 'A') stack[stack.Count - 1] <<= 1;
                                }
                            }
                            else if (next == 'a')
                            {
                                // Number operations
                                if (stack.Count < 2)
                                {
                                    if (strict)
                                    {
                                        State = InterpreterState.Error;
                                        Error = "Not enough items in stack!";
                                    }
                                }
                                else
                                {
                                    byte b = stack.Pop();
                                    byte a = stack.Pop();
                                    if (after == 'w') stack.Add((byte)(a - b));
                                    else if (after == 'a') stack.Add((byte)(a + b));
                                    else if (after == 'W') stack.Add((byte)(a & b));
                                    else if (after == 'A') stack.Add((byte)(a ^ b));
                                }
                            }
                            else if (next == 'W')
                            {
                                // Stack operations
                                if (after == 'w')
                                {
                                    // Pop top of stack
                                    if (stack.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Stack empty!";
                                        }
                                    }
                                    else
                                    {
                                        stack.Pop();
                                    }
                                }
                                else if (after == 'a')
                                {
                                    // Pop top of stack and push to other
                                    if (stack.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Stack empty!";
                                        }
                                    }
                                    else
                                    {
                                        other.Add(stack.Pop());
                                    }
                                }
                                else if (after == 'W')
                                {
                                    // Swap top two items in stack
                                    if (stack.Count < 2)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Not enough items in stack!";
                                        }
                                    }
                                    else
                                    {
                                        var (a, b) = (stack.Pop(), stack.Pop());
                                        stack.Add(a);
                                        stack.Add(b);
                                    }
                                }
                                else if (after == 'A')
                                {
                                    // Duplicate top of stack
                                    if (stack.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Stack empty!";
                                        }
                                    }
                                    else
                                    {
                                        stack.Add(stack[stack.Count - 1]);
                                    }
                                }
                            }
                            else if (next == 'A')
                            {
                                // Control operations
                                if (after == 'w')
                                {
                                    // Pop stack and go to label if 0
                                    if (stack.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Stack empty!";
                                        }
                                    }
                                    else if (stack.Pop() == 0)
                                    {
                                        index = label;
                                    }
                                }
                                else if (after == 'a')
                                {
                                    // Pop stack and go to label if not 0
                                    if (stack.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Stack empty!";
                                        }
                                    }
                                    else if (stack.Pop() != 0)
                                    {
                                        index = label;
                                    }
                                }
                                else if (after == 'W')
                                {
                                    // Pop stacks and go to label if current stack > other stack
                                    if (stack.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Stack empty!";
                                        }
                                    }
                                    else if (other.Count == 0)
                                    {
                                        if (strict)
                                        {
                                            State = InterpreterState.Error;
                                            Error = "Other stack empty!";
                                        }
                                    }
                                    else if (stack.Pop() > other.Pop())
                                    {
                                        index = label;
                                    }
                                }
                                else if (after == 'A')
                                {
                                    // Define label
                                    label = index;
                                }
                            }
                        }
                        else if (strict)
                        {
                            State = InterpreterState.Error;
                            Error = "Expected symbol but found end of program!";
                        }
                        break;
                }
            }

            if (index >= program.Length)
            {
                State = InterpreterState.Finished;
            }

        }

        public void StepUntilStop(int maxSteps)
        {
            int i = 0;
            while (i < maxSteps && State == InterpreterState.Ready)
            {
                Step();
                i++;
            }
        }

        public void StepUntil() => StepUntilStop(int.MaxValue);

        public void TakeInput(byte value)
        {
            if (State == InterpreterState.AwaitingInput)
            {
                stack.Add(value);
                State = InterpreterState.Ready;
            }
            else
            {
                throw new InvalidOperationException("Cannot take input while last instruction was not a take input instruction!");
            }
        }

        public enum InterpreterState
        {
            Ready,
            AwaitingInput,
            Error,
            Finished
        }
    }
}
