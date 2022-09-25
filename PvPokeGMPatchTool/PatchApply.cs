using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PvPokeGMPatchTool
{
    static partial class PatchApply
    {
        public static void ApplyPatch(JObject gamemaster, Dictionary<string, List<Change>> changes)
        {
            JArray mons = (JArray)gamemaster["pokemon"];
            JArray moves = (JArray)gamemaster["moves"];
            JArray shadow_eligible = (JArray)gamemaster["shadowPokemon"];

            Program.WriteLineVerbose("Applying Move Changes...");
            ApplyMoves(moves, mons, changes["moves"]);

            Program.WriteLineVerbose("Applying Pokemon Changes...");
            ApplyMons(mons, shadow_eligible, moves, changes["pokemon"]);
        }
    }
}