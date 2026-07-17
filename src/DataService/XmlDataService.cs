using JinxApp.Models;
using System.Runtime.Serialization;
using System.Xml;

namespace JinxApp.DataService
{
    public class XmlDataService : IDataService
    {
        private readonly string currentGamePath;
        private readonly string historyPath;
        private readonly string highScoresPath;

        private static readonly DataContractSerializer gameSerializer =
            new DataContractSerializer(
                typeof(Game),
                new DataContractSerializerSettings
                {
                    PreserveObjectReferences = true,
                    KnownTypes = new[]
                    {
                        typeof(NumberCard), typeof(ChanceCard),
                        typeof(Board), typeof(Dice),
                        typeof(Player), typeof(GameHistory),
                        typeof(Tornade), typeof(Orage)
                    }
                });

        private static readonly DataContractSerializer historySerializer =
            new DataContractSerializer(typeof(List<GameHistory>));

        private static readonly DataContractSerializer highScoresSerializer =
            new DataContractSerializer(typeof(List<ScoreEntry>));

        public XmlDataService(string saveDirectory)
        {
            currentGamePath = Path.Combine(saveDirectory, "current_game.xml");
            historyPath = Path.Combine(saveDirectory, "history.xml");
            highScoresPath = Path.Combine(saveDirectory, "scores.xml");
            Directory.CreateDirectory(saveDirectory);
        }

        public void SaveCurrentGame(Game game) => WriteXml(gameSerializer, currentGamePath, game);
        public Game? LoadCurrentGame() => ReadXml<Game>(gameSerializer, currentGamePath);
        public bool HasCurrentGame() => File.Exists(currentGamePath);
        public void DeleteCurrentGame() { if (File.Exists(currentGamePath)) File.Delete(currentGamePath); }

        public void SaveHistory(List<GameHistory> history) => WriteXml(historySerializer, historyPath, history);
        public List<GameHistory> LoadHistory() => ReadXml<List<GameHistory>>(historySerializer, historyPath) ?? new();

        public void SaveHighScores(List<ScoreEntry> scores) => WriteXml(highScoresSerializer, highScoresPath, scores);
        public List<ScoreEntry> LoadHighScores() => ReadXml<List<ScoreEntry>>(highScoresSerializer, highScoresPath) ?? new();

        private static void WriteXml(DataContractSerializer s, string path, object obj)
        {
            var settings = new XmlWriterSettings { Indent = true };
            using var tw = File.CreateText(path);
            using var writer = XmlWriter.Create(tw, settings);
            s.WriteObject(writer, obj);
        }

        private static T? ReadXml<T>(DataContractSerializer s, string path) where T : class
        {
            if (!File.Exists(path)) return null;
            using var stream = File.OpenRead(path);
            return s.ReadObject(stream) as T;
        }
    }
}