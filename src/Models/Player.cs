using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace JinxApp.Models;

[DataContract(Name = "player")]
public class Player : ObservableObject
{
    private string name = "";

    [DataMember(Name = "name", Order = 0)]
    public string Name
    {
        get => name;
        set { name = value; OnPropertyChanged(); }
    }

    private bool isAi;

    [DataMember(Name = "isAi", Order = 1)]
    public bool IsAi
    {
        get => isAi;
        private set => isAi = value;  // ← setter privé
    }

    private int score;


    /// <summary>Score cumulé du joueur.</summary>

    [DataMember(Name = "score", Order = 2)]

    public int Score
    {
        get => score;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Ne peut pas ajouter un score négatif");
            score = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<NumberCard> numberCards = new();


    /// <summary>Cartes chance en main — ObservableCollection pour notifier l'UI automatiquement.</summary>
    private ObservableCollection<ChanceCard> chanceCards = new();

    // Setter privé requis : DataContractSerializer contourne les constructeurs,
    // une collection get-only resterait nulle à la désérialisation.
    [DataMember(Name = "chanceCards", Order = 4)]
    public ObservableCollection<ChanceCard> ChanceCards
    {
        get => chanceCards;
        private set => chanceCards = value;
    }

    /// <summary>
    /// Initialise un joueur avec son nom et son type.
    /// </summary>

    [DataMember(Name = "numberCards", Order = 3)]
    public ObservableCollection<NumberCard> NumberCards
    {
        get => numberCards;
        private set { numberCards = value; HookHand(); }  // ← setter privé + resync score
    }

    /// <summary>
    /// Abonne le suivi du score à la main : à chaque ajout/retrait de carte
    /// (prise, pénalité de couleur, échange, etc.), le score est resynchronisé
    /// sur la somme des valeurs des cartes détenues — et se met à jour en direct.
    /// On ne resynchronise PAS à l'abonnement : un score chargé (sauvegarde ou
    /// historique, où le joueur n'a pas ses cartes) doit être préservé tel quel.
    /// </summary>
    private void HookHand() => numberCards.CollectionChanged += OnHandChanged;

    private void OnHandChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        score = numberCards.Sum(c => c.Value);
        OnPropertyChanged(nameof(Score));
    }

    private Player()
    {
        name = "";
        numberCards = new();
        chanceCards = new();
        HookHand();
    }

    public Player(string name, bool? isAi)
    {
        this.name = name ?? "Joueur";
        this.isAi = isAi ?? false;
        this.score = 0;
        this.numberCards = new ObservableCollection<NumberCard>();
        this.chanceCards = new ObservableCollection<ChanceCard>();
        HookHand();
    }

    public void AddNumberCard(NumberCard card) => numberCards.Add(card);
    public void RemoveNumberCard(NumberCard card) => numberCards.Remove(card);
}