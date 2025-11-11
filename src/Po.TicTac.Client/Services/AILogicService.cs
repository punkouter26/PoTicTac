using PoTicTac.Client.Models;
using PoTicTac.Client.Services.AIStrategies;

namespace PoTicTac.Client.Services;

public class AILogicService
{
    private readonly Dictionary<Difficulty, IAIStrategy> _strategies;

    public AILogicService()
    {
        _strategies = new Dictionary<Difficulty, IAIStrategy>
        {
            { Difficulty.Easy, new EasyAIStrategy() },
            { Difficulty.Medium, new MediumAIStrategy() },
            { Difficulty.Hard, new HardAIStrategy() }
        };
    }

    public async Task<int[]> GetAiMove(GameBoardState gameState, Difficulty difficulty)
    {
        return await _strategies[difficulty].GetMoveAsync(gameState);
    }
}
