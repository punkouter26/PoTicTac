using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public interface IAIStrategy
{
    Task<int[]> GetMoveAsync(GameBoardState gameState);
}
