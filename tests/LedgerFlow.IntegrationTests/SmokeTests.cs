using System.Net;
using LedgerFlow.IntegrationTests.Infrastructure;
using Npgsql;

namespace LedgerFlow.IntegrationTests;

public sealed class SmokeTests(PostgresIntegrationTestFactory factory)
    : IClassFixture<PostgresIntegrationTestFactory>
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostgresContainer_ShouldSupportPgVectorExtension()
    {
        await using var connection = new NpgsqlConnection(factory.ConnectionString);
        await connection.OpenAsync();

        await using var createCmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector;", connection);
        await createCmd.ExecuteNonQueryAsync();

        await using var verifyCmd = new NpgsqlCommand(
            "SELECT extname FROM pg_extension WHERE extname = 'vector';",
            connection);
        var extName = (string?)await verifyCmd.ExecuteScalarAsync();

        Assert.Equal("vector", extName);
    }
}
