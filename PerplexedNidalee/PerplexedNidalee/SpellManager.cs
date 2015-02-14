using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PerplexedNidalee
{
    class SpellManager
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        private static Spell _Javelin, _Bushwhack, _PrimalSurge;
        private static Spell _Takedown, _Pounce, _Swipe;
        private static Spell _AspectOfTheCougar;

        public static Spell Javelin { get { return _Javelin; } }
        public static Spell Bushwhack { get { return _Bushwhack; } }
        public static Spell PrimalSurge { get { return _PrimalSurge; } }
        public static Spell Takedown { get { return _Takedown; } }
        public static Spell Pounce { get { return _Pounce; } }
        public static Spell Swipe { get { return _Swipe; } }
        public static Spell AspectOfTheCougar { get { return _AspectOfTheCougar; } }

        public static void Initialize()
        {
            _Javelin = new Spell(SpellSlot.Q, 1500f);
            _Javelin.SetSkillshot(0.125f, 40f, 1300f, true, SkillshotType.SkillshotLine);

            _Bushwhack = new Spell(SpellSlot.W, 900f);
            _Bushwhack.SetSkillshot(0.50f, 100f, 1500f, false, SkillshotType.SkillshotCircle);

            _PrimalSurge = new Spell(SpellSlot.E, 650f);

            _Takedown = new Spell(SpellSlot.Q, 200f);

            _Pounce = new Spell(SpellSlot.W, 375f);
            _Pounce.SetSkillshot(0.50f, 400f, 1500f, false, SkillshotType.SkillshotCone);

            _Swipe = new Spell(SpellSlot.E, 300f);
            _Swipe.SetSkillshot(0.50f, 375f, 1500f, false, SkillshotType.SkillshotCone);

            _AspectOfTheCougar = new Spell(SpellSlot.R);
        }

        public static void CastSpell(Spell spell, Obj_AI_Base target, HitChance hitChance, bool packetCast)
        {
            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitChance)
                spell.Cast(target, packetCast);
        }

        public static void CastSpell(Spell spell, Obj_AI_Base target, bool packetCast)
        {
                spell.Cast(target, packetCast);
        }

        public static void CastSpell(Spell spell, Vector3 position, bool packetCast)
        {
            spell.Cast(position, false);
        }

        public static void UseHealIfInDanger(double incomingDmg)
        {
            if (Config.UseSummHeal && !Player.InFountain())
            {
                int healthToUse = (int)(Player.MaxHealth / 100) * Config.HealPct;
                if ((Player.Health - incomingDmg) <= healthToUse)
                {
                    SpellSlot healSlot = Utility.GetSpellSlot(Player, "SummonerHeal");
                    if (healSlot != SpellSlot.Unknown)
                        Player.Spellbook.CastSpell(healSlot);
                }
            }
        }

        internal static void IgniteIfPossible()
        {
            if (Config.UseIgnite)
            {
                SpellSlot igniteSlot = Utility.GetSpellSlot(Player, "SummonerDot");
                if (igniteSlot != SpellSlot.Unknown)
                {
                    var targets = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(600) && hero.IsEnemy);
                    foreach (var target in targets)
                    {
                        if (Config.IgniteMode == "Combo" && Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            Player.Spellbook.CastSpell(igniteSlot, target);
                        else
                        {
                            double igniteDamage = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
                            if (target.Health < igniteDamage)
                                Player.Spellbook.CastSpell(igniteSlot, target);
                        }
                    }
                }
            }
        }
    }
}
