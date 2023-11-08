namespace CLI_UNO_Desktop
{
    public class Deck
    {
        public bool DisableWild { get; }
        public bool DisableSpecial { get; }
        public string Name { get; }
        public ConsoleColor[] Colors { get; }
        public HashSet<string> Cards { get; }
        public HashSet<string> SpecialCards { get; }

        private readonly Random _Random = new();

        public Deck(string name, HashSet<string> cards, HashSet<string> specialCards, ConsoleColor[] colors, bool disableWild, bool disableSpecial)
        {
            Name = name;
            Cards = cards;
            SpecialCards = specialCards;
            Colors = colors;
            DisableWild = disableWild;
            DisableSpecial = disableSpecial;
        }

        public bool CheckIfSpecial(Card card)
        {
            return SpecialCards.Contains(card.Type);
        }

        public ConsoleColor GetRandomColor()
        {
            return Colors[_Random.Next(Colors.Length)];
        }

        public Card GetRandomCard()
        {
            string cardType = Cards.ElementAt(_Random.Next(0, Cards.Count));
            ConsoleColor cardColor = SpecialCards.Contains(cardType) ? ConsoleColor.White : GetRandomColor();

            return new Card(cardColor, cardType);
        }
    }
}