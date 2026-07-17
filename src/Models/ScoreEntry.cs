using System.Runtime.Serialization;

namespace JinxApp.Models
{
    /// <summary>
    /// Entrée du classement des meilleurs scores : nom d'un joueur et son score
    /// final sur une partie. Persistée pour conserver le top des scores.
    /// </summary>
    [DataContract(Name = "scoreEntry")]
    public class ScoreEntry
    {
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; } = "";

        [DataMember(Name = "score", Order = 1)]
        public int Score { get; set; }

        [DataMember(Name = "isAi", Order = 2)]
        public bool IsAi { get; set; }

        public ScoreEntry() { }

        public ScoreEntry(string name, int score, bool isAi = false)
        {
            Name = name;
            Score = score;
            IsAi = isAi;
        }
    }
}
