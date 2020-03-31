using System;
using System.Collections.Generic;

namespace Trail365.Seeds
{
    public static class TextSeedingHelper
    {

        private static readonly string[] WordsForName = new string[] { "Ultra", "Trail", "Sky", "Marathon", "2050", "in", "goes", "on", "Buschberg", "25km", "the", "Classic", "Vienna", "lorem",
                                                                        "Ipsum", "dolor", "sit", "Omnis", "Natus" , "45 km", "quia", "consequuntur", "Magni", "Hochwechsel" ,"XXIV.", "a"};

        private static string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris rutrum arcu id lacus porttitor faucibus ut ut enim. Ut egestas consectetur rutrum. Cras euismod tellus ac tortor finibus, a facilisis eros ultrices. Maecenas tincidunt mauris a efficitur auctor. Nunc porttitor posuere orci eget semper. Etiam iaculis ligula a diam commodo.";

        private static readonly string[] WordsForExcerpt = LoremIpsum.Split(new string[] { " ", ".", "," }, StringSplitOptions.RemoveEmptyEntries);

        public static string GetExcerptDummy()
        {
            Random r = new Random();
            int words = r.Next(3, 33);
            int linebreak = r.Next(5, 9);
            List<string> wordList = new List<string>();
            while (wordList.Count < words)
            {

                if ((wordList.Count > 0) && (wordList.Count % linebreak) == 0)
                {
                    wordList.Add("." + Environment.NewLine+Environment.NewLine); //markdown, 2 newlines!
                    continue;
                }

                int wordIndex = r.Next(0, WordsForExcerpt.Length - 1);
                wordList.Add(WordsForExcerpt[wordIndex] + " ");

            }
            return string.Join(string.Empty, wordList).Trim();
        }
        public static string GetNameDummy()
        {
            Random r = new Random();
            int words = r.Next(2, 8);
            List<string> wordList = new List<string>();
            while (wordList.Count < words)
            {
                int wordIndex = r.Next(0, WordsForName.Length - 1);
                wordList.Add(WordsForName[wordIndex]);
            }
            return string.Join(" ", wordList);
        }

    }
}
