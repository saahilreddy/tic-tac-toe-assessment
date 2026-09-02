using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TicTacToe.Tests.Integration;

public sealed class GamesApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Can_create_game_and_read_state()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/games", new { mode = "TwoPlayer" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var state = await create.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(state);
        Assert.Equal("X", state!.RootElement.GetProperty("currentPlayer").GetString());
        Assert.Equal("InProgress", state.RootElement.GetProperty("gameStatus").GetString());
    }

    [Fact]
    public async Task Invalid_move_returns_bad_request_problem_details()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/games", new { mode = "TwoPlayer" });
        var state = await create.Content.ReadFromJsonAsync<JsonDocument>();
        var gameId = state!.RootElement.GetProperty("gameId").GetGuid();

        var move = await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/moves", new
        {
            gameId,
            player = "O",
            row = 0,
            column = 0,
            expectedVersion = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, move.StatusCode);
    }
}
