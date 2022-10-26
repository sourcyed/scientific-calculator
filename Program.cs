using System.Globalization;
using System.Text;
using Xunit;

namespace Calculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
        }

        static string calculate(string input) // calculate method takes a mathematical expression string as input and returns the result as an output in string format
        {
            Tokenizer tokenizer = new Tokenizer();
            ShuntingYardAlgorithm shuntingYard = new ShuntingYardAlgorithm();
            PostfixNotationCalculator calculator = new PostfixNotationCalculator(3);
            var infixNotationTokens = tokenizer.Parse(input);
            var postfixNotationTokens = shuntingYard.Apply(infixNotationTokens);
            return calculator.Calculate(postfixNotationTokens);
        }
    }

    public class Tokenizer // Tokenizer to parse input string
    {
        readonly StringBuilder valueTokenBuilder;
        readonly List<IToken> infixNotationTokens;

        public Tokenizer()
        {
            valueTokenBuilder = new StringBuilder();
            infixNotationTokens = new List<IToken>();
        }


        public IEnumerable<IToken> Parse(string expression)
        {
            Reset();
            foreach (char next in expression)
            {
                FeedCharacter(next);
            }
            return GetResult();
        }

        void Reset()
        {
            valueTokenBuilder.Clear();
            infixNotationTokens.Clear();
        }

        void FeedCharacter(char next)
        {
            if (IsSpacingCharacter(next))
            {
                if (valueTokenBuilder.Length > 0)
                {
                    var token = CreateToken(valueTokenBuilder.ToString());
                    valueTokenBuilder.Clear();
                    infixNotationTokens.Add(token);
                }
            }
            else if (IsBracketCharacter(next))
            {
                if (valueTokenBuilder.Length > 0)
                {
                    var token = CreateToken(valueTokenBuilder.ToString());
                    valueTokenBuilder.Clear();
                    infixNotationTokens.Add(token);
                }
                var operatorToken = CreateBracketToken(next);
                infixNotationTokens.Add(operatorToken);
            }
            else if (next == '!')
            {
                if (valueTokenBuilder.Length > 0)
                {
                    var token = CreateToken(valueTokenBuilder.ToString());
                    valueTokenBuilder.Clear();
                    infixNotationTokens.Add(token);
                }
                var operatorToken = CreateToken(next.ToString());
                infixNotationTokens.Add(operatorToken);
            }
            else
            {
                valueTokenBuilder.Append(next);
            }
        }

        bool IsSpacingCharacter(char c)
        {
            switch (c)
            {
                case ' ':
                    return true;
                default:
                    return false;
            }
        }

        bool IsBracketCharacter(char c)
        {
            switch (c)
            {
                case '(':
                case ')':
                    return true;
                default:
                    return false;
            }
        }

        IToken CreateToken(string raw)
        {
            switch (raw)
            {
                case "(": return new OperatorToken(OperatorType.OpeningBracket);
                case ")": return new OperatorToken(OperatorType.ClosingBracket);
                case "+": return new OperatorToken(OperatorType.Addition);
                case "-": return new OperatorToken(OperatorType.Subtraction);
                case "*": return new OperatorToken(OperatorType.Multiplication);
                case "/": return new OperatorToken(OperatorType.Division);
                case "^": return new OperatorToken(OperatorType.Exponentiation);
                case "sin": return new OperatorToken(OperatorType.Sin);
                case "cos": return new OperatorToken(OperatorType.Cos);
                case "tan": return new OperatorToken(OperatorType.Tan);
                case "log": return new OperatorToken(OperatorType.Log);
                case "!": return new OperatorToken(OperatorType.Factorial);
                default:
                    if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result1))
                    {
                        return new OperandToken(result1);
                    }
                    else if (uint.TryParse(raw, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result2))
                    {
                        return new OperandToken(result2);
                    }
                    throw new SyntaxException($"The expression {raw} has an invalid format.");
            }
        }

        OperatorToken CreateBracketToken(char c)
        {
            switch (c)
            {
                case '(': return new OperatorToken(OperatorType.OpeningBracket);
                case ')': return new OperatorToken(OperatorType.ClosingBracket);
                default:
                    throw new SyntaxException($"There's no suitable bracket for the char {c}");
            }
        }

        IEnumerable<IToken> GetResult()
        {
            if (valueTokenBuilder.Length > 0)
            {
                var token = CreateToken(valueTokenBuilder.ToString());
                valueTokenBuilder.Clear();
                infixNotationTokens.Add(token);
            }
            return infixNotationTokens.ToList();
        }
    }

    public class ShuntingYardAlgorithm // Using the shunting yard algorithm to convert expressions in infix notation to postfix notation
    {
        readonly Stack<OperatorToken> operatorsStack;
        readonly List<IToken> postfixNotationTokens;
        public ShuntingYardAlgorithm()
        {
            operatorsStack = new Stack<OperatorToken>();
            postfixNotationTokens = new List<IToken>();
        }

        public IEnumerable<IToken> Apply(IEnumerable<IToken> infixNotationTokens)
        {
            Reset();
            foreach (var token in infixNotationTokens)
            {
                ProcessToken(token);
            }
            return GetResult();
        }

        void Reset()
        {
            operatorsStack.Clear();
            postfixNotationTokens.Clear();
        }

        private void ProcessToken(IToken token)
        {
            switch (token)
            {
                case OperandToken operandToken:
                    StoreOperand(operandToken);
                    break;
                case OperatorToken operatorToken:
                    ProcessOperator(operatorToken);
                    break;
                default:
                    var exMessage = $"An unknown token type: {token.GetType().ToString()}.";
                    throw new SyntaxException(exMessage);
            }
        }

        void StoreOperand(OperandToken operandToken)
        {
            postfixNotationTokens.Add(operandToken);
        }

        void ProcessOperator(OperatorToken operatorToken)
        {
            switch (operatorToken.OperatorType)
            {
                case OperatorType.OpeningBracket:
                    PushOpeningBracket(operatorToken);
                    break;
                case OperatorType.ClosingBracket:
                    PushClosingBracket(operatorToken);
                    break;
                default:
                    PushOperator(operatorToken);
                    break;
            }
        }
        void PushOpeningBracket(OperatorToken operatorToken)
        {
            operatorsStack.Push(operatorToken);
        }

        void PushClosingBracket(OperatorToken operatorToken)
        {
            bool openingBracketFound = false;

            while (operatorsStack.Count > 0)
            {
                var stackOperatorToken = operatorsStack.Pop();
                if (stackOperatorToken.OperatorType == OperatorType.OpeningBracket)
                {
                    openingBracketFound = true;
                    break;
                }

                postfixNotationTokens.Add(stackOperatorToken);
            }

            if (!openingBracketFound)
            {
                throw new SyntaxException("An unexpected closing bracket.");
            }
        }

        void PushOperator(OperatorToken operatorToken)
        {
            var operatorPriority = GetOperatorPriority(operatorToken);
            while (operatorsStack.Count > 0)
            {
                var stackOperatorToken = operatorsStack.Peek();
                if (stackOperatorToken.OperatorType == OperatorType.OpeningBracket)
                {
                    break;
                }

                var stackOperatorPriority = GetOperatorPriority(stackOperatorToken);
                if (stackOperatorPriority < operatorPriority)
                {
                    break;
                }
                postfixNotationTokens.Add(operatorsStack.Pop());
            }
            operatorsStack.Push(operatorToken);
        }

        int GetOperatorPriority(OperatorToken operatorToken)
        {
            switch (operatorToken.OperatorType)
            {
                case OperatorType.Addition:
                case OperatorType.Subtraction:
                    return 1;
                case OperatorType.Multiplication:
                case OperatorType.Division:
                    return 2;
                default:
                    return 3;
            }
        }

        List<IToken> GetResult()
        {
            while (operatorsStack.Count > 0)
            {
                var token = operatorsStack.Pop();
                if (token.OperatorType == OperatorType.OpeningBracket)
                {
                    throw new SyntaxException("A redundant opening bracket.");
                }
                postfixNotationTokens.Add(token);
            }
            return postfixNotationTokens.ToList();
        }
    }

    public class PostfixNotationCalculator // Calculating the final value from the postfix notation
    {
        private Stack<OperandToken> operandTokensStack;
        private int decimalPrecision = -1;
        

        public int DecimalPrecision { get { return decimalPrecision; } set { decimalPrecision = value; } }

        public PostfixNotationCalculator()
        {
            operandTokensStack = new Stack<OperandToken>();
        }

        public PostfixNotationCalculator(int decimalPrecision)
        {
            this.decimalPrecision = decimalPrecision;
            operandTokensStack = new Stack<OperandToken>();
        }

        public string Calculate(IEnumerable<IToken> tokens)
        {
            foreach (var token in tokens)
            {
                ProcessToken(token);
            }
            string formatting = (decimalPrecision == -1) ? "G29" : "n" + decimalPrecision;
            return GetResult().Value.ToString(formatting, CultureInfo.InvariantCulture);
        }

        void ProcessToken(IToken token)
        {
            switch (token)
            {
                case OperandToken operandToken:
                    StoreOperand(operandToken);
                    break;
                case OperatorToken operatorToken:
                    ApplyOperator(operatorToken);
                    break;
                default:
                    var exMessage = $"An unknown token type: {token.GetType()}";
                    throw new SyntaxException(exMessage);
            }
        }

        void StoreOperand(OperandToken operandToken)
        {
            operandTokensStack.Push(operandToken);
        }

        void ApplyOperator(OperatorToken operatorToken)
        {
            switch (operatorToken.OperatorType)
            {
                case OperatorType.Addition:
                    ApplyAdditionOperator();
                    break;
                case OperatorType.Subtraction:
                    ApplySubtractionOperator();
                    break;
                case OperatorType.Multiplication:
                    ApplyMultiplicationOperator();
                    break;
                case OperatorType.Division:
                    ApplyDivisionOperator();
                    break;
                case OperatorType.Exponentiation:
                    ApplyExponentiationOperator();
                    break;
                case OperatorType.Sin:
                    ApplySinOperator();
                    break;
                case OperatorType.Cos:
                    ApplyCosOperator();
                    break;
                case OperatorType.Tan:
                    ApplyTanOperator();
                    break;
                case OperatorType.Log:
                    ApplyLogOperator();
                    break;
                case OperatorType.Factorial:
                    ApplyFactorialOperator();
                    break;
                default:
                    var exMessage = $"An uknown operator type: {operatorToken.OperatorType}.";
                    throw new SyntaxException(exMessage);
            }
        }

        void ApplyAdditionOperator()
        {
            var operands = GetBinaryOperatorArguments();
            var result = new OperandToken(operands.Item1.Value + operands.Item2.Value);
            operandTokensStack.Push(result);
        }

        void ApplySubtractionOperator()
        {
            var operands = GetBinaryOperatorArguments();
            var result = new OperandToken(operands.Item1.Value - operands.Item2.Value);
            operandTokensStack.Push(result);
        }

        void ApplyMultiplicationOperator()
        {
            var operands = GetBinaryOperatorArguments();
            var result = new OperandToken(operands.Item1.Value * operands.Item2.Value);
            operandTokensStack.Push(result);
        }

        void ApplyDivisionOperator()
        {
            var operands = GetBinaryOperatorArguments();
            var result = new OperandToken(operands.Item1.Value / operands.Item2.Value);
            operandTokensStack.Push(result);
        }

        void ApplyExponentiationOperator()
        {
            var operands = GetBinaryOperatorArguments();
            var result = new OperandToken((decimal)Math.Pow(decimal.ToDouble(operands.Item1.Value), decimal.ToDouble(operands.Item2.Value)));
            operandTokensStack.Push(result);
        }

        void ApplySinOperator()
        {
            var operand = GetUnaryOperatorArgument();
            var result = new OperandToken((decimal)Math.Sin((Math.PI / 180) * decimal.ToDouble(operand.Value)));
            operandTokensStack.Push(result);
        }

        void ApplyCosOperator()
        {
            var operand = GetUnaryOperatorArgument();
            var result = new OperandToken((decimal)Math.Cos((Math.PI / 180) * decimal.ToDouble(operand.Value)));
            operandTokensStack.Push(result);
        }

        void ApplyTanOperator()
        {
            var operand = GetUnaryOperatorArgument();
            var result = new OperandToken((decimal)Math.Tan((Math.PI / 180) * decimal.ToDouble(operand.Value)));
            operandTokensStack.Push(result);
        }
        
        void ApplyLogOperator()
        {
            var operand = GetUnaryOperatorArgument();
            var result = new OperandToken((decimal)Math.Log10(decimal.ToDouble(operand.Value)));
            operandTokensStack.Push(result);
        }

        void ApplyFactorialOperator()
        {
            var operand = GetUnaryOperatorArgument();
            decimal fact = operand.Value;
            for (int i = Convert.ToInt32(operand.Value) - 1; i >= 1; i--)
            {
                fact *= i;
            }
            var result = new OperandToken(fact);
            operandTokensStack.Push(result);
        }

        Tuple<OperandToken, OperandToken> GetBinaryOperatorArguments()
        {
            if (operandTokensStack.Count < 2)
            {
                var exMessage = "Not enough arguments for applying a binary operator.";
                throw new SyntaxException(exMessage);
            }

            var right = operandTokensStack.Pop();
            var left = operandTokensStack.Pop();

            return Tuple.Create(left, right);
        }

        OperandToken GetUnaryOperatorArgument()
        {
            if (operandTokensStack.Count < 1)
            {
                var exMessage = "Not enough arguments for applying an unary operator.";
                throw new SyntaxException(exMessage);
            }
            var op = operandTokensStack.Pop();

            return op;
        }

        OperandToken GetResult()
        {
            if (operandTokensStack.Count == 0)
            {
                var exMessage = "The expression is invalid. Check please that the expression is not empty.";
                throw new SyntaxException(exMessage);
            }
            if (operandTokensStack.Count != 1)
            {
                var exMessage = "The expression is invalid. Check please that you're providing the full expression and the tokens have a correct order.";
                throw new SyntaxException(exMessage);
            }

            return operandTokensStack.Pop();
        }

    }
    public interface IToken { }

    public class OperandToken : IToken
    {
        readonly decimal value;
        public decimal Value { get { return value; } }

        public OperandToken(decimal value)
        {
            this.value = value;
        }

    }

    public enum OperatorType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Exponentiation,
        OpeningBracket,
        ClosingBracket,
        Sin,
        Cos,
        Tan,
        Log,
        Factorial
    }

    public class OperatorToken : IToken
    {
        private readonly OperatorType operatorType;
        public OperatorType OperatorType { get { return operatorType; } }

        public OperatorToken(OperatorType operatorType)
        {
            this.operatorType = operatorType;
        }

    }

    public class MathExpressionException : Exception
    {
        public MathExpressionException(string message) : base(message)
        {

        }
    }

    public class SyntaxException : MathExpressionException
    {
        public SyntaxException(string message) : base(message)
        {

        }
    }

    public class UnitTests // Unit tests for checking if all functions work correctly
    {
        readonly Tokenizer tokenizer;
        readonly ShuntingYardAlgorithm shuntingYard;
        readonly PostfixNotationCalculator calculator;

        public UnitTests()
        {
            tokenizer = new Tokenizer();
            shuntingYard = new ShuntingYardAlgorithm();
            calculator = new PostfixNotationCalculator();
        }

        [Fact]
        public void Question1Tests()
        {
            Assert.Equal("7", Calculate("2 + 5"));
            Assert.Equal("5", Calculate("8 - 3"));
            Assert.Equal("20", Calculate("5 * 4"));
            Assert.Equal("4", Calculate("8 / 2"));
            Assert.Equal("16", Calculate("4 ^ 2"));
        }

        [Fact]
        public void Question2Tests()
        {
            Assert.Equal("7", Calculate("1 + 2 * 3"));
            Assert.Equal("9", Calculate("(1 + 2) * 3"));
            Assert.Equal("19", Calculate("6 + 3 - 2 + 12"));
            Assert.Equal("53", Calculate("2 * 15 + 23"));
            Assert.Equal("1", Calculate("10 - 3 ^ 2"));
        }

        [Fact]
        public void Question3Tests()
        {
            Assert.Equal("10.5", Calculate("3.5 * 3"));
            Assert.Equal("-77", Calculate("-53 + -24"));
            calculator.DecimalPrecision = 3;
            Assert.Equal("3.333", Calculate("10 / 3"));
            calculator.DecimalPrecision = -1;
            Assert.Equal("-18", Calculate("(-20 * 1.8) / 2"));
            calculator.DecimalPrecision = 3;
            Assert.Equal("-54.315", Calculate("-12.315 - 42"));
            calculator.DecimalPrecision = -1;
        }

        [Fact]
        public void Question4Tests()
        {
            calculator.DecimalPrecision = 0;
            Assert.Equal("125", Calculate("10 * (sin(30) * 25)"));
            calculator.DecimalPrecision = -1;
            Assert.Equal("24", Calculate("(2! * 2!)!"));
            Assert.Equal("69096", Calculate("100 * ((1 + E7) * 3 - (2.2 * 1.1) / 0.5 + -0.2)"));
            Assert.Equal("690.96", Calculate("(1 + E7) * 3 - (2.2 * 1.1) / 0.5 + -0.2"));
        }

        string Calculate(string input)
        {
            var infixNotationTokens = tokenizer.Parse(input);
            var postfixNotationTokens = shuntingYard.Apply(infixNotationTokens);
            return calculator.Calculate(postfixNotationTokens);
        }
    }
}