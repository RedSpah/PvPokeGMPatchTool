using Newtonsoft.Json.Linq;
using CommandLine;
using System.Net;
using System;
using System.Diagnostics;

namespace PvPokeGMPatchTool // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public class Options
        {
            [Option('p', "pure_gm_file", Default = "gamemaster_pure.json", HelpText = "File name to be used for the pure, unchanged gamemaster.json file. Superceded by custom pure gamemaster path.")]
            public string PureGameMasterFile { get; set; }

            [Option('P', "pure_gm_path", Default = "", HelpText = "Custom path to the pure gamemaster file.")]
            public string PureGameMasterPath { get; set; }

            [Option('d', "download", Default = false, HelpText = "If set to true, downloads the pure gamemaster from the PvPoke GitHub repo if not present. Otherwise, will create the pure gamemaster out of the already present one.")]
            public bool Download { get; set; }

            [Option('D', "force_download", Default = false, HelpText = "If set to true, always downloads the pure gamemaster from the PvPoke GitHub repo, regardless if its already present or not.")]
            public bool ForceDownload { get; set; }

            [Option('f', "patch_path", Default = "./patch.json", HelpText = "Path to the patch file to use. (./patch.json by default)")]
            public string PatchPath { get; set; }

            [Option('q', "quiet", Default = false, HelpText = "Quiet mode, suppresses the printing of any messages except for errors.")]
            public bool Quiet { get; set; }

            [Option('r', "xampp_folder", Default = "pvpoke", HelpText = "The folder to use in xampp/htdocs if using the default configuration. Superceded by custom gamemaster folder path.")]
            public string XamppFolder { get; set; }

            [Option('g', "gamemaster_file", Default = "gamemaster.json", HelpText = "File name to be used for the final gamemaster, and which may be used to prepare the pure gamemaster.")]
            public string GameMasterFile { get; set; }

            [Option('G', "gamemaster_path", Default = "", HelpText = "Custom path to the folder containing the gamemaster.")]
            public string GameMasterPath { get; set; }

            [Option('n', "noreset", Default = false, HelpText = "If set to true, doesn't execute a server reset action after updating the gamemaster.")]
            public bool NoReset { get; set; }

            [Option('c', "custom_reset_script_path", Default = "", HelpText = "Path to the custom script to be executed to restart the apache server pvpoke is running on.")]
            public string CustomResetScriptPath { get; set; }

            [Option('v', "verbose", Default = false, HelpText = "Verbose output.")]
            public bool Verbose { get; set; }
        }

        public static bool Quiet = false;
        public static bool Verbose = false;

        static void Main(string[] args)
        {
            bool NoReset = false;
            bool Download = false;
            bool ForceDownload = false;
            bool CustomResetScript = false;
            string GameMasterFolderPath = "";
            string GameMasterPath = "";
            string PureGameMasterPath = "";
            string PatchPath = "";
            string ResetScriptPath = "";

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    Download = o.Download;
                    ForceDownload = o.ForceDownload;
                    Quiet = o.Quiet;
                    Verbose = o.Verbose;
                    NoReset = o.NoReset;

                    if (string.IsNullOrWhiteSpace(o.GameMasterPath))
                    {
                        GameMasterFolderPath = "../" + o.XamppFolder + "/src/data";
                    }
                    else
                    {
                        GameMasterFolderPath = o.GameMasterPath;
                    }

                    GameMasterPath = GameMasterFolderPath + "/" + o.GameMasterFile;

                    if (string.IsNullOrWhiteSpace(o.PureGameMasterPath))
                    {
                        PureGameMasterPath = GameMasterFolderPath + "/" + o.PureGameMasterFile;
                    }
                    else
                    {
                        PureGameMasterPath = o.PureGameMasterPath;
                    }

                    PatchPath = o.PatchPath;

                    if (!string.IsNullOrWhiteSpace(o.PureGameMasterPath))
                    {
                        CustomResetScript = true;
                        ResetScriptPath = o.CustomResetScriptPath;
                    }
                });

            WriteLineQuiet("=== Welcome to Grumpig 0.1! ===");
            WriteLineQuiet($"Patch File Path: {PatchPath}");
            WriteLineQuiet($"Gamemaster File Path: {GameMasterPath}");
            WriteLineQuiet($"Pure Gamemaster File Path: {PureGameMasterPath}");
            if (CustomResetScript)
            {
                WriteLineQuiet($"Custom Reset Path: {ResetScriptPath}");
            }

            JObject PatchFileJSON = new JObject();
            JObject GameMasterJSON = new JObject();

            // Patch File Handling
            if (!File.Exists(PatchPath))
            {
                Console.WriteLine("Error: Patch File specified does not exist or is otherwise invalid.");
                return;
            } 

            try
            {
                WriteLineVerbose("Reading and parsing the Patch File...");
                PatchFileJSON = JObject.Parse(File.ReadAllText(PatchPath));
                WriteLineVerbose("Patch File loaded.");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while reading and parsing the Patch File: {e.Message}");
                return;
            }


            // Pure Gamemaster Handling

            // If doesn't exist
            try
            {
                if (ForceDownload)
                {
                    WriteLineVerbose("Force Downloading Pure Gamemaster from PvPoke GitHub...");
                    DownloadPureGamemaster(PureGameMasterPath, @"https://raw.githubusercontent.com/pvpoke/pvpoke/master/src/data/gamemaster.min.json");
                    WriteLineVerbose("Pure Gamemaster created.");
                }
                else
                {
                    if (!File.Exists(PureGameMasterPath))
                    {
                        if (Download)
                        {
                            WriteLineVerbose("Pure Gamemaster not found, downloading Pure Gamemaster from PvPoke GitHub...");
                            DownloadPureGamemaster(PureGameMasterPath, @"https://raw.githubusercontent.com/pvpoke/pvpoke/master/src/data/gamemaster.min.json");
                            WriteLineVerbose("Pure Gamemaster created.");
                        }
                        else
                        {
                            if (!File.Exists(GameMasterPath))
                            {
                                Console.WriteLine("Error: Neither the Pure Gamemaster File nor the Gamemaster File exist. Use the -d argument if you want to download the Pure Gamemaster File from PvPoke Github repo, or specify a correct Gamemaster File path.");
                                return;
                            }

                            WriteLineVerbose("Creating Pure Gamemaster from current gamemaster...");
                            File.Copy(GameMasterPath, PureGameMasterPath);
                            WriteLineVerbose("Pure Gamemaster created.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while creating the Pure Gamemaster File: {e.Message}");
                return;
            }

            // Loading
            try
            {
                WriteLineVerbose("Reading and parsing the Pure Gamemaster File...");
                GameMasterJSON = JObject.Parse(File.ReadAllText(PureGameMasterPath));
                WriteLineVerbose("Pure Gamemaster File loaded.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while reading and parsing the Pure Gamemaster File: {e.Message}");
                return;
            }


            // Restart Script Handling
            if (!NoReset && !CustomResetScript && (!File.Exists("../../apache_stop.bat") || !File.Exists("../../apache_start.bat")))
            {
                Console.WriteLine($"Error: Could not find the default XAMPP Reset scripts (apache_stop.bat and apache_start.bat). Make sure you are running this program in the right location, specified in the README. If you're not on Windows, you'll need to either pass the -n flag to disable reset script, or pass the path to a custom Apache server restart script with the -c flag.");
                return;
            }


            Dictionary<string, List<Change>> Changes = new Dictionary<string, List<Change>>();

            // Parsing Changes
            WriteLineVerbose("Constructing the list of Changes...");
            try
            {
                Changes = PatchParse.ParsePatchFile(PatchFileJSON);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while parsing the Patch File: {e.Message}");
                return;
            }

            // Before Text
            try
            {
                WriteLineVerbose($"Pokemon before Changes: {((JArray)GameMasterJSON["pokemon"]).Children().Count()}");
                WriteLineVerbose($"Moves before Changes: {((JArray)GameMasterJSON["moves"]).Children().Count()}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while using the Gamemaster. Make sure the Pure Gamemaster is a valid PvPoke Gamemaster file: {e.Message}");
                return;
            }

            // Applying Changes
            WriteLineVerbose("Applying the Changes...");
            try
            {
                PatchApply.ApplyPatch(GameMasterJSON, Changes);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while applying the Changes: {e.Message}");
                return;
            }

            // After Text
            try
            {
                WriteLineVerbose($"Pokemon after Changes: {((JArray)GameMasterJSON["pokemon"]).Children().Count()}");
                WriteLineVerbose($"Moves after Changes: {((JArray)GameMasterJSON["moves"]).Children().Count()}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while using the Gamemaster. Make sure the Pure Gamemaster is a valid PvPoke Gamemaster file: {e.Message}");
                return;
            }

            // Writing Changes
            WriteLineVerbose("Writing the new Gamemaster...");
            try
            {
                File.WriteAllText(GameMasterPath, GameMasterJSON.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: An Exception occured while writing the new Gamemaster: {e.Message}");
                return;
            }


            if (!NoReset)
            {
                // Restarting
                WriteLineVerbose("Restarting the Apache server...");
                try
                {
                    if (CustomResetScript)
                    {
                        Process CustomReset = new Process();
                        CustomReset.StartInfo.FileName = ResetScriptPath;
                        CustomReset.Start();
                    }
                    else 
                    {
                        DefaultRestartScript();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: An Exception occured while restarting the Apache server: {e.Message}");
                    return;
                }
            }

            WriteLineVerbose("Done!");
        }


        public static void WriteLineQuiet(string text)
        {
            if (!Quiet) { Console.WriteLine(text); }
        }

        public static void WriteLineVerbose(string text)
        {
            if (Verbose) { Console.WriteLine(text); }
        }

        async static void DownloadPureGamemaster(string path, string URL)
        {
            using (HttpClient client = new HttpClient()) 
            {
                Uri URI = new Uri(URL);

                using (Stream stream = await client.GetStreamAsync(URI))
                {
                    using (FileStream filestream = new FileStream(path, FileMode.Create))
                    {
                        await stream.CopyToAsync(filestream);
                    }
                }
            }
        }

        static void DefaultRestartScript()
        {
            Process apache_stop = new Process();
            apache_stop.StartInfo.FileName = "../../apache_stop.bat";

            Process apache_start = new Process();
            apache_start.StartInfo.FileName = "../../apache_start.bat";

            apache_stop.Start();

            Thread.Sleep(2000); // Giving the Apache Server time to shut down

            apache_start.Start();
        }
    }
}