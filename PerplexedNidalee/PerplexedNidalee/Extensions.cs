using LeagueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerplexedNidalee
{
    static class Extensions
    {
        public static bool Hunted(this Obj_AI_Base target)
        {
            foreach (BuffInstance buff in target.Buffs)
            {
                if (buff.Name.ToLower() == "nidaleepassivehunted")
                    return true;
            }
            return false;
        }
    }
}
