using Newtonsoft.Json.Linq;

namespace PvPokeGMPatchTool // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string patch_filename = "patch.json";
            string gm_filename = "gamemaster.json";
            string new_gm_filename = "gamemasternew.json";

            JObject gamemaster = JObject.Parse(File.ReadAllText(gm_filename));
          
            JArray mons = (JArray)gamemaster["pokemon"];
            JArray moves = (JArray)gamemaster["moves"];
            JArray shadow_eligible = (JArray)gamemaster["shadowPokemon"];

            Console.WriteLine("mons before: " + mons.Children().Count());
            Console.WriteLine("moves before: " + moves.Children().Count());

            Dictionary<string, List<Change>> patch = PatchParse.ParsePatchFile(patch_filename);

            PatchApply.ApplyMoves(moves, mons, patch["moves"]);
            PatchApply.ApplyMons(mons, shadow_eligible, moves, patch["pokemon"]);

            File.WriteAllText(new_gm_filename, gamemaster.ToString());  
        }

    }
}