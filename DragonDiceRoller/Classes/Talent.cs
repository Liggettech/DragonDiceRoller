using System.Collections.Generic;

namespace DragonDiceRoller
{
    class Talent
    {
        

        public string Name { get; set; }
        public string Classes { get; set; }
        public string Prerequisite { get; set; }
        public string Description { get; set; }
        public string Novice { get; set; }
        public string Journeyman { get; set; }
        public string Master { get; set; }

        public Talent()
        {
            Name = "N/A";
            Classes = "N/A";
            Prerequisite = "N/A";
            Description = "N/A";
            Novice = "N/A";
            Journeyman = "N/A";
            Master = "N/A";            
        }

        public Talent(string sInName, string sInClasses, string sInNovice, string sInJourneyman, string sInMaster, string sInPrereq = "N/A", string sInDescription = "N/A")
        {
            Name = sInName;
            Classes = sInClasses;
            Prerequisite = sInPrereq;
            Description = sInDescription.Replace("^", "\n");
            Novice = sInNovice;
            Journeyman = sInJourneyman;
            Master = sInMaster;                        
        }

        public static List<string> GetProperties()
        {
            return new List<string>()
            {
                "name",
                "classes",
                "prerequisite",
                "description",
                "novice",
                "journeyman",
                "master"
            };            
        }
    }
}
