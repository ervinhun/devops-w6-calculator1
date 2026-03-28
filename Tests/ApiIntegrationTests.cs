using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Calculator.Api.Contracts;
using Calculator.Api.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tests;

public class ApiIntegrationTests
{
    [Test]
    public async Task PostCalculations_HappyPath_PersistsHistoryAndReturnsResult()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "add",
            a = 2,
            b = 3
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Multiple(() =>
        {
            Assert.That(body.RootElement.GetProperty("result").GetString(), Is.EqualTo("5"));
            Assert.That(body.RootElement.GetProperty("historyPersisted").GetBoolean(), Is.True);
            Assert.That(repository.Inserts.Count, Is.EqualTo(1));
            Assert.That(repository.Inserts[0].Operation, Is.EqualTo("Add"));
            Assert.That(repository.Inserts[0].Success, Is.True);
        });
    }

    [Test]
    public async Task PostCalculations_ValidationFailure_ReturnsBadRequest()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "",
            a = 10,
            b = 2
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.Inserts, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task PostCalculations_MissingBinaryOperand_ReturnsBadRequestAndPersistsFailure()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "divide",
            a = 10
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.Inserts, Has.Count.EqualTo(1));
        Assert.That(repository.Inserts[0].Success, Is.False);
        Assert.That(repository.Inserts[0].ErrorMessage, Does.Contain("requires operand b"));
    }

    [Test]
    public async Task PostCalculations_UnknownOperation_ReturnsBadRequest()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "mod",
            a = 10,
            b = 2
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.Inserts, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task GetRecent_ReturnsDescendingByCreatedAt_AndAppliesTake()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/calculations", new { operation = "add", a = 1, b = 1 });
        await client.PostAsJsonAsync("/api/calculations", new { operation = "multiply", a = 2, b = 3 });
        await client.PostAsJsonAsync("/api/calculations", new { operation = "factorial", a = 4 });

        var response = await client.GetAsync("/api/calculations/recent?take=2");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var json = JsonDocument.Parse(payload);
        var items = json.RootElement.EnumerateArray().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(items.Length, Is.EqualTo(2));
            Assert.That(items[0].GetProperty("operation").GetString(), Is.EqualTo("Factorial"));
            Assert.That(items[1].GetProperty("operation").GetString(), Is.EqualTo("Multiply"));
        });
    }

    [Test]
    public async Task GetRecent_ClampsTakeBetweenOneAndHundred()
    {
        var repository = new InMemoryHistoryRepository();
        for (var i = 0; i < 120; i += 1)
        {
            await repository.InsertAsync("Add", i, i, (i + i).ToString(), true, null);
        }

        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var highResponse = await client.GetAsync("/api/calculations/recent?take=500");
        using (var highDoc = JsonDocument.Parse(await highResponse.Content.ReadAsStringAsync()))
        {
            var highItems = highDoc.RootElement.EnumerateArray().ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(highResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(repository.LastRequestedTake, Is.EqualTo(100));
                Assert.That(highItems.Length, Is.EqualTo(100));
            });
        }

        var lowResponse = await client.GetAsync("/api/calculations/recent?take=-10");
        using (var lowDoc = JsonDocument.Parse(await lowResponse.Content.ReadAsStringAsync()))
        {
            var lowItems = lowDoc.RootElement.EnumerateArray().ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(lowResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(repository.LastRequestedTake, Is.EqualTo(1));
                Assert.That(lowItems.Length, Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task PostCalculations_DbDown_ReturnsOkWithHistoryPersistedFalse()
    {
        var repository = new InMemoryHistoryRepository { ThrowOnInsert = true };
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "add",
            a = 3,
            b = 4
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Multiple(() =>
        {
            Assert.That(body.RootElement.GetProperty("result").GetString(), Is.EqualTo("7"));
            Assert.That(body.RootElement.GetProperty("historyPersisted").GetBoolean(), Is.False);
        });
    }

    [Test]
    public async Task GetRecent_DbDown_ReturnsServiceUnavailable()
    {
        var repository = new InMemoryHistoryRepository { ThrowOnGetRecent = true };
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/calculations/recent");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public async Task PostCalculations_Subtract_HappyPath_ReturnsExpectedResult()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "subtract",
            a = 9,
            b = 4
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(body.RootElement.GetProperty("result").GetString(), Is.EqualTo("5"));
        Assert.That(repository.Inserts[0].Operation, Is.EqualTo("Subtract"));
    }

    [Test]
    public async Task PostCalculations_IsPrime_HappyPath_ReturnsBooleanResult()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "isprime",
            a = 7
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(body.RootElement.GetProperty("result").GetString(), Is.EqualTo("true"));
        Assert.That(repository.Inserts[0].Operation, Is.EqualTo("IsPrime"));
    }

    [Test]
    public async Task PostCalculations_FactorialMaxBoundary_Succeeds()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "factorial",
            a = 12
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(body.RootElement.GetProperty("result").GetString(), Is.EqualTo("479001600"));
        Assert.That(repository.Inserts[^1].Success, Is.True);
    }

    [Test]
    public async Task PostCalculations_FactorialAboveLimit_ReturnsBadRequestAndPersistsFailure()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "factorial",
            a = 100000
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.Inserts[^1].Success, Is.False);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(body.RootElement.GetProperty("error").GetString(), Does.Contain("up to 12"));
    }

    [Test]
    public async Task PostCalculations_FactorialNegative_ReturnsBadRequestAndPersistsFailure()
    {
        var repository = new InMemoryHistoryRepository();
        await using var factory = new ApiFactory(repository);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            operation = "factorial",
            a = -1
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.Inserts[^1].Success, Is.False);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(body.RootElement.GetProperty("error").GetString(), Does.Contain("not defined for negative numbers"));
    }

    private sealed class ApiFactory(ICalculationHistoryRepository repository) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICalculationHistoryRepository>();
                services.AddSingleton(repository);
            });
        }
    }

    private sealed class InMemoryHistoryRepository : ICalculationHistoryRepository
    {
        private readonly List<HistoryItem> _items = [];
        private long _nextId = 1;

        public bool ThrowOnInsert { get; init; }
        public bool ThrowOnGetRecent { get; init; }
        public int LastRequestedTake { get; private set; }
        public IReadOnlyList<HistoryItem> Inserts => _items;

        public Task InsertAsync(string operation, int a, int? b, string? resultText, bool success, string? errorMessage)
        {
            if (ThrowOnInsert)
            {
                throw new InvalidOperationException("Database unavailable");
            }

            _items.Add(new HistoryItem(
                _nextId++,
                operation,
                a,
                b,
                resultText,
                success,
                errorMessage,
                DateTimeOffset.UtcNow));

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<HistoryItem>> GetRecentAsync(int take)
        {
            if (ThrowOnGetRecent)
            {
                throw new InvalidOperationException("Database unavailable");
            }

            LastRequestedTake = take;

            IReadOnlyList<HistoryItem> values = _items
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToList();

            return Task.FromResult(values);
        }
    }
}
