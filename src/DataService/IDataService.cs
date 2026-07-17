using JinxApp.Models;

namespace JinxApp.DataService
{
    public interface IDataService
    {
        void SaveCurrentGame(Game game);
        Game? LoadCurrentGame();
        bool HasCurrentGame();
        void DeleteCurrentGame();

        void SaveHistory(List<GameHistory> history);
        List<GameHistory> LoadHistory();

        void SaveHighScores(List<ScoreEntry> scores);
        List<ScoreEntry> LoadHighScores();
    }
}