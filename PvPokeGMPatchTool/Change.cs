using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPokeGMPatchTool
{
    public enum PatchAction
    {
        Modify,
        Clone,
        Delete,
        Add,
        AddMove,
        DeleteMove,
    }

    public class Change
    {
        public string Target;
        public PatchAction Action;
        public JObject Changes;

        public Change (string target, PatchAction action, JObject changes)
        {
            Target = target;
            Action = action;
            Changes = changes;
        }   
    }
}
