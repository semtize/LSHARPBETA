﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Tibbuhs
{
    class Program
    {
        #region Definitions
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static SpellSlot Flash;
        private static SpellSlot Ignite;
        private static SpellSlot Exhaust; //not used
        private static PredictionInput FlashTibbers_pi;
        private static PredictionOutput FlashTibbers_po;
        private static Menu menu;
        private static Orbwalking.Orbwalker orbw;
        private static TargetSelector ts;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Vector3 PredictedTibbers;
        private static Items.Item DFG, ZHONYA;
        #endregion
        static void Main(string[] args)
        {
            if (Player.BaseSkinName != "Annie") return;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region OnGameLoad
        private static void Game_OnGameLoad(EventArgs args)
        {
            #region Spells
            Q = new Spell(SpellSlot.Q, 650);
            Q.SetTargetted(250, 1400);
            W = new Spell(SpellSlot.W, 625);
            W.SetSkillshot(600, (float)(50 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            R = new Spell(SpellSlot.R, 600);
            R.SetSkillshot(250, 200, float.MaxValue, false, SkillshotType.SkillshotCircle);
            /* nvm
            if (SpellSlot.Summoner1.ToString().ToLower().Contains("flash")) Flash = SpellSlot.Summoner1;
            if (SpellSlot.Summoner1.ToString().ToLower().Contains("dot")) Ignite = SpellSlot.Summoner1;
            if (SpellSlot.Summoner1.ToString().ToLower().Contains("exhaust")) Exhaust = SpellSlot.Summoner1;
            if (SpellSlot.Summoner2.ToString().ToLower().Contains("flash")) Flash = SpellSlot.Summoner2;
            if (SpellSlot.Summoner2.ToString().ToLower().Contains("dot")) Ignite = SpellSlot.Summoner2;
            if (SpellSlot.Summoner2.ToString().ToLower().Contains("exhaust")) Exhaust = SpellSlot.Summoner2;
             */
            Flash = Player.GetSpellSlot("SummonerFlash", true);
            Ignite = Player.GetSpellSlot("SummonerDot", true);
            Exhaust = Player.GetSpellSlot("SummonerExhaust", true);
            SpellList.Add(Q); SpellList.Add(W); SpellList.Add(E); SpellList.Add(R);
            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
            ZHONYA = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3090, float.MaxValue) : new Items.Item(3157, float.MaxValue);
            #endregion

            #region Menu
            menu = new Menu("Tibbers is SoSharp", "tibbuhs", true);

            menu.AddSubMenu(new Menu("Orbwalker", "orbw"));
            orbw = new Orbwalking.Orbwalker(menu.SubMenu("orbw"));

            menu.AddSubMenu(new Menu("Target Selector", "ts"));
            TargetSelector.AddToMenu(menu.SubMenu("ts"));

            menu.AddSubMenu(new Menu("Laning settings", "farm"));
            menu.AddSubMenu(new Menu("Teamfight settings", "combo"));
            menu.AddSubMenu(new Menu("Defensive settings", "defense"));
            menu.AddSubMenu(new Menu("Drawings", "drawings"));
            menu.AddSubMenu(new Menu("Misc settings", "misc"));

            menu.SubMenu("farm").AddItem(new MenuItem("Qlasthit", "Use Q Lasthit").SetValue(true));
            menu.SubMenu("farm").AddItem(new MenuItem("Qstunlasthit", "Use Q stun to Lasthit").SetValue(false));
            menu.SubMenu("farm").AddItem(new MenuItem("Qharass", "Use Q Harass").SetValue(true));
            menu.SubMenu("farm").AddItem(new MenuItem("Qstunharass", "Use Q stun to Harass").SetValue(true));
            menu.SubMenu("farm").AddItem(new MenuItem("AAharass", "Use AA Harass when target stunned").SetValue(true));
            menu.SubMenu("farm").AddItem(new MenuItem("Wharass", "Use W Harass when target stunned").SetValue(true));
            menu.SubMenu("farm").AddItem(new MenuItem("Qharassmana", "Use Q to harass when %mana more than").SetValue(new Slider(25, 0, 100)));
            menu.SubMenu("farm").AddItem(new MenuItem("Elanestuncharge", "Charge E stun in lane").SetValue(true));

            menu.SubMenu("combo").AddItem(new MenuItem("Qcombo", "Use Q in teamfights").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("Wcombo", "Use W in teamfights").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("Wcombomin", "Only use W if it will hit X enemies").SetValue(new Slider(1, 1, 5)));
            menu.SubMenu("combo").AddItem(new MenuItem("Wstuncombo", "Only use W if stun ready").SetValue(false));
            menu.SubMenu("combo").AddItem(new MenuItem("Ecombostuncharge", "Charge E stun in teamfights").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("Rifcantkill", "Summon tibbers even if full combo can't kill").SetValue(false));
            menu.SubMenu("combo").AddItem(new MenuItem("RcomboOnlyOn4Stacks", "Only summon Tibbers if can stun").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("FlashTibbers", "Flash-Tibbers to stun").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("FlashTibbersmin", "Flash-Tibbers only if it will hit X enemies").SetValue(new Slider(3,1,5)));
            menu.SubMenu("combo").AddItem(new MenuItem("UseZHONYA", "Use Zhonya on Low-health No spells ready").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("ZHONYAminhealth", "Zhonya on %hp").SetValue(new Slider(20, 0, 100)));



            menu.SubMenu("defense").AddItem(new MenuItem("Wstunescape", "Use W stun to escape").SetValue(true));
            menu.SubMenu("defense").AddItem(new MenuItem("Wstunescaperange", "Max W stun escape range").SetValue(new Slider(250, 0, (int)W.Range)));
            menu.SubMenu("defense").AddItem(new MenuItem("Eenemies", "Use E when enemies near you").SetValue(true));
            menu.SubMenu("defense").AddItem(new MenuItem("Eenemiesrange", "Use E when enemies closer than").SetValue(new Slider(600, 0, 1250)));

            menu.SubMenu("drawings").AddItem(new MenuItem("drawQ", "Draw Q range").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawW", "Draw W range").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawR", "Draw R range").SetValue(true));
            menu.SubMenu("drawings").AddItem(new MenuItem("drawDMG", "Draw full-combo DMG").SetValue(true));


            menu.SubMenu("misc").AddItem(new MenuItem("passivestacker", "Always stack passive with E").SetValue(false));
            menu.SubMenu("misc").AddItem(new MenuItem("FlashTibbersanytime", "Flash-Tibbers anytime it is possible").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("FlashTibbersanytimemin", "Min targets for Flash-Tibbers anytime").SetValue(new Slider(3, 1, 5)));
            menu.SubMenu("misc").AddItem(new MenuItem("autotibbers", "AutoTibbers on flash").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("tibbersinterrupt", "Use Tibbers to interrupt dangerous spells").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("AntiGapcloser", "Anti-Gapcloser").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("UseMarksmanPotionManager", "Use Marksman# Potion Manager").SetValue(false));
            menu.SubMenu("misc").AddItem(new MenuItem("packets", "Use Packets").SetValue(true));

            menu.AddToMainMenu();

            #endregion

            #region Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            //Game.PrintChat("Tibbers is SoSharp v.{0} loaded!", Assembly.GetExecutingAssembly().GetName().Version);
            #endregion
        }
        #endregion

        #region OnGameUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (orbw.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || orbw.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || orbw.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Laning();
            }
            if (orbw.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            PassiveStacker();
            if (menu.Item("UseMarksmanPotionManager").GetValue<bool>())
            {
                var Extras = new Menu("Extras", "Extras", false);
                new PotionManager(Extras);
                menu.AddSubMenu(Extras);
            }
        }
        #endregion

        #region OnDraw
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("drawQ").GetValue<bool>()) Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);
            if (menu.Item("drawW").GetValue<bool>()) Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Green);
            if (menu.Item("drawR").GetValue<bool>()) Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Blue);
        }
        #endregion

        #region OnPossibleToInterrupt
        private static void OnPossibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (!target.IsEnemy)
                return;
            if (GetPassiveStacks() == 4)
            {
                if (Q.IsReady())
                {
                    Q.Cast(target, UsePackets());
                }
                else if (W.IsReady() && W.InRange(target.Position))
                {
                    W.Cast(target, UsePackets());
                }
                else if (spell.DangerLevel == InterruptableDangerLevel.High && menu.Item("tibbersinterrupt").GetValue<bool>())
                {
                    R.Cast(target, UsePackets());
                }
            }
            if (GetPassiveStacks() == 3)
            {
                if (E.IsReady()) E.Cast(UsePackets());
                if (GetPassiveStacks() == 4)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target, UsePackets());
                    }
                    else if (W.IsReady() && W.InRange(target.Position))
                    {
                        W.Cast(target, UsePackets());
                    }
                    else if (spell.DangerLevel == InterruptableDangerLevel.High && menu.Item("tibbersinterrupt").GetValue<bool>())
                    {
                        R.Cast(target, UsePackets());
                    }
                }
            }
            if (GetPassiveStacks() == 2)
            {
                if (E.IsReady() && Q.IsReady()) 
                {
                    E.Cast(UsePackets());
                    Q.Cast(target, UsePackets());
                }
                if (GetPassiveStacks() == 4)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target, UsePackets());
                    }
                    else if (W.IsReady() && W.InRange(target.Position))
                    {
                        W.Cast(target, UsePackets());
                    }
                    else if (spell.DangerLevel == InterruptableDangerLevel.High && menu.Item("tibbersinterrupt").GetValue<bool>())
                    {
                        R.Cast(target, UsePackets());
                    }
                }
            }
        }
        #endregion

        #region AntiGapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var target = gapcloser.Sender;
            if (target.IsAlly || target.IsMe || !(menu.Item("AntiGapcloser").GetValue<bool>())) return;
            if (GetPassiveStacks() == 4)
            {
                if (Q.IsReady())
                {
                    Q.Cast(gapcloser.Sender, UsePackets());
                }
                else if (W.IsReady() && W.InRange(gapcloser.Sender.Position))
                {
                    W.Cast(gapcloser.Sender, UsePackets());
                }
            }
            if (GetPassiveStacks() == 3)
            {
                if (E.IsReady()) E.Cast(UsePackets());
                if (GetPassiveStacks() == 4)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(gapcloser.Sender, UsePackets());
                    }
                    else if (W.IsReady() && W.InRange(gapcloser.Sender.Position))
                    {
                        W.Cast(gapcloser.Sender, UsePackets());
                    }
                }
            }
        }
        #endregion



        #region Laning
        private static void Laning()
        {
            var target = orbw.GetTarget() as Obj_AI_Base;
            if (target == null) Game.PrintChat("Target null! tell sosharp");
            Cast_Q("farm", target);
            Cast_E("farm");
        }
        #endregion

        #region Combo
        private static void Combo()
        {


            Cast_E("combo");
            var combotarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (combotarget == null) Game.PrintChat("combo target null! tell sosharp");
            var combodmg = ComboDmg(combotarget);
            if (combodmg >= combotarget.Health)
            {
                Cast_R();
                Cast_Q("combo", combotarget);
                Cast_W(combotarget);
                DFG.Cast(combotarget);
                Player.Spellbook.CastSpell(Ignite);
            }
            else
            {
                Cast_Q("combo", qtarget);
                Cast_W(wtarget);
            }
            if (menu.Item("Rifcantkill").GetValue<bool>())
            {
                R.Cast(combotarget, UsePackets());
            }
            if (menu.Item("FlashTibbersanytime").GetValue<bool>())
            {
                FlashTibbers_pi.Aoe = true; FlashTibbers_pi.Collision = false; FlashTibbers_pi.Delay = 250; FlashTibbers_pi.Range = 1000; FlashTibbers_pi.Speed = float.MaxValue; FlashTibbers_pi.Type = SkillshotType.SkillshotCircle; FlashTibbers_pi.Radius = 100;
                FlashTibbers_po = Prediction.GetPrediction(FlashTibbers_pi);
                var flashtibbers_hitcount = FlashTibbers_po.AoeTargetsHitCount;
                var flashtibbers_hitchance = FlashTibbers_po.Hitchance;
                var flashtibbers_targetpos = FlashTibbers_po.UnitPosition;
                if (GetPassiveStacks() == 4 && Player.Spellbook.CanUseSpell(Flash) == SpellState.Ready && flashtibbers_hitcount > menu.Item("FlashTibbersanytimemin").GetValue<int>() && flashtibbers_hitchance >= HitChance.Medium && Player.Distance(FlashTibbers_po.UnitPosition) > R.Range)
                {
                    Player.Spellbook.CastSpell(Flash, flashtibbers_targetpos);
                    R.Cast(flashtibbers_targetpos, UsePackets());
                }
            }
        }
        #endregion



        #region Passive
        private static bool PassiveStacker()
        {
            if(Utility.InFountain())
            {
                if (GetPassiveStacks() < 4)
                {
                    if (W.IsReady()) W.Cast();
                }
                if (GetPassiveStacks() < 4)
                {
                    if (E.IsReady()) E.Cast();
                }
            }

            if(menu.Item("passivestacker").GetValue<bool>())
            {
                if (E.IsReady()) E.Cast();
                return true;
            }
            else return false;
        }
        #endregion

        #region Casting Q
        private static void Cast_Q(string mode, Obj_AI_Base target)
        {
            #region Q Farm Mode
            if (mode == "farm")
            {
                if (GetPassiveStacks() == 4 && !menu.Item("Qstunlasthit").GetValue<bool>())
                {
                    if (target != null)
                    {
                        Q.Cast(target, UsePackets());
                        if (Orbwalking.CanAttack() && menu.Item("AAharass").GetValue<bool>())
                        {
                            orbw.ForceTarget(target);
                        }
                        if (W.InRange(target.Position) && menu.Item("Wharass").GetValue<bool>()) 
                        {
                            W.Cast(target);
                        }
                    }
                }
                var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly).ToList();
                var minionLastHit = allMinions.Where(x => HealthPrediction.LaneClearHealthPrediction(x, (int)Q.Delay) < Player.GetSpellDamage(x, SpellSlot.Q) * 0.8).OrderBy(x => x.Health);
                var enoughmanatoharass = menu.Item("Qharassmana").GetValue<int>();
                if (!minionLastHit.Any() && menu.Item("Qstunharass").GetValue<bool>() && HaveMana(enoughmanatoharass))
                {
                    if (target != null)
                    {
                        Q.Cast(target, UsePackets());
                        if (Orbwalking.CanAttack())
                        {
                            orbw.ForceTarget(target);
                        }                        
                    }
                }
                var unit = minionLastHit.First();
                Q.CastOnUnit(unit, UsePackets());
            }
            #endregion
            #region Q Combo Mode
            if (mode == "combo")
            {
                if (menu.Item("Qcombo").GetValue<bool>())
                {
                    if (target != null)
                    {
                        Q.Cast(target, UsePackets());
                    }
                }
            }
            #endregion

        }
        #endregion

        #region Casting W
        private static void Cast_W(Obj_AI_Hero target)
        {
            var Wstunescaperange = menu.Item("Wstungapescaperange").GetValue<int>();
            var closestThreat = ObjectManager.Get<Obj_AI_Hero>().First(ct => !ct.IsMe && ct.Distance(Player, false) < Wstunescaperange && ct.IsEnemy);
            if (menu.Item("combotoggle").GetValue<KeyBind>().Active)
            {
                if (target != null)
                {
                    W.Cast(target, UsePackets());
                }
            }
            if (menu.Item("Wstungapescape").GetValue<bool>())
            {
                W.Cast(closestThreat);
            }
        }
        #endregion

        #region Casting E
        private static void Cast_E(string mode)
        {
            var defenseEenemies = menu.Item("Eenemies").GetValue<bool>();
            var defenseEenemiesrange = menu.Item("Eenemiesrange").GetValue<int>();
            var closestEnemy = ObjectManager.Get<Obj_AI_Hero>().First(ce => !ce.IsMe && ce.Distance(Player, false) < defenseEenemiesrange && ce.IsEnemy);
            if (closestEnemy != null && defenseEenemies && E.IsReady())
            {
                E.Cast();
            }

            if(mode == "farm" && !PassiveStacker() && (menu.Item("Elanestuncharge").GetValue<bool>()))
            {
                if(GetPassiveStacks() < 4)
                {
                    if (E.IsReady()) E.Cast();
                }
            }

            if (mode == "combo" && !PassiveStacker() && (menu.Item("Ecombostuncharge").GetValue<bool>()))
            {
                if (GetPassiveStacks() < 4)
                {
                    if (E.IsReady()) E.Cast();
                }
            }
        }
        #endregion

        #region Casting R
        private static void Cast_R()
        {
            if (menu.Item("FlashTibbers").GetValue<bool>())
            {
                FlashTibbers_pi.Aoe = true; FlashTibbers_pi.Collision = false; FlashTibbers_pi.Delay = 250; FlashTibbers_pi.Range = 1000; FlashTibbers_pi.Speed = float.MaxValue; FlashTibbers_pi.Type = SkillshotType.SkillshotCircle; FlashTibbers_pi.Radius = 100;
                FlashTibbers_po = Prediction.GetPrediction(FlashTibbers_pi);
                var flashtibbers_hitcount = FlashTibbers_po.AoeTargetsHitCount;
                var flashtibbers_hitchance = FlashTibbers_po.Hitchance;
                PredictedTibbers = FlashTibbers_po.UnitPosition;
                if (GetPassiveStacks() == 4 && flashtibbers_hitcount > menu.Item("FlashTibbersmin").GetValue<int>() && flashtibbers_hitchance >= HitChance.Medium && Player.Distance(FlashTibbers_po.UnitPosition) > R.Range)
                {
                    Player.Spellbook.CastSpell(Flash, PredictedTibbers);
                    R.Cast(PredictedTibbers, UsePackets());
                }
                else if (Player.Distance(FlashTibbers_po.UnitPosition) < R.Range)
                {
                    R.Cast(FlashTibbers_po.UnitPosition, UsePackets());
                }
            }
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var minTargets = menu.Item("flashtibbersmin").GetValue<int>();
            if (menu.Item("RcomboOnlyOn4Stacks").GetValue<bool>())
            {
                if (GetPassiveStacks() == 4)
                {
                    R.Cast(target, UsePackets());
                }
                else if (GetPassiveStacks() == 3)
                {
                    E.Cast();
                    if (GetPassiveStacks() == 4)
                    {
                        R.Cast(target, UsePackets());
                    }
                }
            }
            else
            {
                R.Cast(target, UsePackets());
            }
        }
        #region OnProcessSpellCast
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var UseZHONYA = menu.Item("UseZhonya").GetValue<bool>();
            var ZHONYAminhealth = menu.Item("ZHONYAminhealth").GetValue<int>();
            if(!sender.IsMe && args.Target.IsMe && UseZHONYA)
            {
                if (menu.Item("combotoggle").GetValue<KeyBind>().Active && !R.IsReady() && !Q.IsReady() && !W.IsReady() && (Player.Health / Player.MaxHealth * 100 < ZHONYAminhealth) && ZHONYA.IsReady())
                {
                    ZHONYA.Cast();
                }
            }
            #region Endif
            if (sender.IsMe && args.SData.Name == "SummonerFlash")
            {
                var LastFlashTime = Environment.TickCount;
                if (Environment.TickCount - LastFlashTime < 1)
                {
                    if (R.WillHit(PredictedTibbers, Player.Position))
                    {
                        if (menu.Item("autotibbers").GetValue<bool>())
                        {
                            R.Cast(PredictedTibbers, UsePackets());
                        }
                    }
                }
            }
            #endregion
        }
        #endregion
        #endregion

        #region Utility

        private static int GetPassiveStacks()
        {
            var buffs = ObjectManager.Player.Buffs.Where(buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
            var buffInstances = buffs as BuffInstance[] ?? buffs.ToArray();
            if (!buffInstances.Any())
                return 0;
            var buf = buffInstances.First();
            var count = buf.Count >= 4 ? 4 : buf.Count;
            return buf.Name.ToLower() == "pyromania_particle" ? 4 : count;
        }

        private static double SpellDmg(Obj_AI_Hero target, SpellSlot spell)
        {
            var spelldamage = Player.GetSpellDamage(target, spell);
            return spelldamage;
        }

        private static double ComboDmg(Obj_AI_Hero target)
        {
            var combodmg = 0.0d;
            if (Q.IsReady()) combodmg += SpellDmg(target, SpellSlot.Q);
            if (W.IsReady()) combodmg += SpellDmg(target, SpellSlot.W);
            if (R.IsReady()) combodmg += SpellDmg(target, SpellSlot.R);
            if (DFG.IsReady()) combodmg += Player.GetItemDamage(target, Damage.DamageItems.Dfg);
            if (Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready) combodmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            return combodmg;
        }

        private static bool HaveMana(float minMana)
        {
            if (((Player.Mana / Player.MaxMana) * 100) > minMana)
            {
                return true;
            }
            return false;
        }

        private static bool UsePackets()
        {
            if (menu.Item("packets").GetValue<bool>()) return true;
            return false;
        }
        #endregion

    }
}
