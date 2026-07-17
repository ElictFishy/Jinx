using Microsoft.Maui.Controls;
using JinxApp.Models;
using JinxApp.Managers;

namespace JinxApp.Views
{
    public partial class GamePage : ContentPage
    {
        private bool hasPickedCard = false;
        private bool hasUsedChanceCard = false;   // une seule carte chance par tour de joueur
        private bool isAiPlaying = false;
        private bool randomEventHooked = false;

        // Cartes sélectionnées pour MULTI_COLOR (cartes du plateau)
        private readonly List<NumberCard> multiColorSelectedCards = new();

        public GamePage()
        {
            InitializeComponent();
            BindingContext = JinxData.g;
        }

        // =====================================================================
        // Responsive : mise à l'échelle selon la taille de la fenêtre
        // =====================================================================

        // Barres haut/bas, main et dé : mis à l'échelle selon la fenêtre.
        // Le plateau est géré séparément (OnBoardSizeChanged) pour qu'il tienne
        // toujours sans scroll.
        private static readonly (string Key, double Base)[] ScaledResources =
        {
            ("TopBarH", 52), ("LeaveW", 100), ("LeaveH", 44), ("TitleFont", 26), ("PillFont", 15),
            ("BottomBarH", 150), ("ChanceBtnW", 80), ("ChanceBtnH", 105),
            ("HandCardW", 80), ("HandCardH", 114), ("HandValTop", 11), ("HandValBig", 32),
            ("DiceBox", 44), ("DiceFont", 26),
        };

        // Taille de référence du design d'origine.
        private const double RefWidth = 1000.0;
        private const double RefHeight = 680.0;

        // Ratio portrait des cartes du plateau (90 x 126).
        private const double CardRatio = 126.0 / 90.0;

        // Marges fixes de la mise en page (cf. GamePage.xaml) :
        // Grid externe Padding="14,12", RowSpacing="8" (2 espaces entre 3 rangées),
        // cadre du plateau Padding="12,12".
        private const double OuterPadX = 14 * 2;   // gauche + droite
        private const double OuterPadY = 12 * 2;   // haut + bas
        private const double RowSpacingTotal = 8 * 2;
        private const double BoardPad = 12 * 2;     // padding du cadre du plateau

        private double _lastScale = -1;
        private double _lastBoardCardW = -1;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            ApplyResponsiveScale(width, height);
            EstimateBoardCards(width, height);
        }

        private void ApplyResponsiveScale(double width, double height)
        {
            if (width <= 0 || height <= 0) return;

            // Plus petit ratio largeur/hauteur, borné. On reste modéré pour que
            // les barres n'avalent pas la hauteur réservée au plateau.
            double scale = Math.Min(width / RefWidth, height / RefHeight);
            scale = Math.Clamp(scale, 0.75, 1.3);

            // Évite les recalculs inutiles lors de micro-redimensionnements.
            if (Math.Abs(scale - _lastScale) < 0.01) return;
            _lastScale = scale;

            foreach (var (key, baseValue) in ScaledResources)
                Resources[key] = baseValue * scale;
        }

        // Affinage : recalcule à partir de la taille RÉELLE du cadre du plateau.
        private void OnBoardSizeChanged(object? sender, EventArgs e)
        {
            if (sender is not Border board) return;
            ComputeBoardCards(board.Width - board.Padding.HorizontalThickness,
                              board.Height - board.Padding.VerticalThickness);
        }

        // Estimation à partir de la taille de la page. OnSizeAllocated se déclenche
        // toujours (y compris à la maximisation), contrairement au SizeChanged du
        // cadre du plateau qui n'est pas fiable sur WinUI après un redimensionnement.
        private void EstimateBoardCards(double width, double height)
        {
            if (width <= 0 || height <= 0) return;

            double scale = Math.Clamp(Math.Min(width / RefWidth, height / RefHeight), 0.75, 1.3);
            double topBar = 52 * scale;
            double bottomBar = 150 * scale;

            double innerH = height - OuterPadY - topBar - bottomBar - RowSpacingTotal - BoardPad;
            double innerW = width - OuterPadX - BoardPad;
            ComputeBoardCards(innerW, innerH);
        }

