namespace CLI_UNO_Desktop
{
    public struct Card : IEquatable<Card>
    {
        public string Type { get; }
        public ConsoleColor Color { get; set; }

        public Card(ConsoleColor color, string type)
        {
            Color = color;
            Type = type;
        }

        public bool Equals(Card other)
        {
            throw new NotImplementedException();
        }
    }
}
