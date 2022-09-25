using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PvPokeGMPatchTool
{
    static partial class PatchParse
    {
        public static Dictionary<string, List<Change>> ParsePatchFile(JObject patch)
        {
            Dictionary<string, List<Change>> patchChanges = new Dictionary<string, List<Change>>();

            patchChanges["moves"] = ParseChanges((JArray)patch["moves"]);
            patchChanges["pokemon"] = ParseChanges((JArray)patch["pokemon"]);

            return patchChanges;
        }

        public static List<Change> ParseChanges(JArray Source)
        {
            List<Change> changes = new List<Change>();

            foreach (JObject entry in Source)
            {
                if (!entry.ContainsKey("target") || !entry.ContainsKey("action"))
                {
                    throw new ArgumentException("Invalid Patch Entry (must have \"target\" and \"action\" fields)");
                }

                string Target = (string)entry["target"];
                PatchAction Action = ParseAction((string)entry["action"]);

                JObject Changes = new JObject();

                if (entry.ContainsKey("changes"))
                {
                    Changes = (JObject)entry["changes"];
                }
                else
                {
                    if (Action != PatchAction.Delete)
                    {
                        throw new ArgumentException("Invalid Patch Entry (non-Delete actions must have the \"changes\" field present)");
                    }
                }

                changes.Add(new Change(Target, Action, Changes));
            }

            return changes;
        }

        public static PatchAction ParseAction(string str) => str.ToLower() switch
        {
            "modify" => PatchAction.Modify,
            "add" => PatchAction.Add,
            "clone" => PatchAction.Clone,
            "delete" => PatchAction.Delete,
            "addmove" => PatchAction.AddMove,
            "deletemove" => PatchAction.DeleteMove,
            _ => throw new ArgumentException("Invalid Action: \"" + str + "\""),
        };
    }
}
