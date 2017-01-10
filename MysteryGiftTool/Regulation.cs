using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PKHeX.Core;

namespace MysteryGiftTool
{
    internal class RegulationArchive
    {
        private readonly List<Regulation> Regulations;
        private readonly string Name;

        public RegulationArchive(byte[] archive, string name)
        {
            Name = name;
            Regulations = new List<Regulation>();
            var num_archives = BitConverter.ToUInt32(archive, 4);
            for (var i = 0; i < num_archives; i++)
            {
                var ofs = BitConverter.ToUInt32(archive, 0x10 + 8 * i);
                var len = BitConverter.ToUInt32(archive, 0x14 + 8 * i);
                if (len == 0x4A8)
                {
                    var reg = new byte[len];
                    Array.Copy(archive, ofs, reg, 0, len);
                    Regulations.Add(new Regulation(reg));
                }
                else
                {
                    Program.Log($"Invalid regulation found in archive! len = {len.ToString("X")}");
                }
            }
        }

        public void Save(string dir)
        {
            var bin_dir = Path.Combine(dir, "bin");
            var txt_dir = Path.Combine(dir, "txt");
            Program.CreateDirectoryIfNull(bin_dir);
            Program.CreateDirectoryIfNull(txt_dir);
            for (var i = 0; i < Regulations.Count; i++)
            {
                Program.Log($"Regulation {i+1}/{Regulations.Count}:");
                var summary = Regulations[i].ToString();
                Program.Log(summary);
                File.WriteAllBytes(Path.Combine(bin_dir, $"{Name}_{i+1}.bin"), Regulations[i].Data);
                File.WriteAllText(Path.Combine(txt_dir, $"{Name}_{i+1}.txt"), summary);
                Program.Log(string.Empty);
            }
        }
    }

    internal class Regulation
    {
        public readonly byte[] Data;

        private string[] BattleTypes ={"Singles", "Doubles", "[Type 3 - Battle Royale?]", "[Type 4]"};
        private string[] LevelTypes = {"Normal", "Minimum", "Maximum", "Scale Down", "Set", "Scale Up"};

        public string Title => Util.TrimFromZero(Encoding.Unicode.GetString(Data, 0x3FC, 0x4A));
        public string Subtitle => Util.TrimFromZero(Encoding.Unicode.GetString(Data, 0x446, 0x4A));

        public string BattleType => BattleTypes[Data[5]];
        public int MinAllowed => Data[6];
        public int MaxAllowed => Data[7];
        public int MinUsable => Data[8];
        public int MaxUsable => Data[9];

        public int LegendariesAllowed => Data[0xA];

        public int LevelCap => Data[0xB];
        public string LevelStyle => LevelTypes[Data[0xC]];
        public bool SpeciesClause => Data[0xE] == 0;
        public bool ItemClause => Data[0xF] == 0;



        public Regulation(byte[] d)
        {
            Data = (byte[])d.Clone();
        }

        public override string ToString()
        {
            if (GameInfo.Strings == null)
                GameInfo.Strings = GameInfo.getStrings("en");
            var sb = new StringBuilder();
            sb.AppendLine("Regulation: ");
            sb.AppendLine(Title);
            sb.AppendLine(Subtitle);

            sb.AppendLine($"Format: {BattleType}, bring {MinAllowed}-{MaxAllowed}, use {MinUsable}-{MaxUsable}.");
            sb.AppendLine($"Level cap: {LevelCap}");
            sb.AppendLine($"Level Cap Scaling Style: {LevelStyle}");
            sb.AppendLine($"Number of Legendaries allowed: {LegendariesAllowed}");

            sb.AppendLine($"Species Clause: {SpeciesClause}");
            sb.AppendLine($"Item Clause: {ItemClause}");

            sb.AppendLine();
            sb.AppendLine("=====");
            sb.AppendLine("Allowed Pokemon");
            sb.AppendLine("=====");
            for (var i = 0; i < GameInfo.Strings.specieslist.Length; i++)
            {
                var ofs = i/8 + 0x7C;
                if (((Data[ofs] >> (i%8)) & 1) != 1)
                    sb.AppendLine(GameInfo.Strings.specieslist[i]);
            }

            sb.AppendLine();
            sb.AppendLine("=====");
            sb.AppendLine("Banned Pokemon");
            sb.AppendLine("=====");
            for (var i = 0; i < GameInfo.Strings.specieslist.Length; i++)
            {
                var ofs = i / 8 + 0x7C;
                if (((Data[ofs] >> (i % 8)) & 1) == 1)
                    sb.AppendLine(GameInfo.Strings.specieslist[i]);
            }

            sb.AppendLine();
            sb.AppendLine("=====");
            sb.AppendLine("Allowed Items");
            sb.AppendLine("=====");
            for (var i = 0; i < GameInfo.Strings.itemlist.Length; i++)
            {
                var ofs = i / 8 + 0x7C;
                if (((Data[ofs] >> (i % 8)) & 1) != 1)
                    sb.AppendLine(GameInfo.Strings.itemlist[i]);
            }

            sb.AppendLine();
            sb.AppendLine("=====");
            sb.AppendLine("Banned Items");
            sb.AppendLine("=====");
            for (var i = 0; i < GameInfo.Strings.itemlist.Length; i++)
            {
                var ofs = i / 8 + 0x7C;
                if (((Data[ofs] >> (i % 8)) & 1) == 1)
                    sb.AppendLine(GameInfo.Strings.itemlist[i]);
            }

            sb.AppendLine();
            sb.AppendLine("=====");
            sb.AppendLine("Allowed Moves");
            sb.AppendLine("=====");
            for (var i = 0; i < GameInfo.Strings.movelist.Length; i++)
            {
                var ofs = i / 8 + 0x7C;
                if (((Data[ofs] >> (i % 8)) & 1) != 1)
                    sb.AppendLine(GameInfo.Strings.movelist[i]);
            }

            sb.AppendLine();
            sb.AppendLine("=====");
            sb.AppendLine("Banned Moves");
            sb.AppendLine("=====");
            for (var i = 0; i < GameInfo.Strings.movelist.Length; i++)
            {
                var ofs = i / 8 + 0x7C;
                if (((Data[ofs] >> (i % 8)) & 1) == 1)
                    sb.AppendLine(GameInfo.Strings.movelist[i]);
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