        // Calcule la taille des cartes pour que la grille 4x4 remplisse l'espace
        // disponible sans jamais déborder (donc sans scroll).
        private void ComputeBoardCards(double innerW, double innerH)
        {
            if (innerW <= 0 || innerH <= 0) return;

            const int n = Board.SIZE;            // 4 colonnes / 4 rangées

            // Espacement MINIMAL réservé pour le calcul de la taille des cartes.
            // Plus il est petit, plus les cartes peuvent être grandes.
            double minGapH = Math.Clamp(innerW / 130.0, 5, 14);
            double minGapV = Math.Clamp(innerH / 60.0, 8, 16);

            // Taille maximale d'une carte selon la largeur et la hauteur dispo.
            double widthLimit = (innerW - (n - 1) * minGapH) / n;
            double heightLimit = (innerH - (n - 1) * minGapV) / n;

            // Carte portrait : on prend la dimension la plus contraignante.
            // Le facteur 0.99 laisse une petite marge anti-débordement (no scroll).
            double cardW = Math.Min(widthLimit, heightLimit / CardRatio) * 0.99;
            cardW = Math.Max(cardW, 28);          // plancher de sécurité
            double cardH = cardW * CardRatio;

            // Étalement : on répartit TOUT l'espace restant entre les cartes pour
            // que la grille occupe la quasi-totalité de la zone du plateau.
            double spH = Math.Max(minGapH, (innerW * 0.99 - n * cardW) / (n - 1));
            double spV = Math.Max(minGapV, (innerH * 0.985 - n * cardH) / (n - 1));

            Resources["BoardSpacingH"] = spH;
            Resources["BoardSpacingV"] = spV;
            Resources["BoardCardW"] = cardW;
            Resources["BoardCardH"] = cardH;
            Resources["BoardValTop"] = Math.Max(8, cardW * 0.16);
            Resources["BoardValBig"] = Math.Max(18, cardW * 0.46);

            // On ne ré-applique que si la taille a sensiblement changé.
            if (Math.Abs(cardW - _lastBoardCardW) <= 1.0) return;
            _lastBoardCardW = cardW;

            // Taille EXACTE du contenu (cartes + espacements). En fixant la
            // CollectionView à cette taille, chaque cellule vaut exactement cardW
            // et l'étalement reste maîtrisé : la grille demeure centrée et homogène.
            double viewW = n * cardW + (n - 1) * spH;
            double viewH = n * cardH + (n - 1) * spV;
            ApplyBoardLayout(viewW, viewH);
        }

