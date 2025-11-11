using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public interface IAIStrategy
{
    public Task<int[]> GetMoveAsync(GameBoardState gameState);
}
