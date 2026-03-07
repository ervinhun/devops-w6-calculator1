using System.Runtime.CompilerServices;

namespace Calculator;

public class CachedCalculator : ICalculator
{
    private readonly SimpleCalculator _calculator = new();
    public Dictionary<string, Calculation> Cache { get; } = new();

    public int Add(int a, int b)
    {
        var calc = GetCachedResult<int>(a, b) ?? StoreInCache(_calculator.Add(a, b), a, b);
        return calc.Result;
    }

    public int Subtract(int a, int b)
    {        
        var calc = GetCachedResult<int>(a, b) ?? StoreInCache(_calculator.Subtract(a, b), a, b);
        return calc.Result;
    }

    public int Multiply(int a, int b)
    {        
        var calc = GetCachedResult<int>(a, b) ?? StoreInCache(_calculator.Multiply(a, b), a, b);
        return calc.Result;
    }

    public int Divide(int a, int b)
    {        
        var calc = GetCachedResult<int>(a, b) ?? StoreInCache(_calculator.Divide(a, b), a, b);
        return calc.Result;
    }

    public int Factorial(int n)
    {
        var calc = GetCachedResult<int>(n) ?? StoreInCache(_calculator.Factorial(n), n);
        return calc.Result;
    }

    public bool IsPrime(int candidate)
    {
        var calc = GetCachedResult<bool>(candidate) ?? StoreInCache(_calculator.IsPrime(candidate), candidate);
        return calc.Result;
    }
    
    private Calculation<T>? GetCachedResult<T>(int a, int? b = null, [CallerMemberName]string operation = "")
    {
        var calc = new Calculation<T>(default, operation, a, b);
        if (Cache.ContainsKey(calc.GetKey()))
        {
            return (Calculation<T>?)Cache[calc.GetKey()];
        }
        return null;
    }
    
    private Calculation<T> StoreInCache<T>(T result, int a, int? b = null, [CallerMemberName]string operation = "")
    {
        var calc = new Calculation<T>(result, operation, a, b);
        calc.Result = result;
        Cache.Add(calc.GetKey(), calc);
        return calc;
    }
    
    public class Calculation
    {
        private int A { get; }
        private int? B { get; }
        private string Operation { get; }

        protected Calculation(string operation, int a, int? b = null)
        {
            A = a;
            B = b;
            Operation = operation;
        }
    
        public string GetKey()
        {
            return string.Concat(A, Operation, B);
        }
    }

    private sealed class Calculation<T>(T? result, string operation, int a, int? b = null)
        : Calculation(operation, a, b)
    {
        public T? Result { get; set; } = result;
    }
}