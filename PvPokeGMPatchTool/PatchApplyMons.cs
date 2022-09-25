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
        public static void ApplyMons(JArray mons, JArray shadows, JArray moves, List<Change> changes)
        {
            foreach (Change change in changes)
            {
                ApplyMon(mons, shadows, moves, change);
            }
        }

        static void ApplyMon(JArray mons, JArray shadows, JArray moves, Change change)
        {
            if (change.Action == PatchAction.Modify)
            {
                JObject mon = (JObject)mons.Where(x => (string)(((JObject)x)["speciesId"]) == MonIDStringConvert(change.Target)).First();
                PatchObject(mon, change.Changes);
                Program.WriteLineQuiet($"Pokemon | Modify | {change.Target} | Modified {change.Changes.Count} Fields");
            }
            else if (change.Action == PatchAction.Clone)
            {
                JObject mon = (JObject)mons.Where(x => (string)(((JObject)x)["speciesId"]) == MonIDStringConvert(change.Target)).First();
                JObject clonedMon = new JObject();
                PatchObject(clonedMon, mon);
                PatchObject(clonedMon, change.Changes);

                while (HasMon(mons, (string)clonedMon["speciesId"]))
                {
                    clonedMon["speciesId"] += "_new";
                }

                if (!change.Changes.ContainsKey("speciesName"))
                {
                    clonedMon["speciesName"] += " (New)";
                }

                mons.Add(clonedMon);
                Program.WriteLineQuiet($"Pokemon | Clone | {change.Target} | Cloned into {(string)clonedMon["speciesId"]}, modifying {change.Changes.Count} Fields");
            }
            else if (change.Action == PatchAction.Add)
            {
                JObject newMon = new JObject();
                PatchObject(newMon, change.Changes);

                while (HasMon(mons, (string)newMon["speciesId"]))
                {
                    newMon["speciesId"] += "_new";
                }

                if (!change.Changes.ContainsKey("speciesName"))
                {
                    newMon["speciesName"] = "New Mon";
                }

                if (change.Target.Trim().ToLower() != "noshadow")
                {
                    JObject newShadow = new JObject();
                    PatchObject(newShadow, change.Changes);
                    newShadow["speciesId"] += "_shadow";
                    newShadow["speciesName"] += " (Shadow)";

                    ((JArray)newMon["tags"]).Add("shadoweligible");
                    ((JArray)newShadow["tags"]).Add("shadow");
                    shadows.Add((string)newMon["speciesId"]);
                    mons.Add(newShadow);
                    mons.Add(newMon);

                    Program.WriteLineQuiet($"Pokemon | Add | Added {(string)newMon["speciesName"]}, id: \"{(string)newMon["speciesId"]}\", and its shadow {(string)newShadow["speciesName"]}, id: \"{(string)newShadow["speciesId"]}\"");
                }
                else
                {
                    mons.Add(newMon);

                    Program.WriteLineQuiet($"Pokemon | Add | Added {(string)newMon["speciesName"]}, id: \"{(string)newMon["speciesId"]}\"");
                }
            }
            else if (change.Action == PatchAction.Delete)
            {
                Program.WriteLineQuiet($"Pokemon | Delete | {change.Target} | Don't delete mons you goober");
            }
            else if (change.Action == PatchAction.AddMove)
            {
                List<string> targets = new List<string>();
                targets.Add(MonIDStringConvert(change.Target));
                if (shadows.Where(x => (string)x == MonIDStringConvert(change.Target)).Count() > 0)
                {
                    targets.Add(MonIDStringConvert(change.Target) + "_shadow");
                }

                foreach (string target in targets)
                {
                    JObject mon = (JObject)mons.Where(x => (string)(((JObject)x)["speciesId"]) == target).First();
                    JArray fast = ((JArray)mon["fastMoves"]);
                    JArray charged = ((JArray)mon["chargedMoves"]);

                    int addedfast = 0;
                    if (change.Changes.ContainsKey("fastMoves"))
                    {
                        foreach (string move in (JArray)change.Changes["fastMoves"])
                        {
                            List<string> movestoadd = moves.Where(x => ((string)(((JObject)x)["moveId"])) == MoveIDStringConvert(move) || IsModOf(MoveIDStringConvert(move), ((string)(((JObject)x)["moveId"])))).Select(x => ((string)(((JObject)x)["moveId"]))).ToList();
                            foreach (string finmove in movestoadd)
                            {
                                fast.Add(finmove);
                                addedfast++;
                            }
                        }
                    }

                    int addedcharged = 0;
                    if (change.Changes.ContainsKey("chargedMoves"))
                    {
                        foreach (string move in (JArray)change.Changes["chargedMoves"])
                        {
                            List<string> movestoadd = moves.Where(x => ((string)(((JObject)x)["moveId"])) == MoveIDStringConvert(move) || IsModOf(MoveIDStringConvert(move), ((string)(((JObject)x)["moveId"])))).Select(x => ((string)(((JObject)x)["moveId"]))).ToList();
                            foreach (string finmove in movestoadd)
                            {
                                charged.Add(finmove);
                                addedcharged++;
                            }
                        }
                    }

                    mon["fastMoves"] = fast;
                    mon["chargedMoves"] = charged;
                    Program.WriteLineQuiet($"Pokemon | AddMove | {change.Target} | Added {addedfast} Fast Moves and {addedcharged} Charged Moves");
                }
            }
            else if (change.Action == PatchAction.DeleteMove)
            {
                List<string> targets = new List<string>();
                targets.Add(MonIDStringConvert(change.Target));
                if (shadows.Where(x => (string)x == MonIDStringConvert(change.Target)).Count() > 0)
                {
                    targets.Add(MonIDStringConvert(change.Target) + "_shadow");
                }

                foreach (string target in targets)
                {

                    JObject mon = (JObject)mons.Where(x => (string)(((JObject)x)["speciesId"]) == target).First();
                    JArray fast = ((JArray)mon["fastMoves"]);
                    JArray charged = ((JArray)mon["chargedMoves"]);

                    int delfast = 0;
                    if (change.Changes.ContainsKey("fastMoves"))
                    {
                        foreach (string move in (JArray)change.Changes["fastMoves"])
                        {
                            if (fast.Where(x => (string)x == move).Count() > 0)
                            {
                                JArray tmp = new JArray(fast.Where(x => (string)x != move && !IsModOf(move, (string)x)).ToArray());
                                delfast += (fast.Count() - tmp.Count());
                                fast = tmp;
                            }   
                        }
                    }

                    int delcharged = 0;
                    if (change.Changes.ContainsKey("chargedMoves"))
                    {
                        foreach (string move in (JArray)change.Changes["chargedMoves"])
                        {
                            if (charged.Where(x => (string)x == move).Count() > 0)
                            {
                                JArray tmp = new JArray(charged.Where(x => (string)x != move && !IsModOf(move, (string)x)).ToArray());
                                delcharged += (charged.Count() - tmp.Count());
                                charged = tmp;
                            }
                        }
                    }

                    if (fast.Count() == 0)
                    {
                        fast.Add("STRUGGLE");
                    }

                    if (charged.Count() == 0)
                    {
                        charged.Add("STRUGGLE");
                    }

                    mon["fastMoves"] = fast;
                    mon["chargedMoves"] = charged;
                    Program.WriteLineQuiet($"Pokemon | DeleteMove | {target} | Deleted {delfast} Fast Moves and {delcharged} Charged Moves");
                }
            }
        }

        static string MonIDStringConvert(string input)
        {
            return input.ToLower().Trim().Replace(' ', '_');
        }

        static bool HasMon(JArray mons, string monname)
        {
            return mons.Where(x => (string)(((JObject)x)["speciesId"]) == MonIDStringConvert(monname)).Count() > 0;
        }
    }
}