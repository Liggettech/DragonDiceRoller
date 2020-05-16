using System.Collections.Generic;

namespace DragonDiceRoller
{
    class Spell
    {
        public string Name { get; set; }
        public string School { get; set; }
        public string Type { get; set; }
        public int Cost { get; set; }
        public string CastTime { get; set; }
        public int TargetNumber { get; set; }
        public string Test { get; set; }
        public string Prerequisite { get; set; }
        public string Description { get; set; }

        public Spell()
        {
            Name = "N/A";
            School = "N/A";
            Type = "N/A";
            Cost = 0;
            CastTime = "N/A";
            TargetNumber = 0;
            Test = "N/A";
            Prerequisite = "N/A";
            Description = "N/A";
        }

        public Spell(string sInName, string sInSchool, string sInType, int iInCost, string sInCastTime,
                     int iInTargetNumber, string sInTest, string sInPrereq = "N/A", string sInDescription = "N/A")
        {
            Name = sInName;
            School = sInSchool;
            Type = sInType;
            Cost = iInCost;
            CastTime = sInCastTime;
            TargetNumber = iInTargetNumber;
            Test = sInTest;
            Prerequisite = sInPrereq;
            Description = sInDescription.Replace("^", "\n");
        }

        public static List<string> GetFilterFields()
        {
            return new List<string>()
            {
                "name",
                "school",
                "type",
                "cost",
                "time",
                "target",
                "test",
                "prerequisite"                
            };
        }
    }
}
