namespace UNO
{
    public static class Utilities
    {
        public static void WriteLineColor(object o, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(o);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteColor(object o, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.Write(o);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string? GetColor(ConsoleColor c)
        {
            return Enum.GetName(typeof(ConsoleColor), c);
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
    }
}
