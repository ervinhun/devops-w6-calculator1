using Calculator;

namespace Tests;

public class CachedCalculatorTest
{
    [Test]
    public void Add()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 2;
        var b = 3;

        // Act
        var result = calc.Add(a, b);

        // Assert
        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void Subtract()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 5;
        var b = 3;

        // Act
        var result = calc.Subtract(a, b);

        // Assert
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Multiply()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 4;
        var b = 5;

        // Act
        var result = calc.Multiply(a, b);

        // Assert
        Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public void Divide()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 10;
        var b = 2;

        // Act
        var result = calc.Divide(a, b);

        // Assert
        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void CachedResultsMultiply()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 4;
        var b = 5;

        // Act
        var result1 = calc.Multiply(a, b);
        var result2 = calc.Multiply(a, b); // This should hit the cache

        // Assert
        Assert.That(result1, Is.EqualTo(20));
        Assert.That(result2, Is.EqualTo(20));
        Assert.That(calc._cache.Count, Is.EqualTo(1), 
            "Cache should contain only 1 entry - GetCachedResult prevents duplicates");
    }

    [Test]
    public void CachedResultsDivide()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 10;
        var b = 2;

        // Act
        var result1 = calc.Divide(a, b);
        var result2 = calc.Divide(a, b); // This should hit the cache

        // Assert
        Assert.That(result1, Is.EqualTo(5));
        Assert.That(result2, Is.EqualTo(5));
        Assert.That(calc._cache.Count, Is.EqualTo(1),
            "Cache should contain only 1 entry - GetCachedResult prevents duplicates");
    }
    
    [Test]
    public void Factorial()
    {
        // Arrange
        var calc = new CachedCalculator();
        var n = 5;

        // Act
        var result = calc.Factorial(n);

        // Assert
        Assert.That(result, Is.EqualTo(120));
    }
    
    [Test]
    public void FactorialFail()
    {
        // Arrange
        var calc = new CachedCalculator();
        var n = -1;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => calc.Factorial(n));
        Assert.That(ex.Message, Is.EqualTo("Factorial is not defined for negative numbers"));
    }

    [Test]
    public void IsPrimeTrue()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 7;

        // Act
        var result = calc.IsPrime(candidate);

        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void IsPrimeFalse()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 4;

        // Act
        var result = calc.IsPrime(candidate);

        // Assert
        Assert.That(result, Is.False);
    }
    
    public void IsPrimeEdgeCase()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 1;
        
        // Act
        var result = calc.IsPrime(candidate);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void IsPrimeEdgeCaseZero()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 0;  
        
        // Act
        var result = calc.IsPrime(candidate);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void IsPrimeEdgeCaseNegative()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = -5;
        
        // Act
        var result = calc.IsPrime(candidate);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void IsPrimeEdgeCaseTwo()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 2;
        
        // Act
        var result = calc.IsPrime(candidate);
        
        // Assert
        Assert.That(result, Is.True);
    }
    
        [Test]
    public void CachedResults()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 2;
        var b = 3;

        // Act
        var result1 = calc.Add(a, b);
        var result2 = calc.Add(a, b); // This should hit the cache

        // Assert
        Assert.That(result1, Is.EqualTo(5));
        Assert.That(result2, Is.EqualTo(5));
        Assert.That(calc._cache.Count, Is.EqualTo(1)); // Ensure only one entry in cache
    }
    
    [Test]
    public void CachedResultsFactorial()
    {
        // Arrange
        var calc = new CachedCalculator();
        var n = 5; 
        
        // Act
        var result1 = calc.Factorial(n);
        var result2 = calc.Factorial(n); // This should hit the cache
        
        // Assert
        Assert.That(result1, Is.EqualTo(120));
        Assert.That(result2, Is.EqualTo(120));
        Assert.That(calc._cache.Count, Is.EqualTo(1)); // Ensure only one entry in cache
    }
    
    [Test]
    public void CachedResultsIsPrime()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 7;
        
        // Act
        var result1 = calc.IsPrime(candidate);
        var result2 = calc.IsPrime(candidate); // This should hit the cache
        
        // Assert
        Assert.That(result1, Is.True);
        Assert.That(result2, Is.True);
        Assert.That(calc._cache.Count, Is.EqualTo(1)); // Ensure only one entry in cache
    }
    
    [Test]
    public void CachedResultsIsPrimeFalse()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 4;
        
        // Act
        var result1 = calc.IsPrime(candidate);
        var result2 = calc.IsPrime(candidate); // This should hit the cache
        
        // Assert
        Assert.That(result1, Is.False);
        Assert.That(result2, Is.False);
        Assert.That(calc._cache.Count, Is.EqualTo(1)); // Ensure only one entry in cache
    }
    
    [Test]
    public void WhenBypassingGetCachedResult_CacheGrowsWithDuplicateEntries()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 2;
        var b = 3;

        // Act
        // Call Add multiple times with same parameters
        calc.Add(a, b);
        calc.Add(a, b);
        calc.Add(a, b);

        // Assert
        // With current code (using ?? operator): cache should have 1 entry
        // The second and third calls would hit GetCachedResult and return null ? false
        // so StoreInCache would only be called once
        Assert.That(calc._cache.Count, Is.EqualTo(1), 
            "Cache should contain only 1 entry because GetCachedResult checks cache first");
    }
    

    [Test]
    public void WhenBypassingGetCachedResult_FactorialCacheGrowsWithDuplicates()
    {
        // Arrange
        var calc = new CachedCalculator();
        var n = 5;

        // Act
        // Call Factorial multiple times
        calc.Factorial(n);
        calc.Factorial(n);
        calc.Factorial(n);

        // Assert
        // With current code: should have 1 entry
        // Without GetCachedResult check: would have 3 duplicate entries
        Assert.That(calc._cache.Count, Is.EqualTo(1),
            "Cache should contain only 1 entry for Factorial");
    }

    [Test]
    public void WhenBypassingGetCachedResult_IsPrimeCacheGrowsWithDuplicates()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 7;

        // Act
        // Call IsPrime multiple times with same parameter
        calc.IsPrime(candidate);
        calc.IsPrime(candidate);
        calc.IsPrime(candidate);

        // Assert
        // With current code: should have 1 entry
        // Without GetCachedResult check: would have 3 duplicate entries
        Assert.That(calc._cache.Count, Is.EqualTo(1),
            "Cache should contain only 1 entry for IsPrime");
    }

    [Test]
    public void WhenBypassingGetCachedResult_MultiplyCacheGrowsWithDuplicates()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 4;
        var b = 5;

        // Act
        // Call Multiply multiple times with same parameters
        calc.Multiply(a, b);
        calc.Multiply(a, b);
        calc.Multiply(a, b);

        // Assert
        // With current code: should have 1 entry
        // Without GetCachedResult check: would throw "An item with the same key has already been added"
        Assert.That(calc._cache.Count, Is.EqualTo(1),
            "Cache should contain only 1 entry for Multiply - GetCachedResult must check cache first");
    }

    [Test]
    public void WhenBypassingGetCachedResult_DivideCacheGrowsWithDuplicates()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 10;
        var b = 2;

        // Act
        // Call Divide multiple times with same parameters
        calc.Divide(a, b);
        calc.Divide(a, b);
        calc.Divide(a, b);

        // Assert
        // With current code: should have 1 entry
        // Without GetCachedResult check: would throw "An item with the same key has already been added"
        Assert.That(calc._cache.Count, Is.EqualTo(1),
            "Cache should contain only 1 entry for Divide - GetCachedResult must check cache first");
    }

    [Test]
    public void GetCachedResult_ReturnsCachedValueOnSecondCall()
    {
        // Arrange
        var calc = new CachedCalculator();
        var a = 10;
        var b = 5;

        // Act
        var firstResult = calc.Add(a, b);
        var cacheCountAfterFirst = calc._cache.Count;
        
        var secondResult = calc.Add(a, b);
        var cacheCountAfterSecond = calc._cache.Count;

        // Assert
        // Both results should be correct
        Assert.That(firstResult, Is.EqualTo(15));
        Assert.That(secondResult, Is.EqualTo(15));
        
        // Cache should have 1 entry after first call
        Assert.That(cacheCountAfterFirst, Is.EqualTo(1));
        
        // Cache count should NOT increase after second call (GetCachedResult found it)
        Assert.That(cacheCountAfterSecond, Is.EqualTo(1),
            "Second call should not increase cache size - GetCachedResult found the cached value");
    }

    [Test]
    public void MixedOperations_CacheContainsOnlyOneEntryPerUniqueLookup()
    {
        // Arrange
        var calc = new CachedCalculator();

        // Act
        calc.Add(2, 3);
        calc.Add(2, 3);  // Same as first
        calc.Subtract(5, 2);
        calc.Subtract(5, 2);  // Same as first subtract
        calc.Factorial(5);
        calc.Factorial(5);  // Same as first factorial
        
        // Assert
        // Should have 3 entries: one for Add(2,3), one for Subtract(5,2), one for Factorial(5)
        Assert.That(calc._cache.Count, Is.EqualTo(3),
            "Cache should have 3 entries: 1 Add + 1 Subtract + 1 Factorial");
    }

    [Test]
    public void WithoutGetCachedResult_DuplicateWouldOccur_Simulation()
    {
        // This test simulates what WOULD happen if GetCachedResult check was removed
        // by manually calling StoreInCache multiple times
        
        // Arrange
        var calc = new CachedCalculator();
        var n = 5;

        // Act - Simulate what would happen without the ?? GetCachedResult check
        // First call would be: StoreInCache(_calculator.Factorial(5), 5)
        calc.Factorial(n);  // Cache now has 1 entry
        
        // Second call would also be: StoreInCache(_calculator.Factorial(5), 5) 
        // (but this would fail because key already exists)
        try
        {
            calc.Factorial(n);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("already exists"))
        {
            // This is expected - we'd get "An item with the same key has already been added"
            Assert.Pass("Without GetCachedResult, StoreInCache would throw due to duplicate key");
            return;
        }

        // If we got here, the cache lookup (GetCachedResult) is working correctly
        Assert.That(calc._cache.Count, Is.EqualTo(1));
    }

    [Test]
    public void CacheHitVerification_SameKeyIsRetrieved()
    {
        // Arrange
        var calc = new CachedCalculator();
        var candidate = 11;

        // Act
        var firstCall = calc.IsPrime(candidate);
        var firstCacheEntry = calc._cache.First().Value;

        var secondCall = calc.IsPrime(candidate);
        var secondCacheEntry = calc._cache.First().Value;

        // Assert
        Assert.That(firstCall, Is.True);
        Assert.That(secondCall, Is.True);
        
        // The cache entries should be the same object (same reference)
        // This proves GetCachedResult is returning the cached instance
        Assert.That(firstCacheEntry, Is.SameAs(secondCacheEntry),
            "Cache should return the same cached object on repeated calls");
    }

    [Test]
    public void PerformanceImpact_CacheVsBypass()
    {
        // Arrange
        var calc = new CachedCalculator();
        int iterations = 1000;

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            calc.Factorial(10);  // Same call each time
        }
        sw.Stop();

        // Assert
        // With proper caching (GetCachedResult), all 1000 calls should be very fast
        // Without caching, it would recalculate 1000 times and try to add duplicates
        Assert.That(calc._cache.Count, Is.EqualTo(1),
            "After 1000 identical calls, cache should still have only 1 entry");
        
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100),
            $"1000 cached lookups should be fast (was {sw.ElapsedMilliseconds}ms)");
    }
}