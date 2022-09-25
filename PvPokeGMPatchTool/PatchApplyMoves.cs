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
        public static void ApplyMoves(JArray moves, JArray mons, List<Change> changes)
        {
            foreach (Change change in changes)
            {
                ApplyMove(moves, mons, change);
            }
        }

        static void ApplyMove(JArray moves, JArray mons, Change change)
        {           
            if (change.Action == PatchAction.Modify)
            {
                JObject move = (JObject)moves.Where(x => (string)(((JObject)x)["moveId"]) == MoveIDStringConvert(change.Target)).First();
                PatchObject(move, change.Changes);
                Program.WriteLineQuiet($"Move | Modify | {change.Target} | Modified {change.Changes.Count} Fields");
            }
            else if (change.Action == PatchAction.Clone)
            {
                JObject move = (JObject)moves.Where(x => (string)(((JObject)x)["moveId"]) == MoveIDStringConvert(change.Target)).First();
                JObject clonedMove = new JObject();
                PatchObject(clonedMove, move);
                PatchObject(clonedMove, change.Changes);
                string newAbbr = GenerateMoveAbbreviation(clonedMove);
                string newName = GenerateMoveDisplayName(clonedMove);
                string newMoveID = GenerateMoveID(clonedMove);               

                while (HasMove(moves, newMoveID))
                {
                    newMoveID += "_NEW";
                }

                clonedMove["moveId"] = newMoveID;
                clonedMove["name"] = newName;
                clonedMove["abbreviation"] = newAbbr;

                moves.Add(clonedMove);

                bool clonemovefast = ((int)clonedMove["energy"] == 0);
                string originalMoveID = (string)move["moveId"];

                int monpatches = 0;
                foreach (JObject mon in mons)
                {
                    if (clonemovefast)
                    {
                        JArray fast = ((JArray)mon["fastMoves"]);

                        if (fast.Where(x => (string)x == originalMoveID).Count() > 0)
                        {
                            fast.Add(newMoveID);
                            mon["fastMoves"] = fast;
                            monpatches++;
                        }
                    }
                    else
                    {
                        JArray charged = ((JArray)mon["chargedMoves"]);

                        if (charged.Where(x => (string)x == originalMoveID).Count() > 0)
                        {
                            charged.Add(newMoveID);
                            mon["chargedMoves"] = charged;
                            monpatches++;
                        }
                    }                 
                }
                Program.WriteLineQuiet($"Move | Clone | {change.Target} | Cloned into {(string)clonedMove["moveId"]}, modifying {change.Changes.Count} Fields, and added the new move to {monpatches} Pokemon");
            }
            else if (change.Action == PatchAction.Add)
            {
                JObject newMove = new JObject();
                PatchObject(newMove, change.Changes);            

                while (HasMove(moves, (string)newMove["moveId"]))
                {
                    newMove["moveId"] = (string)newMove["moveId"] + "_NEW";
                }

                moves.Add(newMove);

                bool newmovefast = ((int)newMove["energy"] == 0);

                List<string> monstoadd = change.Target.Split(',').Select(x => x.Trim()).ToList();
                monstoadd.Concat(monstoadd.Select(x => x + "_shadow").ToList());

                int monsadded = 0;
                foreach (JObject mon in mons)
                {
                    if (monstoadd.Contains(((string)mon["speciesId"]).Trim()))
                    {
                        if (newmovefast)
                        {
                            JArray fast = ((JArray)mon["fastMoves"]);
                            fast.Add((string)newMove["moveId"]);
                            mon["fastMoves"] = fast;
                            monsadded++;
                        }
                        else
                        {
                            JArray charged = ((JArray)mon["chargedMoves"]);
                            charged.Add((string)newMove["moveId"]);
                            mon["chargedMoves"] = charged;
                            monsadded++;
                        }
                    }
                }

                Program.WriteLineQuiet($"Move | Add | Added {(string)newMove["moveId"]}, adding it to movesets of {monsadded} Pokemon");
            }
            else if (change.Action == PatchAction.Delete)
            {
                JObject move = (JObject)moves.Where(x => (string)(((JObject)x)["moveId"]) == MoveIDStringConvert(change.Target)).First();
                bool delmovefast = ((int)move["energy"] == 0);
                string moveID = (string)move["moveId"];
                moves.Remove(move);

                int movesdeld = 0;
                int monsdeld = 0;
                foreach (JObject mon in mons)
                {
                    if (delmovefast)
                    {
                        JArray fast = ((JArray)mon["fastMoves"]);
                        if (fast.Where(x => (string)x == moveID).Count() > 0)
                        {
                            monsdeld++;
                            JArray tmp = new JArray(fast.Where(x => (string)x != moveID && !IsModOf(moveID, (string)x)).ToArray());
                            movesdeld += (fast.Count() - tmp.Count());
                            mon["fastMoves"] = tmp;      
                        }

                        //if (fast.Remove(new JValue(moveID)))
                        //{
                        //    movesdeld++;
                        //    mon["fastMoves"] = fast;
                        //}                     
                    }
                    else
                    {
                        JArray charged = ((JArray)mon["chargedMoves"]);
                        if (charged.Where(x => (string)x == moveID).Count() > 0)
                        {
                            monsdeld++;
                            JArray tmp = new JArray(charged.Where(x => (string)x != moveID && !IsModOf(moveID, (string)x)).ToArray());
                            movesdeld += (charged.Count() - tmp.Count());
                            mon["chargedMoves"] = tmp;
                        }                      
                    }
                }
                Program.WriteLineQuiet($"Move | Delete | {change.Target} | Deleted the move, removing it and its mods from {monsdeld} Pokemon, {movesdeld} in total");
            }
        }

        static void PatchObject(JObject move, JObject changes)
        {
            foreach (JProperty prop in changes.Properties())
            {
                move[prop.Name] = prop.Value;
            }
        }


        static string MoveIDStringConvert(string input)
        {
            return input.ToUpper().Trim().Replace(' ', '_');
        }

        static bool HasMove(JArray moves, string movename)
        {
            return moves.Where(x => (string)(((JObject)x)["moveId"]) == MoveIDStringConvert(movename)).Count() > 0;
        }

        static string GenerateMoveID(JObject move)
        {
            return (string)move["moveId"] + "_MOD_" + (int)move["power"] + "_" + ((int)move["energy"] + (int)move["energyGain"]);
        }

        static string GenerateMoveDisplayName(JObject move)
        {
            return (string)move["name"] + " (" + (int)move["power"] + (move.ContainsKey("buffs") ? "+" : "") + "/" + ((int)move["energy"] + (int)move["energyGain"]) + ")";
        }

        static string GenerateMoveAbbreviation(JObject move)
        {
            return ((move.ContainsKey("abbreviation")) ? (string)move["abbreviation"] : new string(((string)move["name"]).Trim().Split(' ').Select(x => x[0]).ToArray())) + "+";
        }

        static bool IsModOf(string basemove, string testmove)
        {
            return testmove.StartsWith(basemove + "_MOD_");
        }
    }
}