using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Reflection;
using Color = System.Drawing.Color;

namespace PerplexedNidalee
{
    enum NidaleeSpell
    {
        Javelin,
        Bushwhack,
        PrimalSurge,
        Takedown,
        Pounce,
        Swipe
    }
    class Program
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        static float javelinCD = 6;
        static float[] bushwhackCD = { 13, 12, 11, 10, 9 };
        static float primalSurgeCD = 12;

        static float javelinTime, bushwhackTime, primalSurgeTime;
        static float takedownTime, pounceTime, swipeTime;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Nidalee")
                return;

            if (Updater.Outdated())
            {
                Game.PrintChat("<font color=\"#ff0000\">Perplexed Nidalee is outdated! Please update to {0}!</font>", Updater.GetLatestVersion());
                return;
            }

            SpellManager.Initialize();
            ItemManager.Initialize();
            Config.Initialize();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            Game.PrintChat("<font color=\"#ff3300\">Perplexed Nidalee ({0})</font> - <font color=\"#ffffff\">Loaded!</font>", Version);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.DodgeWithPounce)
            {
                var awayPosition = gapcloser.End.Extend(ObjectManager.Player.ServerPosition, ObjectManager.Player.Distance(gapcloser.End) + SpellManager.Pounce.Range);
                if (!cougarForm())
                    SpellManager.AspectOfTheCougar.Cast();
                SpellManager.CastSpell(SpellManager.Pounce, awayPosition, false);
            }  
        }

        static bool cougarForm()
        {
            return Player.Spellbook.GetSpell(SpellSlot.Q).Name != "JavelinToss";
        }

        static bool isOffCooldown(NidaleeSpell spell)
        {
            switch (spell)
            {
                case NidaleeSpell.Javelin:
                    return (javelinTime - Game.Time) <= 0;
                case NidaleeSpell.Bushwhack:
                    return (bushwhackTime - Game.Time) <= 0;
                case NidaleeSpell.PrimalSurge:
                    return (primalSurgeTime - Game.Time) <= 0;
                case NidaleeSpell.Takedown:
                    return (takedownTime - Game.Time) <= 0;
                case NidaleeSpell.Pounce:
                    return (pounceTime - Game.Time) <= 0;
                case NidaleeSpell.Swipe:
                    return (swipeTime - Game.Time) <= 0;
                default:
                    return false;
            }
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;
            switch(args.SData.Name)
            {
                case "JavelinToss":
                    javelinTime = Game.Time + javelinCD + (javelinCD * Player.PercentCooldownMod);
                    break;
                case "Bushwhack":
                    float bwCD = bushwhackCD[SpellManager.Bushwhack.Level - 1];
                    bushwhackTime = Game.Time + bwCD + (bwCD * Player.PercentCooldownMod);
                    break;
                case "PrimalSurge":
                    primalSurgeTime = Game.Time + primalSurgeCD + (primalSurgeCD * Player.PercentCooldownMod);
                    break;

                case "Takedown":
                    takedownTime = Game.Time + 5 + (5 * Player.PercentCooldownMod);
                    break;
                case "Pounce":
                    pounceTime = Game.Time + 5 + (5 * Player.PercentCooldownMod);
                    break;
                case "Swipe":
                    swipeTime = Game.Time + 5 + (5 * Player.PercentCooldownMod);
                    break;
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            HealIfNeeded();
            SpellManager.IgniteIfPossible();
            SpellManager.UseHealIfInDanger(0);
            ItemManager.CleanseCC();

            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ItemManager.UseOffensiveItems();
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }
        }

        static void HealIfNeeded()
        {
            int manaToHeal = (int)(Player.MaxMana / 100) * Config.HealManaPct;

            if (!Config.EnableHeal || cougarForm() || Player.Mana < manaToHeal || Player.IsRecalling())
                return;
            if(Config.HealSelf)
            {
                int healthToHeal = (int) (Player.MaxHealth / 100) * Config.HealPct;
                if (Player.Health <= healthToHeal && SpellManager.PrimalSurge.IsReady())
                    SpellManager.CastSpell(SpellManager.PrimalSurge, Player, false);
            }
            if (Config.HealAllies)
            {
                var targets = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(SpellManager.PrimalSurge.Range) && hero.IsAlly && hero.IsDead).OrderBy(hero => hero.Health);
                foreach (Obj_AI_Hero target in targets)
                {
                    int healthToHeal = (int)(target.MaxHealth / 100) * Config.HealPct;
                    if (target.Health <= healthToHeal && SpellManager.PrimalSurge.IsReady())
                        SpellManager.CastSpell(SpellManager.PrimalSurge, target, false);
                }
            }
        }

        static void Combo()
        {
            var target = TargetSelector.GetSelectedTarget();
            if(target == null || target.IsValidTarget(SpellManager.Javelin.Range))
                target =  TargetSelector.GetTarget(SpellManager.Javelin.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget(SpellManager.Javelin.Range))
                return;
            if (cougarForm())
            {
                //Cougar combo
                if (Config.ComboTakedown && (isOffCooldown(NidaleeSpell.Takedown) || SpellManager.Takedown.IsReady()))
                    SpellManager.Takedown.Cast();
                if (Config.ComboPounce && (isOffCooldown(NidaleeSpell.Pounce) || SpellManager.Pounce.IsReady()))
                {
                    float pounceRange = target.Hunted() ? 750 : SpellManager.Pounce.Range;
                    if (target.IsValidTarget(pounceRange))
                        SpellManager.CastSpell(SpellManager.Pounce, target.ServerPosition, false);
                }
                if (Config.ComboSwipe && (isOffCooldown(NidaleeSpell.Swipe) || SpellManager.Swipe.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Swipe.Range))
                        SpellManager.CastSpell(SpellManager.Swipe, target.ServerPosition, false);
                }
                if (Config.ComboJavelin && isOffCooldown(NidaleeSpell.Javelin))
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
                //Human combo
            else
            {
                if (Config.ComboJavelin && (isOffCooldown(NidaleeSpell.Javelin) || SpellManager.Javelin.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Javelin.Range))
                        SpellManager.CastSpell(SpellManager.Javelin, target, HitChance.VeryHigh, false);
                }
                if (Config.ComboBushwhack && (isOffCooldown(NidaleeSpell.Bushwhack) || SpellManager.Bushwhack.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Bushwhack.Range))
                        SpellManager.CastSpell(SpellManager.Bushwhack, target.ServerPosition, false);
                }
                if (target.Hunted())
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
        }

        static void Harass()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || target.IsValidTarget(SpellManager.Javelin.Range))
                target = TargetSelector.GetTarget(SpellManager.Javelin.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget(SpellManager.Javelin.Range))
                return;
            if (cougarForm())
            {
                //Cougar harass
                if (Config.HarassTakedown && (isOffCooldown(NidaleeSpell.Takedown) || SpellManager.Takedown.IsReady()))
                    SpellManager.Takedown.Cast();
                if (Config.HarassPounce && (isOffCooldown(NidaleeSpell.Pounce) || SpellManager.Pounce.IsReady()))
                {
                    float pounceRange = target.Hunted() ? 750 : SpellManager.Pounce.Range;
                    if (target.IsValidTarget(pounceRange))
                        SpellManager.CastSpell(SpellManager.Pounce, target.ServerPosition, false);
                }
                if (Config.HarassSwipe && (isOffCooldown(NidaleeSpell.Swipe) || SpellManager.Swipe.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Swipe.Range))
                        SpellManager.CastSpell(SpellManager.Swipe, target.ServerPosition, false);
                }
                if (Config.HarassJavelin && isOffCooldown(NidaleeSpell.Javelin))
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
            else
            {
                //Human harass
                int manaToHarass = (int)(Player.MaxMana / 100) * Config.HealManaPct;
                if (Player.Mana < manaToHarass)
                    return;
                if (Config.HarassJavelin && (isOffCooldown(NidaleeSpell.Javelin) || SpellManager.Javelin.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Javelin.Range))
                        SpellManager.CastSpell(SpellManager.Javelin, target, HitChance.VeryHigh, false);
                }
                if (Config.HarassBushwhack && (isOffCooldown(NidaleeSpell.Bushwhack) || SpellManager.Bushwhack.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Bushwhack.Range))
                        SpellManager.CastSpell(SpellManager.Bushwhack, target.ServerPosition, false);
                }
                if (target.Hunted())
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
        }

        static void LaneClear()
        {
            var target = MinionManager.GetMinions(Player.ServerPosition, SpellManager.Javelin.Range, MinionTypes.All, MinionTeam.Enemy).Where(minion => minion.IsValidTarget(SpellManager.Javelin.Range)).OrderBy(minion => minion.Health).FirstOrDefault();
            if (!target.IsValidTarget(SpellManager.Javelin.Range))
                return;
            if (cougarForm())
            {
                //Cougar clear
                if (Config.LaneTakedown && (isOffCooldown(NidaleeSpell.Takedown) || SpellManager.Takedown.IsReady()))
                    SpellManager.Takedown.Cast();
                if (Config.LanePounce && (isOffCooldown(NidaleeSpell.Pounce) || SpellManager.Pounce.IsReady()))
                {
                    float pounceRange = target.Hunted() ? 750 : SpellManager.Pounce.Range;
                    if (target.IsValidTarget(pounceRange))
                        SpellManager.CastSpell(SpellManager.Pounce, target.ServerPosition, false);
                }
                if (Config.LaneSwipe && (isOffCooldown(NidaleeSpell.Swipe) || SpellManager.Swipe.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Swipe.Range))
                        SpellManager.CastSpell(SpellManager.Swipe, target.ServerPosition, false);
                }
                if (Config.LaneJavelin && isOffCooldown(NidaleeSpell.Javelin))
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
            else
            {
                //Human clear
                int manaToClear = (int)(Player.MaxMana / 100) * Config.LaneManaPct;
                if (Player.Mana < manaToClear)
                    return;
                if (Config.LaneJavelin && (isOffCooldown(NidaleeSpell.Javelin) || SpellManager.Javelin.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Javelin.Range))
                        SpellManager.CastSpell(SpellManager.Javelin, target, HitChance.High, false);
                }
                if (Config.LaneBushwhack && (isOffCooldown(NidaleeSpell.Bushwhack) || SpellManager.Bushwhack.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Bushwhack.Range))
                        SpellManager.CastSpell(SpellManager.Bushwhack, target.ServerPosition, false);
                }
                if (target.Hunted())
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
        }

        static void JungleClear()
        {
            var target = MinionManager.GetMinions(Player.ServerPosition, SpellManager.Javelin.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (!target.IsValidTarget(SpellManager.Javelin.Range))
                return;
            if (cougarForm())
            {
                //Cougar clear
                if (Config.JungleTakedown && (isOffCooldown(NidaleeSpell.Takedown) || SpellManager.Takedown.IsReady()))
                    SpellManager.Takedown.Cast();
                if (Config.JunglePounce && (isOffCooldown(NidaleeSpell.Pounce) || SpellManager.Pounce.IsReady()))
                {
                    float pounceRange = target.Hunted() ? 750 : SpellManager.Pounce.Range;
                    if (target.IsValidTarget(pounceRange))
                        SpellManager.CastSpell(SpellManager.Pounce, target.ServerPosition, false);
                }
                if (Config.JungleSwipe && (isOffCooldown(NidaleeSpell.Swipe) || SpellManager.Swipe.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Swipe.Range))
                        SpellManager.CastSpell(SpellManager.Swipe, target.ServerPosition, false);
                }
                if (Config.JungleJavelin && isOffCooldown(NidaleeSpell.Javelin))
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
            else
            {
                //Human clear
                int manaToClear = (int)(Player.MaxMana / 100) * Config.JungleManaPct;
                if (Player.Mana < manaToClear)
                    return;
                if (Config.JungleJavelin && (isOffCooldown(NidaleeSpell.Javelin) || SpellManager.Javelin.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Javelin.Range))
                        SpellManager.CastSpell(SpellManager.Javelin, target, HitChance.High, false);
                }
                if (Config.JungleJavelin && (isOffCooldown(NidaleeSpell.Bushwhack) || SpellManager.Bushwhack.IsReady()))
                {
                    if (target.IsValidTarget(SpellManager.Bushwhack.Range))
                        SpellManager.CastSpell(SpellManager.Bushwhack, target.ServerPosition, false);
                }
                if (target.Hunted())
                {
                    if (SpellManager.AspectOfTheCougar.IsReady())
                        SpellManager.AspectOfTheCougar.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if(Config.DrawJavelin.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.Javelin.Range, Config.DrawJavelin.Color);
        }
    }
}