        private void ApplyBoardLayout(double viewW, double viewH)
        {
            if (BoardView == null) return;

            // Différé pour ne pas réentrer dans la passe de layout en cours.
            Dispatcher.Dispatch(() =>
            {
                BoardView.WidthRequest = viewW;
                BoardView.HeightRequest = viewH;

                // La CollectionView (panneau virtualisé) ne re-mesure pas toujours
                // ses items quand un DynamicResource change. On force la
                // reconstruction des cases pour appliquer les nouvelles tailles.
                var source = BoardView.ItemsSource;
                BoardView.ItemsSource = null;
                BoardView.ItemsSource = source ?? JinxData.g?.Board.GridAsList;
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (JinxData.g == null || JinxData.gm == null) return;

            // Abonnement unique à l'événement aléatoire (Orage / Tornade)
            if (!randomEventHooked)
            {
                JinxData.gm.RandomEventTriggered += OnRandomEventTriggered;
                randomEventHooked = true;
            }

            BindingContext = null;
            BindingContext = JinxData.g;
            hasUsedChanceCard = false;
            RefreshDice();
            RefreshChanceCards();

            if (JinxData.g.CurrentPlayer?.IsAi == true)
                await PlayAiTurnsAsync();
        }

        // =====================================================================
        // Cartes chance
        // =====================================================================

        private void RefreshChanceCards()
        {
            int count = JinxData.g?.CurrentPlayer?.ChanceCards?.Count ?? 0;
            ChanceCardCountLabel.Text = count == 1 ? "1 carte" : $"{count} cartes";
        }

        private void OnChanceCardToggleTapped(object? sender, TappedEventArgs e)
        {
            ChanceCardPanel.IsVisible = !ChanceCardPanel.IsVisible;
        }

        private void OnChanceCardPanelClose(object? sender, TappedEventArgs e)
        {
            ChanceCardPanel.IsVisible = false;
        }

        private async void OnDrawChanceCardSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (JinxData.g == null || JinxData.gm == null) return;
            if (isAiPlaying) return;

            var selected = e.CurrentSelection.FirstOrDefault();
            ((CollectionView)sender!).SelectedItem = null;

            if (selected is not NumberCard cardToGive) return;
            if (JinxData.g.CurrentPlayer == null) return;

            bool confirm = await DisplayAlertAsync(
                "Piocher une carte chance",
                $"Échanger votre carte {cardToGive.Value} ({cardToGive.Color}) contre une carte chance aléatoire ?",
                "Confirmer", "Annuler");

            if (!confirm) return;

            ChanceCard drawn = JinxData.gm.DrawChanceCard(JinxData.g.CurrentPlayer, cardToGive);
            RefreshChanceCards();

            await DisplayAlertAsync("✦ Carte chance obtenue !",
                $"Vous avez reçu : {drawn.Type}\n({drawn.UsageLabel})", "OK");
        }

        private async void OnChanceCardTapped(object? sender, TappedEventArgs e)
        {
            if (JinxData.g == null || JinxData.gm == null) return;
            if (isAiPlaying) return;

            if (e.Parameter is not ChanceCard card) return;
            if (JinxData.g.CurrentPlayer == null) return;

            // Une seule carte chance peut être utilisée par tour de joueur.
            if (hasUsedChanceCard)
            {
                await DisplayAlertAsync("Attention",
                    "Vous avez déjà utilisé une carte chance ce tour ! Attendez le prochain tour.", "OK");
                return;
            }

            Player player = JinxData.g.CurrentPlayer;

            switch (card.Type)
            {
                case ChanceCardType.INCREASE_DICE:
                    if (!JinxData.gm.HasRolled)
                    {
                        // Avant le lancer : effet mis en attente, appliqué au lancer du dé.
                        JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                        RefreshDice();
                        RefreshChanceCards();
                        await DisplayAlertAsync("✦ Carte chance", PendingDeltaMessage(), "OK");
                        break;
                    }
                    if (JinxData.g.Dice.Value >= 6)
                    {
                        await DisplayAlertAsync("Impossible", "Le dé est déjà à 6 !", "OK");
                        return;
                    }
                    JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                    RefreshDice();
                    RefreshChanceCards();
                    await DisplayAlertAsync("✦ Carte chance", $"Dé augmenté → {JinxData.g.Dice.Value}", "OK");
                    break;

                case ChanceCardType.DECREASE_DICE:
                    if (!JinxData.gm.HasRolled)
                    {
                        // Avant le lancer : effet mis en attente, appliqué au lancer du dé.
                        JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                        RefreshDice();
                        RefreshChanceCards();
                        await DisplayAlertAsync("✦ Carte chance", PendingDeltaMessage(), "OK");
                        break;
                    }
                    if (JinxData.g.Dice.Value <= 1)
                    {
                        await DisplayAlertAsync("Impossible", "Le dé est déjà à 1 !", "OK");
                        return;
                    }
                    JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                    RefreshDice();
                    RefreshChanceCards();
                    await DisplayAlertAsync("✦ Carte chance", $"Dé réduit → {JinxData.g.Dice.Value}", "OK");
                    break;

                case ChanceCardType.REROLL_DICE:
                    if (!JinxData.gm.HasRolled)
                    {
                        await DisplayAlertAsync("Attention", "Lancez d'abord le dé !", "OK");
                        return;
                    }
                    JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                    RefreshDice();
                    RefreshChanceCards();
                    await DisplayAlertAsync("✦ Carte chance", $"Dé relancé → {JinxData.g.Dice.Value}", "OK");
                    break;

                case ChanceCardType.MULTI_COLOR:
                    if (!JinxData.gm.HasRolled)
                    {
                        await DisplayAlertAsync("Attention", "Lancez d'abord le dé !", "OK");
                        return;
                    }
                    if (hasPickedCard)
                    {
                        await DisplayAlertAsync("Attention", "Vous avez déjà pris une carte ce tour !", "OK");
                        return;
                    }
                    JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                    RefreshChanceCards();
                    OpenMultiColorPanel();
                    break;

                case ChanceCardType.TAKE_LOW_CARD:
                    if (hasPickedCard)
                    {
                        await DisplayAlertAsync("Attention", "Vous avez déjà pris une carte ce tour !", "OK");
                        return;
                    }
                    JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                    RefreshChanceCards();
                    await DisplayAlertAsync("✦ Carte chance", "Prenez une carte de valeur 1 à 3 sur le plateau !", "OK");
                    break;

                case ChanceCardType.TAKE_HIGH_CARD:
                    if (hasPickedCard)
                    {
                        await DisplayAlertAsync("Attention", "Vous avez déjà pris une carte ce tour !", "OK");
                        return;
                    }
                    JinxData.gm.UseChanceCard(card, player);
                    hasUsedChanceCard = true;
                    RefreshChanceCards();
                    await DisplayAlertAsync("✦ Carte chance", "Prenez une carte de valeur 4 à 6 sur le plateau !", "OK");
                    break;
            }
        }
        
        /// <summary>
        /// Vérifie si le plateau contient au moins une carte active pour la valeur donnée.
        /// </summary>
        private bool BoardHasCardForValue(int value)
        {
            if (JinxData.g == null) return false;
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                {
                    NumberCard? c = JinxData.g.Board.Grid[row, col];
                    if (c != null && c.IsActive && c.Value == value)
                        return true;
                }
            return false;
        }

        /// <summary>
        /// Retourne toutes les positions (row, col) des cartes actives correspondant à la valeur.
        /// </summary>
        private List<(int row, int col)> GetMatchingCards(int value)
        {
            var list = new List<(int, int)>();
            if (JinxData.g == null) return list;
            for (int row = 0; row < Board.SIZE; row++)
                for (int col = 0; col < Board.SIZE; col++)
                {
                    NumberCard? c = JinxData.g.Board.Grid[row, col];
                    if (c != null && c.IsActive && c.Value == value)
                        list.Add((row, col));
                }
            return list;
        }

        // =====================================================================
        // MULTI_COLOR panel
        // =====================================================================

        private void OpenMultiColorPanel()
        {
            if (JinxData.g == null) return;

            multiColorSelectedCards.Clear();
            var boardCards = new List<NumberCard>();
            for (int r = 0; r < Board.SIZE; r++)
                for (int c = 0; c < Board.SIZE; c++)
                {
                    NumberCard? card = JinxData.g.Board.Grid[r, c];
                    if (card != null && card.IsActive)
                        boardCards.Add(card);
                }

            MultiColorSelection.ItemsSource = boardCards;
            MultiColorSelection.SelectedItems?.Clear();
            UpdateMultiColorSumLabel();
            MultiColorBanner.IsVisible = true;
        }

        private void OnMultiColorSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            multiColorSelectedCards.Clear();
            foreach (var item in MultiColorSelection.SelectedItems ?? new List<object>())
                if (item is NumberCard nc)
                    multiColorSelectedCards.Add(nc);
            UpdateMultiColorSumLabel();
        }

