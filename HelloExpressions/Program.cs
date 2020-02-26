using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelloExpressions
{
    // обычная запись
    // {КСС.ВОСПР} == НЕТ && {МСБОИ.СЗСЗУ} == 0xFF && {КАДР}[22] & 0x80 == 0x80
    // или
    // ({КСС.ВОСПР} == НЕТ) && ({МСБОИ.СЗСЗУ} == 0xFF) && ({КАДР}[22] & 0x80 == 0x80)

    // польская нотация
    // && && == {КСС.ВОСПР} НЕТ == {МСБОИ.СЗСЗУ} 0xFF & {КАДР}[22] 0x80
    // или
    // && (&& (== {КСС.ВОСПР} НЕТ) (== {МСБОИ.СЗСЗУ} 0xFF)) (& {КАДР}[22] 0x80)



    public class BinaryOperator
    {
        private readonly Func<Expression, Expression, BinaryExpression> _expression;
        public string Name { get; }

        public BinaryExpression GetExpression(Expression left, Expression right) => _expression.Invoke(left, right);

        protected BinaryOperator(string name) => Name = name;

        public BinaryOperator(string name, Func<Expression, Expression, BinaryExpression> getExpression) : this(name) =>
            _expression = getExpression;
    }
    
    //public class BinaryOperator<T>:BinaryOperator
    //{
    //    public BinaryOperator(string name) : base(name)
    //    {
    //    }

    //    public BinaryOperator(string name, Func<string, string, BinaryExpression> expression) : base(name, expression)
    //    {
    //    }
    //}



    internal static class Program
    {
        private static readonly BinaryOperator[] LogicOperators =
        {
            new BinaryOperator("&&", Expression.AndAlso),
            new BinaryOperator("||", Expression.OrElse),
        };

        private static readonly BinaryOperator[] CompareOperators =
        {
            new BinaryOperator("==", Expression.Equal),
            new BinaryOperator("!=", Expression.NotEqual),
            new BinaryOperator(">", Expression.GreaterThan),
            new BinaryOperator(">=", Expression.GreaterThanOrEqual),
            new BinaryOperator("<", Expression.LessThan),
            new BinaryOperator("<=", Expression.LessThanOrEqual)
        };

        private static readonly BinaryOperator[] CalculateOperators =
        {
            new BinaryOperator("*", Expression.Multiply),
            new BinaryOperator("/", Expression.Divide),
            new BinaryOperator("%", Expression.Modulo),
            new BinaryOperator("&", Expression.And),
            new BinaryOperator("|", Expression.Or),
            new BinaryOperator("^", Expression.ExclusiveOr),
            new BinaryOperator("+", Expression.Add),
            new BinaryOperator("-", Expression.Subtract)
        };



        //private static readonly string[] LogicOperators = {"&&", "||"};
        //static readonly string[] CompareOperators = { "==", "!=", ">", ">=", "<", "<=" };
        //static readonly string[] CalculateOperators = { "+", "-", "*", "/", "%", "&", "|", "^" };

        const string TestExpression = "({КСС.ВОСПР} == НЕТ) && ({МСБОИ.СЗСЗУ} == 0xFF) && ({КАДР}[22] & 0x80 == 0x80)";
        const string TestExpressionP = "&& (&& (== {КСС.ВОСПР} НЕТ) (== {МСБОИ.СЗСЗУ} 0xFF)) (& {КАДР}[22] 0x80)";
        

        private static void Main(string[] args)
        {
            Expression<Func<int, int, int>> addExpression = (a, b) => a + b;
            var addExpr = addExpression.Compile();
            var result = addExpr(5, 3);

            Console.WriteLine($"addExpr(5, 3) = {result}");


            var expr = ParseExpression(TestExpression);
            //Console.WriteLine($"{TestExpression} = {}");

            
        }

        private static Expression ParseExpression(string text)
        {
            Debug.WriteLine($"ParseExpression(\"{text}\")");

            text = text.Trim();
            if (text.StartsWith("("))
            {
                if (text.EndsWith(")"))
                {
                    text = text.Substring(1, text.Length - 2);
                    Debug.WriteLine("braces removed");
                }
                else
                    throw new FormatException($"не найдена закрывающая скобка при наличии открывающей в строке {text}");
            }
            else if(text.EndsWith(")"))
                throw new FormatException($"не найдена открывающая скобка при наличии закрывающей в строке {text}");


            var operatorSets = new List<IEnumerable<BinaryOperator>>
            {
                LogicOperators,
                CompareOperators,
                CalculateOperators
            };

            foreach (var operatorSet in operatorSets)
            {
                var op = operatorSet.FirstOrDefault(o => text.Contains(o.Name));
                if (op != default)
                    return GetExpression(op, text);
            }



        }


        private static Expression GetExpression(BinaryOperator op, string text)
        {
            Debug.WriteLine($"operator: {op}");
            SplitBinaryExpressionString(text, op.Name, out var left, out var right);
            return op.GetExpression(ParseExpression(left), ParseExpression(right));
        }

        private static void SplitBinaryExpressionString(
            string expressionString, string operatorString,
            out string left, out string right)
        {
            var posOp = expressionString.IndexOf(operatorString, StringComparison.CurrentCulture);

            left = expressionString.Substring(0, posOp);
            right = expressionString.Substring(posOp + operatorString.Length, 
                expressionString.Length - (posOp + operatorString.Length));
        }

        private static BinaryExpression GetLogicExpression(string op, string left, string right)
        {
            if (op == "&&")
                return Expression.And(ParseExpression(left), ParseExpression(right));
            if (op == "||")
                return Expression.Or(ParseExpression(left), ParseExpression(right));
            throw new Exception();
        }
    }
}
