namespace CLI_UNO
{
    public struct Card
    {
        public string Type { get; }
        public ConsoleColor Color { get; set; }

        public Card(ConsoleColor color, string type)
        {
            Color = color;
            Type = type;
        }
    }
}