        private void UpdateMultiColorSumLabel()
        {
            int sum = multiColorSelectedCards.Sum(c => c.Value);
            int diceVal = JinxData.g?.Dice.Value ?? 0;
            MultiColorSumLabel.Text = $"Somme : {sum} / dé : {diceVal}";

            bool valid = JinxData.gm?.IsValidMultiColorSelection(multiColorSelectedCards) ?? false;
            MultiColorConfirmBtn.IsEnabled = valid;
            MultiColorSumLabel.TextColor = valid ? Color.FromArgb("#7CFC00") : Colors.White;
        }

        private async void OnMultiColorConfirm(object? sender, EventArgs e)
        {
            if (JinxData.g == null || JinxData.gm == null) return;
            if (JinxData.g.CurrentPlayer == null) return;

            if (!JinxData.gm.IsValidMultiColorSelection(multiColorSelectedCards))
            {
                await DisplayAlertAsync("Invalide", "La sélection n'est pas valide !", "OK");
                return;
            }

            JinxData.gm.ApplyMultiColorSelection(multiColorSelectedCards, JinxData.g.CurrentPlayer);

            MultiColorBanner.IsVisible = false;
            hasPickedCard = true;

            RefreshPlayerCards();
            RefreshDice();

            JinxData.gm.EndTurn();
            hasPickedCard = false;
            hasUsedChanceCard = false;
            ChanceCardPanel.IsVisible = false;
            JinxData.g.RefreshCurrentPlayer();
            RefreshPlayerCards();
            RefreshChanceCards();
            RefreshDice();

            if (JinxData.g.CurrentPlayer?.IsAi == true)
                _ = PlayAiTurnsAsync();
        }

        private void OnMultiColorCancel(object? sender, EventArgs e)
        {
            MultiColorBanner.IsVisible = false;
            multiColorSelectedCards.Clear();
        }

        // =====================================================================
        // Sélection des cartes du plateau
        // =====================================================================

