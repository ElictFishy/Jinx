using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace JinxApp.Models;

[DataContract(IsReference = true)]

/// <summary>
/// Classe de base implémentant INotifyPropertyChanged pour le binding MAUI.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifie la vue qu'une propriété a changé.
    /// </summary>
    /// <param name="name">Nom de la propriété (rempli automatiquement).</param>
    protected void OnPropertyChanged([CallerMemberName] string? name = "Joueur")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
