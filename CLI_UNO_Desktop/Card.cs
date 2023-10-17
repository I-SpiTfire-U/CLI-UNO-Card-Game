namespace CLI_UNO_Desktop
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

        public override string ToString()
        {
            return $"{Color} {Type}";
        }
    }
}
