using System.Collections.Generic;

namespace DragonDiceRoller
{
    class Drink
    {       
        public string Name { get; set; }
        public string Difficulty { get; set; }
        public string Description { get; set; }

        public Drink()
        {
            Name = "N/A";
            Difficulty = "N/A";
            Description = "N/A";        
        }

        public Drink(string sInName, string sInDifficulty, string sInDescription = "N/A")
        {
            Name = sInName;
            Difficulty = sInDifficulty;
            Description = sInDescription.Replace("^", "\n");                   
        }

        public static List<string> GetProperties()
        {
            return new List<string>()
            {
                "name",
                "difficulty",
                "description"
            };            
        }
    }
}
