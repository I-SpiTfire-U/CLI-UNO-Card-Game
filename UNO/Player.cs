namespace UNO
{
    public class Player
    {
        public string Name { get; }
        public List<Card> Cards { get; set; }

        public Player(string name)
        {
            Name = name;
            Cards = new List<Card>();
        }
    }
}
