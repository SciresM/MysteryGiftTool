using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using PKHeX.Core;

namespace MysteryGiftTool
{
    internal class Game
    {
        public string Name;
        public string ID;
        public int Generation;
    }

    internal static class Program
    {
        private static DateTime now = DateTime.Now;
        private static bool keep_log;
        private static StreamWriter log;
        private const string filelist_server = "https://npfl.c.app.nintendowifi.net/p01/filelist/{0}/FGONLYT?ap=11012900000";
        private const string file_server = "https://npdl.cdn.nintendowifi.net/p01/nsa/{0}/FGONLYT/{1}?ap=11012900000&tm=2";

        private static readonly Game[] games =
        {
            new Game {Name = "Sun", ID = "8QjtffIMWFhiFpTz", Generation = 7},
            new Game {Name = "Moon", ID = "7mXz0DXR4b4CdD8r", Generation = 7},
            new Game {Name = "X", ID = "h0VRqB2YEgq39zvO", Generation = 6},
            new Game {Name = "Y", ID = "Slv7vHlUOfqrKMpz", Generation = 6},
            new Game {Name = "Omega Ruby", ID = "cRFY0WFHNjPh44If", Generation = 6},
            new Game {Name = "Alpha Sapphire", ID = "guBwm9TlQvYvncKn", Generation = 6}
        };
        

        public static void CreateDirectoryIfNull(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        public static void Log(string msg)
        {
            Console.WriteLine(msg);
            log.WriteLine(msg);
        }


        private static void Main(string[] args)
        {

            CreateDirectoryIfNull("logs");
            CreateDirectoryIfNull("data");
            CreateDirectoryIfNull("wondercards");
            CreateDirectoryIfNull("regulations");
            CreateDirectoryIfNull("cups");
            foreach (var game in games)
                CreateDirectoryIfNull(Path.Combine("data", game.Name));
            var log_file = $"logs/{now.ToString("MMMM dd, yyyy - HH-mm-ss")}.log";
            log = new StreamWriter(log_file, false, Encoding.Unicode);

            Log("MysteryGiftTool v1.0 - SciresM");
            Log($"{now.ToString("MMMM dd, yyyy - HH-mm-ss")}");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            Log("Installed certificate bypass.");

            try
            {
                UpdateArchives();
                Log("Testing Crypto Server...");
                if (NetworkUtils.TestCryptoServer())
                {
                    keep_log = true;
                    Log("Decrypting and extracting gifts...");
                    GameInfo.Strings = GameInfo.getStrings("en");
                    ExtractArchives();
                }
                else
                    keep_log = true;
            }
            catch (Exception ex)
            {
                keep_log = true;
                Log($"An exception occurred: {ex.Message}");
            }

            log.Close();
            if (!keep_log)
                File.Delete(log_file);
        }

        private static void UpdateArchives()
        {
            foreach (var game in games)
            {
                Log($"Updating for {game.Name}...");
                var game_dir = Path.Combine("data", game.Name);
                var game_id = game.ID;
                var updated = false;
                var server_filelist = string.Format(filelist_server, game_id);
                var fl_path = Path.Combine(game_dir, "list.txt");
                var fl = NetworkUtils.MakeCertifiedRequest(server_filelist);
                var old_fl = "";
                if (!File.Exists(fl_path))
                {
                    updated = true;
                    keep_log = true;
                    File.WriteAllText(fl_path, fl);
                }
                else
                {
                    old_fl = File.ReadAllText(fl_path);
                    if (old_fl != fl)
                    {
                        updated = true;
                        keep_log = true;
                        File.WriteAllText(fl_path, fl);
                    }
                }

                if (!updated)
                {
                    Log($"No updates for {game.Name}.");
                    continue;
                }

                Log($"Downloading new BOSS archives for {game.Name}...");
                var archive_dir = Path.Combine(game_dir, "boss");
                CreateDirectoryIfNull(archive_dir);
                var new_boss = fl.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Contains("\t")).Select(BossMetadata.FromString).ToList();
                var old_boss = old_fl.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Contains("\t")).Select(BossMetadata.FromString).ToList();
                foreach (var boss in new_boss)
                {
                    var server_data_url = string.Format(file_server, game_id, boss.Name);
                    var archive_path = Path.Combine(archive_dir, boss.ArchiveName);
                    if (File.Exists(archive_path))
                        continue;
                    var encrypted_archive = NetworkUtils.TryDownload(server_data_url);
                    if (encrypted_archive == null)
                        continue;
                    File.WriteAllBytes(archive_path, encrypted_archive);
                    Log($"Downloaded {boss.FileName}.");
                    if (old_boss.Any(bm => boss.IsUpdatedVersionOf(bm)))
                        Log($"{boss.FileName} is an updated version of an old archive!");
                }
            }
        }