        private void OnCardSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (JinxData.g == null || JinxData.gm == null) return;

            var selected = e.CurrentSelection.FirstOrDefault();
            if (selected == null) { ((CollectionView)sender!).SelectedItem = null; return; }

            // Le plateau expose des BoardCell (wrappers stables autour de NumberCard)
            if (selected is not BoardCell cell || cell.Card == null) { ((CollectionView)sender!).SelectedItem = null; return; }
            NumberCard card = cell.Card;

            if (isAiPlaying) { ((CollectionView)sender!).SelectedItem = null; return; }

            if (!JinxData.gm.HasRolled)
            {
                _ = DisplayAlertAsync("Attention", "Lancez d'abord le dé !", "OK");
            }
            else if (hasPickedCard)
            {
                _ = DisplayAlertAsync("Attention", "Vous avez déjà pris une carte ce tour !", "OK");
            }
            else
            {
                var pos = JinxData.g.Board.FindCardPosition(card.GetId());
                if (pos != null && JinxData.gm.CanPickCard(pos.Value.row, pos.Value.col, JinxData.g.Board, JinxData.g.Dice.Value))
                {
                    hasPickedCard = true;
                    JinxData.gm.PickCard(pos.Value.row, pos.Value.col);
                    JinxData.gm.EndTurn();
                    hasPickedCard = false;
                    hasUsedChanceCard = false;

                    ChanceCardPanel.IsVisible = false;
                    JinxData.g.RefreshCurrentPlayer();
                    RefreshPlayerCards();
                    RefreshChanceCards();
                    RefreshDice();

                    _ = PlayAiTurnsAsync();
                }
                else
                {
                    _ = DisplayAlertAsync("Invalide", "Vous ne pouvez pas prendre cette carte !", "OK");
                }
            }

