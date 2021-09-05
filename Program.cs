using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace osu_auth_checker
{
    public class FileEntry
    {
        public string file_version { get; set; }
        public string filename { get; set; }
        public string file_hash { get; set; }
        public string filesize { get; set; }
        public string timestamp { get; set; }
        public string patch_id { get; set; }
        public string url_full { get; set; }
        public string url_patch { get; set; }
    }

    class Program
    {
        static Thread thread;
        private static readonly HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            Console.Title = "osu!auth checker for maple.software";
            Console.WriteLine("Starting checker thread...");
            thread = new Thread(CheckThread) { IsBackground = false };
            thread.Start();
            Console.ReadKey();
        }

        static async void CheckThread()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Downloading data...");
                    string stable, beta, cuttingedge;
                    using (var wc = new System.Net.WebClient())
                    {
                        stable = wc.DownloadString("https://osu.ppy.sh/web/check-updates.php?action=check&stream=stable40");
                        beta = wc.DownloadString("https://osu.ppy.sh/web/check-updates.php?action=check&stream=beta40");
                        cuttingedge = wc.DownloadString("https://osu.ppy.sh/web/check-updates.php?action=check&stream=cuttingedge");
                    }

                    List<FileEntry> stableFiles = JsonConvert.DeserializeObject<List<FileEntry>>(stable);
                    List<FileEntry> betaFiles = JsonConvert.DeserializeObject<List<FileEntry>>(beta);
                    List<FileEntry> cuttingedgeFiles = JsonConvert.DeserializeObject<List<FileEntry>>(cuttingedge);

                    Console.WriteLine("Checking...");

                    string url = "https://maple.software/backend/osuauthhandler.php";
                    foreach (FileEntry file in stableFiles)
                    {
                        if (file.filename == "osu!auth.dll")
                        {
                            var values = new Dictionary<string, string>
                        {
                            { "h", file.file_hash },
                            { "b", "osu!stable" },
                            { "v", file.file_version},
                            { "u", file.timestamp }
                        };

                            var content = new FormUrlEncodedContent(values);

                            var response = await client.PostAsync(url, content);

                            var responseString = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Response: {responseString}");
                        }
                    }

                    foreach (FileEntry file in betaFiles)
                    {
                        if (file.filename == "osu!auth.dll")
                        {
                            var values = new Dictionary<string, string>
                        {
                            { "h", file.file_hash },
                            { "b", "osu!beta" },
                            { "v", file.file_version},
                            { "u", file.timestamp }
                        };

                            var content = new FormUrlEncodedContent(values);

                            var response = await client.PostAsync(url, content);

                            var responseString = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Response: {responseString}");
                        }
                    }

                    foreach (FileEntry file in cuttingedgeFiles)
                    {
                        if (file.filename == "osu!auth.dll")
                        {
                            var values = new Dictionary<string, string>
                        {
                            { "h", file.file_hash },
                            { "b", "osu!cuttingedge" },
                            { "v", file.file_version},
                            { "u", file.timestamp }
                        };

                            var content = new FormUrlEncodedContent(values);

                            var response = await client.PostAsync(url, content);

                            var responseString = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Response: {responseString}");
                        }
                    }

                    Console.WriteLine("Check complete!");
                    stable = ""; beta = ""; cuttingedge = "";
                    stableFiles.Clear(); betaFiles.Clear(); cuttingedgeFiles.Clear();

                    Thread.Sleep(120000); // every two minutes
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