        private static void ExtractArchives()
        {
            foreach (var game in games)
            {
                Log($"Extracting archives for {game.Name}...");
                var game_dir = Path.Combine("data", game.Name);
                var archive_dir = Path.Combine(game_dir, "boss");
                var dec_dir = Path.Combine(game_dir, "boss_dec");
                CreateDirectoryIfNull(archive_dir);
                CreateDirectoryIfNull(dec_dir);
                foreach (var file in new DirectoryInfo(archive_dir).GetFiles())
                {
                    if (!file.Name.Contains("-_-"))
                        continue;
                    var boss = BossMetadata.FromArchiveName(file.Name);
                    var dec_path = Path.Combine(dec_dir, boss.FileName);
                    if (File.Exists(dec_path))
                        continue;
                    Log($"Decrypting {boss.FileName}...");
                    var dec_data = NetworkUtils.TryDecryptBOSS(File.ReadAllBytes(file.FullName));
                    if (dec_data == null)
                    {
                        Log($"Failed to decrypt {boss.FileName}");
                        continue;
                    }
                    Log($"Decrypted {boss.FileName}.");
                    File.WriteAllBytes(dec_path, dec_data);

                    var content_data = dec_data.Skip(0x296).ToArray();
                    if (content_data.Length == 0x310) // Wondercard!
                    {
                        var wcgdir = Path.Combine("wondercards", game.Name);
                        var wcdir = Path.Combine(wcgdir, $"wc{game.Generation}");
                        var wcfulldir = Path.Combine(wcgdir, $"wc{game.Generation}full");
                        CreateDirectoryIfNull(wcgdir);
                        CreateDirectoryIfNull(wcdir);
                        CreateDirectoryIfNull(wcfulldir);

                        File.WriteAllBytes(Path.Combine(wcfulldir, boss.FileName + $".wc{game.Generation}full"), content_data);

                        MysteryGift wc = null;
                        if (game.Generation == 6)
                        {
                            wc = new WC6(content_data);
                            File.WriteAllBytes(Path.Combine(wcdir, boss.FileName + $".wc{game.Generation}"), wc.Data);
                        }
                        else if (game.Generation == 7)
                        {
                            wc = new WC7(content_data);
                            File.WriteAllBytes(Path.Combine(wcdir, boss.FileName + $".wc{game.Generation}"), wc.Data);
                        }

                        Log($"{boss.FileName} is a wondercard ({wc.Type}): ");
                        Log(wc.FullDesc);
                        Log(MysteryGift.getDescription(wc));
                    }
                    else if (boss.Name.ToUpper().Contains("CUP") && content_data.Length == 0x4C0) // CUP Regulation
                    {
                        Log($"{boss.FileName} is a CUP!");
                        var cup_dir = Path.Combine("cups", game.Name);
                        CreateDirectoryIfNull(cup_dir);
                        var reg_arc = new RegulationArchive(content_data, boss.FileName);
                        Log($"Extracting/Saving {boss.FileName}...");
                        reg_arc.Save(cup_dir);
                    }
                    else if (boss.Name.Contains("regulation") && game.Generation == 7) // Gen VII Regulations
                    {
                        Log($"{boss.FileName} is a regulation!");
                        var reg_dir = Path.Combine("regulations", game.Name);
                        CreateDirectoryIfNull(reg_dir);
                        var reg_arc = new RegulationArchive(content_data, boss.FileName);
                        Log($"Extracting/Saving {boss.FileName}...");
                        reg_arc.Save(reg_dir);
                    }
                }
            }
        }
    }
}