            ((CollectionView)sender!).SelectedItem = null;
        }

        private void OnPlayerCardSelected(object? sender, SelectionChangedEventArgs e)
        {
            ((CollectionView?)sender)!.SelectedItem = null;
        }

        // =====================================================================
        // Tour IA
        // =====================================================================

        private async Task PlayAiTurnsAsync()
        {
            if (JinxData.g == null || JinxData.gm == null) return;

            while (JinxData.g.CurrentPlayer?.IsAi == true)
            {
                isAiPlaying = true;

                Player player = JinxData.g.CurrentPlayer;
                ShowAiBanner(true, player.Name);

                await ShowAiStatus("🤔 Réfléchit...");
                await Task.Delay(1000);

                GameManager.AiTurnResult result = JinxData.gm.PlayAiTurn(player);

                // ── Échange d'une carte numérotée contre une carte chance ────
                if (result.DrewChanceCard)
                {
                    await ShowAiStatus(result.DrawMessage ?? "🔄 Échange une carte contre une carte chance");
                    RefreshPlayerCards();
                    RefreshChanceCards();
                    await Task.Delay(2200);
                }

                // ── Carte PRE-DÉ (TAKE_HIGH / TAKE_LOW) ──────────────────────
                if (result.PreRollCard != null)
                {
                    await ShowAiStatus($"🃏 {result.PreRollMessage}");
                    RefreshChanceCards();
                    await Task.Delay(2000);

                    await ShowAiStatus("✅ Carte prise via carte chance !");
                    await Task.Delay(1400);

                    JinxData.gm.EndTurn();
                    JinxData.g.RefreshCurrentPlayer();
                    RefreshPlayerCards();
                    RefreshChanceCards();
                    RefreshDice();
                    ShowAiBanner(false);
                    isAiPlaying = false;
                    await Task.Delay(400);
                    continue;
                }

                // ── Lancer du dé ─────────────────────────────────────────────
                // On affiche la valeur RÉELLEMENT obtenue au premier lancer
                // (et non la valeur finale après relance), pour que le bandeau
                // soit cohérent avec ce qui se passe.
                await ShowAiStatus("🎲 Lance le dé...");
                await Task.Delay(1300);

                JinxData.g.Dice.Value = result.RawDiceValue;
                RefreshDice();
                await ShowAiStatus($"🎲 Dé : {result.RawDiceValue}");
                await Task.Delay(1500);

                // ── Carte POST-DÉ (INCREASE / DECREASE / REROLL) ─────────────
                // La carte chance a modifié le dé : on affiche la valeur ajustée.
                if (result.PostRollCard != null)
                {
                    await ShowAiStatus($"🃏 {result.PostRollMessage}");
                    JinxData.g.Dice.Value = result.DiceBeforeReroll;
                    RefreshDice();
                    RefreshChanceCards();
                    await Task.Delay(1800);
                }

                // ── Relance automatique ───────────────────────────────────────
                // Aucune carte ne correspondait à la valeur AVANT relance
                // (result.DiceBeforeReroll) → on relance vers la valeur finale.
                if (result.WasRerolled)
                {
                    await ShowAiStatus($"❌ Aucune carte pour le dé {result.DiceBeforeReroll}\n🎲 Relance automatique...");
                    await Task.Delay(1500);
                    JinxData.g.Dice.Value = result.DiceValue;
                    RefreshDice();
                    await ShowAiStatus($"🎲 Relance : {result.DiceValue}");
                    await Task.Delay(1500);
                }

                // ── Prise de carte normale ────────────────────────────────────
                if (result.PickedCard && result.PickedRow.HasValue && result.PickedCol.HasValue)
                {
                    await ShowAiStatus($"✅ Prend la carte {result.DiceValue}...");
                    await Task.Delay(1300);

                    JinxData.gm.PickCard(result.PickedRow.Value, result.PickedCol.Value);

                    await ShowAiStatus($"✅ Carte {result.DiceValue} prise !");
                    await Task.Delay(1300);
                }
                else
                {
                    await ShowAiStatus($"❌ Aucune carte disponible\n😔 Fin de tour sans carte");
                    await Task.Delay(2000);

                    RoundOutcome outcome = await CheckAndHandleRoundOverAsync();

                    // Partie terminée : on a déjà navigué vers l'accueil.
                    if (outcome == RoundOutcome.GameOver)
                    {
                        ShowAiBanner(false);
                        isAiPlaying = false;
                        return;
                    }
                    // Manche terminée : EndRound() ne change PAS le joueur courant.
                    // On NE rend PAS la main au joueur : on relance la boucle pour que
                    // l'IA (toujours joueur courant) enchaîne sur la nouvelle manche.
                    if (outcome == RoundOutcome.RoundOver)
                    {
                        ShowAiBanner(false);
                        isAiPlaying = false;
                        await Task.Delay(400);
                        continue;
                    }
                }

                JinxData.gm.EndTurn();
                ChanceCardPanel.IsVisible = false;
                JinxData.g.RefreshCurrentPlayer();
                RefreshPlayerCards();
                RefreshChanceCards();
                RefreshDice();

                ShowAiBanner(false);
                isAiPlaying = false;
                await Task.Delay(400);
            }
        }

        // =====================================================================
        // Dé
        // =====================================================================

        private async void OnDiceTapped(object? sender, TappedEventArgs e)
        {
            if (JinxData.g == null || JinxData.gm == null) return;
            if (isAiPlaying) return;

            try
            {
                if (hasPickedCard)
                {
                    await DisplayAlertAsync("Impossible", "Vous avez déjà pris une carte, attendez le prochain tour !", "OK");
                    return;
                }

                if (!JinxData.gm.HasRolled)
                {
                    JinxData.g.Dice.Value = JinxData.gm.RollDice();
                    RefreshDice();
                }
                else if (!JinxData.gm.HasRerolled)
                {
                    JinxData.g.Dice.Value = JinxData.gm.ReRollDice();
                    RefreshDice();

                    if (JinxData.gm.IsRoundOver())
                    {
                        await DisplayAlertAsync("Fin de manche",
                            $"Aucune carte disponible pour le chiffre {JinxData.g.Dice.Value} !", "OK");

                        JinxData.gm.EndRound();
                        await ShowRoundLossesAsync();

                        if (JinxData.gm.IsGameOver())
                        {
                            await ShowGameOverRecapAsync();
                            SaveGameToHistory();
                            SaveHighScores();
                            await EndGameAndNavigateAsync();
                            return;
                        }

                        BindingContext = null;
                        BindingContext = JinxData.g;
                        hasUsedChanceCard = false;
                        RefreshDice();
                        RefreshChanceCards();

                        if (JinxData.g.CurrentPlayer?.IsAi == true)
                            await PlayAiTurnsAsync();
                    }
                }
                else
                {
                    await DisplayAlertAsync("Impossible", "Vous avez déjà relancé le dé ce tour !", "OK");
                }
            }
            catch (InvalidOperationException ex)
            {
                await DisplayAlertAsync("Impossible", ex.Message, "OK");
            }
        }

        // =====================================================================
        // Fin de manche
        // =====================================================================

        private enum RoundOutcome { NotOver, RoundOver, GameOver }

        private async Task<RoundOutcome> CheckAndHandleRoundOverAsync()
        {
            if (JinxData.g == null || JinxData.gm == null) return RoundOutcome.NotOver;
            if (!JinxData.gm.IsRoundOver()) return RoundOutcome.NotOver;

            await DisplayAlertAsync("Fin de manche",
                $"Aucune carte disponible pour le chiffre {JinxData.g.Dice.Value} !", "OK");

            JinxData.gm.EndRound();
            await ShowRoundLossesAsync();

            if (JinxData.gm.IsGameOver())
            {
                await ShowGameOverRecapAsync();
                SaveGameToHistory();
                SaveHighScores();
                await EndGameAndNavigateAsync();
                return RoundOutcome.GameOver;
            }

            BindingContext = null;
            BindingContext = JinxData.g;
            RefreshDice();
            RefreshChanceCards();
            return RoundOutcome.RoundOver;
        }

        /// <summary>
        /// Enregistre la partie terminée dans l'historique persistant.
        /// On ne conserve qu'un instantané léger : nom, type (IA) et score final
        /// de chaque joueur. La partie la plus récente est placée en tête.
        /// </summary>
        /// <summary>
        /// Nettoyage de fin de partie : supprime la sauvegarde de la partie courante
        /// (sauf en démonstration, où rien n'a été sauvegardé) puis revient à la page
        /// appropriée — la page des règles pour une démonstration, l'accueil sinon.
        /// </summary>
        private async Task EndGameAndNavigateAsync()
        {
            string target = JinxData.IsDemo ? nameof(RulesPage) : nameof(HomePage);

            if (!JinxData.IsDemo)
                JinxData.dataService?.DeleteCurrentGame();

            JinxData.IsDemo = false;
            await Shell.Current.GoToAsync($"///{target}");
        }

        private void SaveGameToHistory()
        {
            // Partie de démonstration : on n'écrit jamais dans l'historique.
            if (JinxData.IsDemo) return;
            if (JinxData.g == null || JinxData.dataService == null) return;

            List<Player> snapshot = JinxData.g.Players
                .Select(p => new Player(p.Name, p.IsAi) { Score = p.Score })
                .ToList();

            List<GameHistory> history = JinxData.dataService.LoadHistory();
            history.Insert(0, new GameHistory(DateTime.Now, snapshot));
            JinxData.dataService.SaveHistory(history);
        }

        /// <summary>
        /// Met à jour le classement des meilleurs scores avec les scores finaux des
        /// joueurs de la partie terminée, et ne conserve que les 5 meilleurs.
        /// </summary>
        private void SaveHighScores()
        {
            // Partie de démonstration : on ne met jamais à jour le classement.
            if (JinxData.IsDemo) return;
            if (JinxData.g == null || JinxData.dataService == null) return;

            List<ScoreEntry> scores = JinxData.dataService.LoadHighScores();
            foreach (Player p in JinxData.g.Players)
                scores.Add(new ScoreEntry(p.Name, p.Score, p.IsAi));

            List<ScoreEntry> top = scores
                .OrderByDescending(s => s.Score)
                .Take(5)
                .ToList();

            JinxData.dataService.SaveHighScores(top);
        }

        /// <summary>
        /// Affiche, en fin de partie, le classement complet : tous les joueurs
        /// triés par score décroissant avec leurs points (et le marqueur IA).
        /// </summary>
        private async Task ShowGameOverRecapAsync()
        {
            if (JinxData.g == null) return;

            List<Player> ranked = JinxData.g.Players
                .OrderByDescending(p => p.Score)
                .ToList();

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < ranked.Count; i++)
            {
                Player p = ranked[i];
                string name = p.IsAi ? $"{p.Name} (IA)" : p.Name;
                sb.AppendLine($"{i + 1}. {name} — {p.Score} pts");
            }

            await DisplayAlert("🏆 Fin de partie — classement", sb.ToString().TrimEnd(), "OK");
        }

        /// <summary>
        /// Annonce, en fin de manche, les cartes que chaque joueur a perdues à cause
        /// de la pénalité de couleur (couleur encore présente sur le plateau).
        /// </summary>
        private async Task ShowRoundLossesAsync()
        {
            if (JinxData.gm == null) return;

            var sb = new System.Text.StringBuilder();
            foreach (GameManager.RoundLoss loss in JinxData.gm.LastRoundLosses)
            {
                string name = loss.Player.IsAi ? $"{loss.Player.Name} (IA)" : loss.Player.Name;
                if (loss.LostCards.Count == 0)
                    sb.AppendLine($"{name} : aucune carte perdue");
                else
                {
                    string cards = string.Join(", ",
                        loss.LostCards.Select(c => $"{c.Value} ({ColorFr(c.Color)})"));
                    sb.AppendLine($"{name} : {cards}");
                }
            }

            await DisplayAlert("🃏 Cartes perdues (couleur encore sur le plateau)",
                sb.ToString().TrimEnd(), "OK");
        }

        /// <summary>Nom français d'une couleur de carte, pour l'affichage.</summary>
        private static string ColorFr(CardColor color) => color switch
        {
            CardColor.RED => "Rouge",
            CardColor.BLUE => "Bleu",
            CardColor.GREEN => "Vert",
            CardColor.YELLOW => "Jaune",
            CardColor.PURPLE => "Violet",
            CardColor.ORANGE => "Orange",
            CardColor.PINK => "Rose",
            CardColor.BLACK => "Noir",
            _ => color.ToString()
        };

        // =====================================================================
        // Helpers UI
        // =====================================================================

        private void RefreshPlayerCards()
        {
            PlayerCard.ItemsSource = null;
            PlayerCard.ItemsSource = JinxData.g?.CurrentPlayer?.NumberCards;
        }

        private string PendingDeltaMessage()
        {
            int d = JinxData.gm?.PendingDiceDelta ?? 0;
            if (d == 0)
                return "Les effets s'annulent : le dé ne sera pas modifié au lancer.";
            string sign = d > 0 ? $"+{d}" : d.ToString();
            return $"Modificateur en attente : {sign}. Il s'appliquera au lancer du dé.";
        }

        private void RefreshDice()
        {
            if (JinxData.g == null || JinxData.gm == null) return;

            if (!JinxData.gm.HasRolled)
            {
                LabelDiceValue.Text = "?";
                int d = JinxData.gm.PendingDiceDelta;
                LabelRollNumber.Text = d != 0
                    ? $"Appuyez pour lancer ({(d > 0 ? "+" : "")}{d})"
                    : "Appuyez pour lancer";
            }
            else if (!JinxData.gm.HasRerolled)
            {
                LabelDiceValue.Text = JinxData.g.Dice.Value.ToString();
                LabelRollNumber.Text = "Lancer 1";
            }
            else
            {
                LabelDiceValue.Text = JinxData.g.Dice.Value.ToString();
                LabelRollNumber.Text = "Lancer 2";
            }
        }

        private void ShowAiBanner(bool visible, string? aiName = null)
            => MainThread.BeginInvokeOnMainThread(() =>
            {
                AiBanner.IsVisible = visible;
                if (visible && !string.IsNullOrWhiteSpace(aiName))
                    AiTitleLabel.Text = $"🤖 Tour de l'IA : {aiName}";
            });

        private Task ShowAiStatus(string message)
        {
            var tcs = new TaskCompletionSource();
            MainThread.BeginInvokeOnMainThread(() => { AiStatusLabel.Text = message; tcs.SetResult(); });
            return tcs.Task;
        }

        // =====================================================================
        // Événement aléatoire (Orage / Tornade)
        // =====================================================================

        private void OnRandomEventTriggered(object? sender, string message)
        {
            // Le plateau se met à jour automatiquement via le binding (BoardCell).
            // On annonce simplement l'événement au joueur.
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlertAsync("⚡ Événement aléatoire", message, "OK"));
        }

        private async void OnLeaveClicked(object? sender, EventArgs e)
        {
            await LeaveBtn.ScaleToAsync(0.8, 100);
            await LeaveBtn.ScaleToAsync(1.0, 100);

            // Démonstration : on ne sauvegarde pas, et on revient aux règles.
            if (JinxData.IsDemo)
            {
                JinxData.IsDemo = false;
                await Shell.Current.GoToAsync($"///{nameof(RulesPage)}");
                return;
            }

            if (JinxData.g != null)
                JinxData.dataService?.SaveCurrentGame(JinxData.g);

            await Shell.Current.GoToAsync($"///{nameof(HomePage)}");
        }
    }
}