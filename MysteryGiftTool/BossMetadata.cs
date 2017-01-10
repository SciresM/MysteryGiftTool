using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryGiftTool
{
    class BossMetadata
    {
        public string Name;
        public string Type;
        public string ID;
        public string ContentSize;
        public string TimeStamp;

        public string ArchiveName => $"{Name}-_-{Type}-_-{ID}-_-{TimeStamp}";

        public string FileName => $"{Name}_{Type}_{TimeStamp}";

        public static BossMetadata FromString(string desc)
        {
            var spl = desc.Split(new[]{ '\t' });
            if (spl.Count() != 7)
                throw new ArgumentException($"Bad boss string ({spl.Count()}, {string.Join(",", spl)}): {desc}");
            return new BossMetadata
            {
                Name = spl[0],
                Type = spl[2],
                ID = spl[3],
                ContentSize = spl[5],
                TimeStamp = spl[6]
            };
        }

        public static BossMetadata FromArchiveName(string an)
        {
            var spl = an.Split(new string[] {"-_-"}, StringSplitOptions.None);
            if (spl.Count() != 4)
                throw new ArgumentException($"Bad boss string ({spl.Count()}, {string.Join(",", spl)}): {an}");
            return new BossMetadata
            {
                Name = spl[0],
                Type = spl[1],
                ID = spl[2],
                TimeStamp = spl[3]
            };
        }

        public bool IsUpdatedVersionOf(BossMetadata other)
        {
            var upd = true;
            upd &= Name == other.Name;
            upd &= Type == other.Type;
            upd &= ID == other.ID;
            upd &= long.Parse(TimeStamp) > long.Parse(other.TimeStamp);
            return upd;
        }

    }
}
