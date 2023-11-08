namespace CLI_UNO_Desktop
{
    internal class Game
    {
#pragma warning disable CS8618
        private static Deck _ActiveDeck;
#pragma warning restore CS8618
        private static int _NumberOfPlayers;
        private static int _CurrentTurn;
        private static Card _CurrentDiscard;
        private static int _NextPlayerPickup = 0;
        private static bool _ReversePlayDirection = false;
        private static readonly Random _Random = new();
        private static readonly List<List<Card>> _PlayerList = new();

        private static void Main()
        {
            Deck[] availableDecks = CreateDecks();

            while (true)
            {
                GameSetup(availableDecks);
                Play();

                Console.Clear();
                Console.WriteLine("\x1b[3J");
            }
        }

        #region // Setup Logic

        /// <summary>
        /// Contains the creation of game decks
        /// </summary>
        /// <returns>An array with each deck</returns>
        private static Deck[] CreateDecks()
        {
            ConsoleColor[] commonColors = new ConsoleColor[] { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Blue };

            Deck standard = new Deck("Standard",
                new HashSet<string>
                {
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "skip", "reverse", "draw +2", "wild", "wild +4", "wild shuffle"
                },
                new HashSet<string>
                { "wild", "wild +4", "wild shuffle" },
                commonColors, false, false);

            Deck competitive = new Deck("Competitive",
                new HashSet<string>
                {
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "reverse", "draw +1", "draw +2", "wild", "wild shuffle", "self draw +2", "self draw +3",
                },
                new HashSet<string>
                { "wild", "wild shuffle", "self draw +2", "self draw +3" },
                commonColors, false, false);

            Deck candy = new Deck("Candy",
                new HashSet<string>
                {
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "skip", "reverse", "draw +1", "draw +2", "wild", "wild +4", "candy"
                },
                new HashSet<string>
                { "wild", "wild +4", "candy" },
                new ConsoleColor[]
                {
                    ConsoleColor.Red, ConsoleColor.Yellow,
                    ConsoleColor.Green, ConsoleColor.Blue,
                    ConsoleColor.Cyan, ConsoleColor.Magenta
                }, false, false);

            Deck chaos = new Deck("Chaos",
                new HashSet<string>
                {
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "skip", "skip x2", "reverse", "draw +1", "draw +2", "draw +4", "draw +8", "draw +50", "wild", "wild +2",
                    "wild +4", "wild +8", "reverse wild", "skip wild", "skip wild x2", "target wild +2", "swap wild", "candy"
                },
                new HashSet<string>
                { "wild", "wild +2", "wild +4", "wild +8", "reverse wild", "skip wild", "skip wild x2", "target wild +2", "swap wild", "candy" },
                new ConsoleColor[]
                {
                    ConsoleColor.Red, ConsoleColor.Yellow,
                    ConsoleColor.Green, ConsoleColor.Blue,
                    ConsoleColor.Cyan, ConsoleColor.Magenta,
                    ConsoleColor.DarkRed, ConsoleColor.DarkYellow
                }, false, false);

            Deck allWild = new Deck("All Wild",
                new HashSet<string>
                {
                    "wild", "wild +2", "wild +4", "reverse wild", "skip wild", "skip wild x2", "target wild +2", "swap wild"
                },
                new HashSet<string> { "wild", "wild +2", "wild +4", "reverse wild", "skip wild", "skip wild x2", "target wild +2", "swap wild" },
                commonColors, true, true);

            Deck retro = new Deck("Retro",
                new HashSet<string>
                {
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "skip", "reverse", "draw +2", "wild", "wild +4", "wild shuffle"
                },
                new HashSet<string> { },
                new ConsoleColor[]
                { ConsoleColor.Green, ConsoleColor.DarkGreen }, false, false);

            return new Deck[] { standard, competitive, candy, chaos, allWild, retro };
        }

        /// <summary>
        /// Prompts the user to enter values for each game setting as well as setting up the players and discard pile
        /// </summary>
        /// <param name="decks">The available deck options</param>
        private static void GameSetup(Deck[] decks)
        {
            int deckChoice = -1;
            for (int i = 0; i < decks.Length; i++)
            {
                Console.WriteLine($"[{i}] {decks[i].Name}");
            }

            while (deckChoice < 0 || deckChoice >= decks.Length)
            {
                deckChoice = Prompt("Choose a deck: ", "That is not a choice!");
            }
            _ActiveDeck = decks[deckChoice];

            int startingHand = Prompt("# of cards: ", "That is not a number!");
            _NumberOfPlayers = Prompt("# of players: ", "That is not a number!");

            _CurrentTurn = _Random.Next(_NumberOfPlayers);

            _PlayerList.Clear();
            for (int i = 0; i < _NumberOfPlayers; i++)
            {
                _PlayerList.Add(new List<Card>(startingHand));
                for (int j = 0; j < startingHand; j++)
                {
                    _PlayerList[i].Add(_ActiveDeck.GetRandomCard());
                }
            }

            do
            {
                _CurrentDiscard = _ActiveDeck.GetRandomCard();
            }
            while (_ActiveDeck.CheckIfSpecial(_CurrentDiscard) && !_ActiveDeck.DisableSpecial);
        }

        #endregion

        #region // Game Logic

        /// <summary>
        /// The main code that controls each event that occurs during gameplay
        /// </summary>
        private static void Play()
        {
            while (true)
            {
                // Check each players hand for a win
                if (CheckForPlayerWins())
                {
                    return;
                }

                if (AdjustTurn())
                {
                    continue;
                }

                DrawTUI();

                if (_CurrentTurn == 0)
                {
                    PlayerTurn();
                }
                else
                {
                    BotTurn();
                }
            }
        }

        /// <summary>
        /// Checks if a player has 0 cards left and returns whether they won or not
        /// </summary>
        /// <returns>Whether a player won</returns>
        private static bool CheckForPlayerWins()
        {
            for (int i = 0; i < _PlayerList.Count; i++)
            {
                if (_PlayerList[i].Count <= 0)
                {
                    Console.Clear();
                    Console.WriteLine("\x1b[3J");
                    WriteLineColor($"\nPlayer {i} wins!", ConsoleColor.Cyan);
                    _ = Console.ReadKey(true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles the turn logic ie. incrementing, decrementing, skipping
        /// </summary>
        /// <returns>Whether a player has lost their turn</returns>
        private static bool AdjustTurn()
        {
            _CurrentTurn += _ReversePlayDirection ? -1 : 1;

            if (_CurrentTurn < 0)
            {
                _CurrentTurn = (_CurrentTurn + _NumberOfPlayers) % _NumberOfPlayers;
            }
            if (_CurrentTurn >= _NumberOfPlayers)
            {
                _CurrentTurn = (_CurrentTurn - _NumberOfPlayers) % _NumberOfPlayers;
            }

            if (_NextPlayerPickup > 0)
            {
                for (int i = 0; i < _NextPlayerPickup; i++)
                {
                    _PlayerList[_CurrentTurn].Add(_ActiveDeck.GetRandomCard());
                }
                _NextPlayerPickup = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws the TUI that displays the players, how many cards they have, the top discard, and the players hand
        /// </summary>
        private static void DrawTUI()
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            Console.Write("-----------------------" +
                          "\nDiscard Pile: ");
            WriteLineColor(_CurrentDiscard.Type, _CurrentDiscard.Color);
            char direction = _ReversePlayDirection ? '<' : '>';
            Console.WriteLine($"Current Direction: {direction}\n" +
                              $"-----------------------");

            // Prints out each player and how many cards they have left
            WriteLineColor("\nPlayer List:", ConsoleColor.Cyan);
            Console.WriteLine("-------------");
            for (int i = 0; i < _PlayerList.Count; i++)
            {
                Console.ForegroundColor = i == _CurrentTurn ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine($"Player {i} : Cards {_PlayerList[i].Count}");
            }

            // Prints out main players hand
            WriteLineColor("\nYour Cards:", ConsoleColor.Cyan);
            Console.WriteLine("------------");
            Console.WriteLine("[-1] Pick Up\n");
            for (int i = 0; i < _PlayerList[0].Count; i++)
            {
                Console.Write($"[{i}] ");
                WriteLineColor($"{_PlayerList[0][i].Type}", _PlayerList[0][i].Color);
            }
        }

        #endregion

        #region // Player Logic

        /// <summary>
        /// The code for player turns that lets the player select a card from their hand to use or pick up a card
        /// </summary>
        private static void PlayerTurn()
        {
            Card selectedCard;

            // Checks for errors while user picks their card
            while (true)
            {
                Console.Write("Pick a card: ");
                if (!TryGetValidUserChoice(out int choice))
                {
                    WriteLineColor("That is not a valid option!", ConsoleColor.Red);
                }

                if (choice == -1)
                {
                    _PlayerList[0].Add(_ActiveDeck.GetRandomCard());
                    return;
                }

                selectedCard = _PlayerList[0][choice];

                if (IsValidMove(selectedCard))
                {
                    break;
                }
                WriteLineColor("That is not a valid option!", ConsoleColor.Red);
            }

            PlayCard(selectedCard);
            Console.Clear();
        }

        /// <summary>
        /// Checks if the selected card index is a valid choice
        /// </summary>
        /// <param name="choice">The index selected</param>
        /// <returns>Whether the index is valid or not</returns>
        private static bool TryGetValidUserChoice(out int choice)
        {
            if (!int.TryParse(Console.ReadLine(), out choice) || choice < -1 || choice >= _PlayerList[0].Count)
            {
                choice = -1;
                return false;
            }
            return true;
        }

        #endregion

        #region // Bot Logic

        /// <summary>
        /// The AI behind the bots and how they pick their cards 
        /// </summary>
        private static void BotTurn()
        {
            Thread.Sleep(2500);

            ShuffleBotHand(_PlayerList[_CurrentTurn]);

            // Randomly selects a card from the bots hand
            foreach (Card card in _PlayerList[_CurrentTurn])
            {
                if (IsValidMove(card))
                {
                    PlayCard(card);
                    return;
                }
            }

            _PlayerList[_CurrentTurn].Add(_ActiveDeck.GetRandomCard());
            Console.Clear();
        }

        /// <summary>
        /// Shuffles the bots hand and picks a random card from it
        /// </summary>
        /// <param name="hand">The current bots hand</param>
        private static void ShuffleBotHand(List<Card> hand)
        {
            int handCount = hand.Count;
            while (handCount > 1)
            {
                handCount--;
                int pick = _Random.Next(handCount);
                (hand[handCount], hand[pick]) = (hand[pick], hand[handCount]);
            }
        }

        #endregion
        
        #region // Turn Logic

        /// <summary>
        /// Plays a selected card from a players hand and discards it
        /// </summary>
        /// <param name="selectedCard">The card that the player selected</param>
        private static void PlayCard(Card selectedCard)
        {
            _ = _PlayerList[_CurrentTurn].Remove(selectedCard);
            _CurrentDiscard = selectedCard;
            UseCard(selectedCard);
        }

        /// <summary>
        /// Checks whether a card is a valid choice
        /// </summary>
        /// <param name="card">The selected card</param>
        /// <returns>Whether the card is valid</returns>
        private static bool IsValidMove(Card card)
        {
            return card.Type == _CurrentDiscard.Type
                || card.Color == _CurrentDiscard.Color
                || _ActiveDeck.CheckIfSpecial(card);
        }

        #endregion

        #region // Card Logic

        /// <summary>
        /// Preforms a set of one or more actions based upon a given card
        /// </summary>
        /// <param name="card">The selected card</param>
        private static void UseCard(Card card)
        {
            int repeatValue = 1;

            if (card.Type.Contains('x'))
            {
                repeatValue = Convert.ToInt32(card.Type.Split('x')[1]);
            }

            for (int i = 0; i < repeatValue; i++)
            {
                if (card.Type.Contains('+'))
                {
                    HandleDrawingActions(card);
                }
                HandleRegularActions(card);
            }
        }

        /// <summary>
        /// Handles regular actions that don't require and drawing functionality
        /// </summary>
        /// <param name="card">The selected card</param>
        private static void HandleRegularActions(Card card)
        {
            if (card.Type.Contains("wild") && !_ActiveDeck.DisableWild)
            {
                ActionWild();
            }

            if (card.Type.Contains("reverse"))
            {
                _ReversePlayDirection = !_ReversePlayDirection;
            }

            if (card.Type.Contains("candy"))
            {
                _CurrentDiscard.Color = _ActiveDeck.GetRandomColor();
            }

            if (card.Type.Contains("shuffle"))
            {
                ActionShuffle();
            }

            if (card.Type.Contains("swap"))
            {
                ActionSwap();
            }

            if (card.Type.Contains("skip"))
            {
                _CurrentTurn += _ReversePlayDirection ? -1 : 1;
            }
        }

        /// <summary>
        /// Handles cards that require players to draw more cards
        /// </summary>
        /// <param name="card">The selected card</param>
        private static void HandleDrawingActions(Card card)
        {
            int drawValue = Convert.ToInt32(card.Type.Split('+')[1]);

            if (card.Type.Contains("self"))
            {
                ActionSelf(drawValue);
            }
            else if (card.Type.Contains("target"))
            {
                ActionTarget(drawValue);
            }
            else
            {
                _NextPlayerPickup += drawValue;
            }
        }

        /// <summary>
        /// All of the players have their cards shuffled and dispensed evenly
        /// </summary>
        private static void ActionShuffle()
        {
            int index = 0;
            List<Card> totalCards = new();

            foreach (List<Card> hand in _PlayerList)
            {
                totalCards.AddRange(hand);
                hand.Clear();
            }

            foreach (Card c in totalCards)
            {
                _PlayerList[index].Add(c);
                index++;
                if (index == _NumberOfPlayers)
                {
                    index = 0;
                }
            }
        }

        /// <summary>
        /// Swaps the current player hand with a target players and bots pick randomly
        /// </summary>
        private static void ActionSwap()
        {
            Card[] attackerCards;
            Card[] targetCards;
            int target;

            if (_CurrentTurn == 0)
            {
                target = GetTargetPlayer();
            }
            else
            {
                do
                {
                    target = _Random.Next(_NumberOfPlayers);
                }
                while (target == _CurrentTurn);
            }

            attackerCards = _PlayerList[_CurrentTurn].ToArray();
            targetCards = _PlayerList[target].ToArray();

            _PlayerList[_CurrentTurn].Clear();
            _PlayerList[target].Clear();

            _PlayerList[_CurrentTurn] = targetCards.ToList();
            _PlayerList[target] = attackerCards.ToList();
        }

        /// <summary>
        /// Adds cards to the current players hand and chooses a random color
        /// </summary>
        /// <param name="drawValue">The number of cards to add</param>
        private static void ActionSelf(int drawValue)
        {
            for (int i = 0; i < drawValue; i++)
            {
                _PlayerList[_CurrentTurn].Add(_ActiveDeck.GetRandomCard());
            }
            _CurrentDiscard.Color = _ActiveDeck.GetRandomColor();
        }

        /// <summary>
        /// Adds cards to another players hand and chooses a random color
        /// </summary>
        /// <param name="pickupAmount">The number of cards to add</param>
        private static void ActionTarget(int pickupAmount)
        {
            int target = GetTargetPlayer();
            for (int i = 0; i < pickupAmount; i++)
            {
                _PlayerList[target].Add(_ActiveDeck.GetRandomCard());
            }
            _CurrentDiscard.Color = _ActiveDeck.GetRandomColor();
        }

        /// <summary>
        /// Allows a player to change the color to something of their choosing and bots choose a random color
        /// </summary>
        private static void ActionWild()
        {
            if (_CurrentTurn == 0)
            {
                foreach (ConsoleColor color in _ActiveDeck.Colors)
                {
                    WriteLineColor($"> {Enum.GetName(typeof(ConsoleColor), color)}", color);
                }

                while (true)
                {
                    Console.Write("Choose Color: ");
                    if (Enum.TryParse(Console.ReadLine(), true, out ConsoleColor color))
                    {
                        _CurrentDiscard.Color = color;
                        break;
                    }
                    WriteLineColor("That is not a valid option!", ConsoleColor.Red);
                }
            }
            else
            {
                _CurrentDiscard.Color = _ActiveDeck.GetRandomColor();
            }
        }

        /// <summary>
        /// Lets a player choose another player as a target and bots pick a random player
        /// </summary>
        /// <returns>The target player</returns>
        private static int GetTargetPlayer()
        {
            int target;
            if (_CurrentTurn == 0)
            {
                for (int j = 1; j < _PlayerList.Count; j++)
                {
                    Console.WriteLine($"[{j}] Player {j}");
                }

                do
                {
                    target = Prompt("Player to target: ", "That is not a valid option!");
                }
                while (target == _CurrentTurn);
            }
            else
            {
                do
                {
                    target = _Random.Next(_NumberOfPlayers);
                }
                while (target == _CurrentTurn);
            }
            return target;
        }

        #endregion

        #region // Utilities

        public static void WriteLineColor(object o, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(o);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static int Prompt(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int outVar))
                {
                    return outVar;
                }
                WriteLineColor(errorMessage, ConsoleColor.Red);
            }
        }

        #endregion
    }
}