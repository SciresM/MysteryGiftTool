using System;

namespace MysteryGiftTool
{
    internal class BossMetadata
    {
        public string Name;
        private string Type;
        private string ID;
        private string ContentSize;
        private string TimeStamp;

        public string ArchiveName => $"{Name}-_-{Type}-_-{ID}-_-{TimeStamp}";

        public string FileName => $"{Name}_{Type}_{TimeStamp}";

        public static BossMetadata FromString(string desc)
        {
            var spl = desc.Split('\t');
            if (spl.Length != 7)
                throw new ArgumentException($"Bad boss string ({spl.Length}, {string.Join(",", spl)}): {desc}");
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
            var spl = an.Split(new[] {"-_-"}, StringSplitOptions.None);
            if (spl.Length != 4)
                throw new ArgumentException($"Bad boss string ({spl.Length}, {string.Join(",", spl)}): {an}");
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
