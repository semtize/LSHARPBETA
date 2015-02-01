#region

/*
 * Credits to:
 * Eskor
 * Roach_
 * Both for helping me alot doing this Assembly and start On L# 
 * lepqm for cleaning my shit up
 * iMeh Code breaker 101
 */
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Annie
{
    internal class Program
    {
        public const string CharName = "Annie";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        public static float DoingCombo;
        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;
        public static Menu Config;

        private static int StunCount
        {
            get
            {
                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => buff.Name == "pyromania" || buff.Name == "pyromania_particle"))
                {
                    switch (buff.Name)
                    {
                        case "pyromania":
                            return buff.Count;
                        case "pyromania_particle":
                            return 4;
                    }
                }

                return 0;
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600f);
            R1 = new Spell(SpellSlot.R, 900f);

            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.60f, 50f * (float) Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.20f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R1.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(R);
            SpellList.Add(R1);

            Config = new Menu(CharName, CharName, true);

            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);

            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Combo settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("qCombo", "Use Q")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("wCombo", "Use W")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("rCombo", "Use R")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("itemsCombo", "Use Items")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("flashCombo", "Targets needed to Flash -> R(stun)")).SetValue(new Slider(4, 5, 1));
			
			Config.AddSubMenu(new Menu("Flash Combo", "FlashCombo"));
            Config.SubMenu("FlashCombo").AddItem(new MenuItem("FlashComboKey", "FlashCombo!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("FlashCombo").AddItem(new MenuItem("FlashComboMinEnemies", "FlashCombo Min Enemies Hit").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("FlashCombo").AddItem(new MenuItem("FlashAntiSuicide", "Use Flash Anti Suicide").SetValue(true));

            Config.AddSubMenu(new Menu("Harass(Mixed Mode) settings", "harass"));
            Config.SubMenu("harass")
                .AddItem(new MenuItem("qFarmHarass", "Last hit with Disintegrate (Q)").SetValue(true));
            Config.SubMenu("harass").AddItem(new MenuItem("qHarass", "Harass with Q")).SetValue(true);
            Config.SubMenu("harass").AddItem(new MenuItem("wHarass", "Harass with W")).SetValue(true);

            Config.AddSubMenu(new Menu("Farm Settings", "lasthit"));
            Config.SubMenu("lasthit").AddItem(new MenuItem("qFarm", "Last hit with Disintegrate (Q)").SetValue(true));
            Config.SubMenu("lasthit").AddItem(new MenuItem("wFarm", "Lane Clear with Incinerate (W)").SetValue(true));
            Config.SubMenu("lasthit")
                .AddItem(new MenuItem("saveqStun", "Don't Last Hit with Q while stun is up").SetValue(true));
            Config.AddSubMenu(new Menu("Draw Settings", "draw"));

            Config.AddSubMenu(new Menu("Misc", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("PCast", "Packet Cast Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("autoShield", "Auto shield agaisnt AAs").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("suppMode", "Support mode").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("FountainPassive", "Charge Stun in Fountain").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("LanePassive", "Charge Stun In Lane").SetValue(true));
            Config.SubMenu("misc")
                .AddItem(new MenuItem("LanePassivePercent", "Min Mana % to Charge").SetValue(new Slider(60)));

            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("QDraw", "Draw Disintegrate (Q) Range").SetValue(
                        new Circle(true, Color.FromArgb(128, 178, 0, 0))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("WDraw", "Draw Incinerate (W) Range").SetValue(
                        new Circle(false, Color.FromArgb(128, 32, 178, 170))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("RDraw", "Draw Tibbers (R) Range").SetValue(
                        new Circle(true, Color.FromArgb(128, 128, 0, 128))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("R1Draw", "Draw Flash -> R combo Range").SetValue(
                        new Circle(true, Color.FromArgb(128, 128, 0, 128))));
			Config.SubMenu("draw").AddItem(new MenuItem("ComboDamage", "Drawings on HPBar").SetValue(true));


            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObject;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;

            Game.PrintChat("Annie# Loaded");
			
			Config.Item("ComboDamage").ValueChanged += (object sender, OnValueChangeEventArgs e) => { Utility.HpBarDamageIndicator.Enabled = e.GetNewValue<bool>(); };
            if (Config.Item("ComboDamage").GetValue<bool>())
            {
                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = true;
            }
        }
		
		private static float GetComboDamage(Obj_AI_Hero enemy)
			{
				IEnumerable<SpellSlot> spellCombo = new[] { SpellSlot.Q, SpellSlot.W };
				if (StunCount >= 4)
					spellCombo = spellCombo.Concat(new[] { SpellSlot.Q });
				if (R.IsReady())
					spellCombo = spellCombo.Concat(new[] { SpellSlot.R });
	
				return (float)ObjectManager.Player.GetComboDamage(enemy, spellCombo);
			}
			
        private static void OnDraw(EventArgs args)
        {
            // Utility.DrawCircle(R1.GetPrediction(SimpleTs.GetTarget(900, SimpleTs.DamageType.Magical)).CastPosition, 250,
            //     Color.Aquamarine);
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Draw").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly || !(sender is Obj_SpellMissile) || !Config.Item("autoShield").GetValue<bool>())
            {
                return;
            }

            var missile = (Obj_SpellMissile) sender;
            if (!(missile.SpellCaster is Obj_AI_Hero) || !(missile.Target.IsMe))
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast();
            }
            else if (!ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId).IsMelee())
            {
                var ecd = (int) (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time) *
                          1000;
                if ((int) Vector3.Distance(missile.Position, ObjectManager.Player.ServerPosition) /
                    ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId)
                        .BasicAttack.MissileSpeed * 1000 > ecd)
                {
                    Utility.DelayAction.Add(ecd, () => E.Cast(ObjectManager.Player, true));
                }
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var flashRtarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            ChargeStun();
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Orbwalker.SetAttack(false);
                    Combo(target, flashRtarget);
                    Orbwalker.SetAttack(true);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (Config.Item("suppMode").GetValue<bool>())
                    {
                        Farm(false);
                    }
                    Harass(target);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm(false);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm(true);
                    break;
            }
        }

        private static void ChargeStun()
        {
            if (StunCount == 4 || ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Config.Item("FountainPassive").GetValue<bool>() && ObjectManager.Player.InFountain())
            {
                if (E.IsReady())
                {
                    E.Cast();
                    return;
                }

                if (W.IsReady())
                {
                    W.Cast(Game.CursorPos);
                }
                return;
            }

            if (Config.Item("LanePassive").GetValue<bool>() && E.IsReady() &&
                ObjectManager.Player.ManaPercentage() >= Config.Item("LanePassivePercent").GetValue<Slider>().Value)
            {
                E.Cast();
            }
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount > DoingCombo;
        }

        private static void Harass(Obj_AI_Base target)
        {
            if (Config.Item("qHarass").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast(target, Config.Item("PCast").GetValue<bool>());
            }
            if (Config.Item("wHarass").GetValue<bool>() && W.IsReady())
            {
                W.Cast(target, Config.Item("PCast").GetValue<bool>());
            }
        }

		private static double GetBurstComboDamage(Obj_AI_Hero target)
        {
            double totalComboDamage = 0;
            totalComboDamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
            totalComboDamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            totalComboDamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);

            if (ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                totalComboDamage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            return totalComboDamage;
        }

		public static void FlashCombo()
        {
            var UseFlashCombo = Config.Item("FlashComboKey").GetValue<KeyBind>().Active;
            var FlashComboMinEnemies = Config.Item("FlashComboMinEnemies").GetValue<Slider>().Value;
            var FlashAntiSuicide = Config.Item("FlashAntiSuicide").GetValue<bool>();

            if (!UseFlashCombo)
                return;
				
            if (((StunCount == 3 && E.IsReady()) || StunCount == 4) && (ObjectManager.Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready) && R.IsReady())
            {
                var allEnemies = DevHelper.GetEnemyList()
                    .Where(x => ObjectManager.Player.Distance(x) > R.Range && ObjectManager.Player.Distance(x) < R.Range + 500);

                var enemies = DevHelper.GetEnemyList()
                    .Where(x => ObjectManager.Player.Distance(x) > R.Range && ObjectManager.Player.Distance(x) < R.Range + 400 && GetBurstComboDamage(x) * 0.9 > x.Health)
                    .OrderBy(x => x.Health);

                bool isSuicide = FlashAntiSuicide ? allEnemies.Count() - enemies.Count() > 2 : false;

                if (enemies.Any() && !isSuicide)
                { 
                    var enemy = enemies.First();
                    if (DevHelper.CountEnemyInPositionRange(enemy.ServerPosition, 250) >= FlashComboMinEnemies)
                    {
                        var predict = R.GetPrediction(enemy, true).CastPosition;

                        if (StunCount == 3)
                        {
                            E.Cast();
                        }

                        ObjectManager.Player.Spellbook.CastSpell(FlashSlot, predict);

                        if (R.IsReady())
                            R.Cast(predict);

                        if (W.IsReady())
                            W.Cast(predict);

                        if (E.IsReady())
                            E.Cast();

                    }
                }
            }
        }
		
        private static void Combo(Obj_AI_Base target, Obj_AI_Base flashRtarget)
        {
            if ((target == null && flashRtarget == null) || Environment.TickCount < DoingCombo ||
                (!Q.IsReady() && !W.IsReady() && !R.IsReady()))
            {
                return;
            }
            if (Config.Item("itemsCombo").GetValue<bool>() && target != null)
            {
                Items.UseItem(3128, target);
            }


            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useW = Config.Item("wCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();
            switch (StunCount)
            {
                case 3:
                    if (target == null)
                    {
                        return;
                    }
                    if (Q.IsReady() && useQ)
                    {
                        DoingCombo = Environment.TickCount;
                        Q.Cast(target, Config.Item("PCast").GetValue<bool>());
                        Utility.DelayAction.Add(
                            (int) (ObjectManager.Player.Distance(target, false) / Q.Speed * 1000 - Game.Ping / 2.0) +
                            250, () =>
                            {
                                if (R.IsReady() &&
                                    !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health))
                                {
                                    R.Cast(target, false, true);
                                }
                            });
                    }
                    else if (W.IsReady() && useW)
                    {
                        //W.Cast(target);
						 W.CastIfHitchanceEquals(target, target.IsMoving ? HitChance.High : HitChance.Medium);
                        DoingCombo = Environment.TickCount + 250f;
                    }


                    break;
                case 4:
                    if (ObjectManager.Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready && R.IsReady() &&
                        target == null)
                    {
                        var position = R1.GetPrediction(flashRtarget, true).CastPosition;

                        if (ObjectManager.Player.Distance(position) > 600 &&
                            GetEnemiesInRange(flashRtarget.ServerPosition, 250) >=
                            Config.Item("flashCombo").GetValue<Slider>().Value)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(FlashSlot, position);
                        }

                        Items.UseItem(3128, flashRtarget);
                        R.Cast(flashRtarget, false, true);

                        if (W.IsReady() && useW)
                        {
                            W.Cast(flashRtarget, false, true);
                        }
                        else if (Q.IsReady() && useQ)
                        {
                            Q.Cast(flashRtarget, Config.Item("PCast").GetValue<bool>());
                        }
                    }
                    else if (target != null)
                    {
                        if (R.IsReady() && useR &&
                            !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                        {
                            R.Cast(target, false, true);
                        }

                        if (W.IsReady() && useW)
                        {
                            W.Cast(target, false, true);
                        }

                        if (Q.IsReady() && useQ)
                        {
                            Q.Cast(target, Config.Item("PCast").GetValue<bool>());
                        }
                    }
                    break;
                default:
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(target, Config.Item("PCast").GetValue<bool>());
                    }

                    if (W.IsReady() && useW)
                    {
                        W.CastIfHitchanceEquals(target, target.IsMoving ? HitChance.High : HitChance.Medium);
						W.Cast(target, false, true);
                    }

                    break;
            }

            if (IgniteSlot != SpellSlot.Unknown && target != null &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(target, false) < 600 &&
                ObjectManager.Player.GetSpellDamage(target, IgniteSlot) > target.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            }
        }

        private static void Farm(bool laneclear)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var jungleMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
            minions.AddRange(jungleMinions);

            if (laneclear && Config.Item("wFarm").GetValue<bool>() && W.IsReady())
            {
                if (minions.Count > 0)
                {
                    W.Cast(W.GetLineFarmLocation(minions).Position.To3D());
                }
            }
            if (((!Config.Item("qFarm").GetValue<bool>() ||
                  !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LastHit)) &&
                 (!Config.Item("qFarmHarass").GetValue<bool>() ||
                  !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Mixed)) &&
                 !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LaneClear)) ||
                Config.Item("saveqStun").GetValue<bool>() && StunCount == 4 || !Q.IsReady())
            {
                return;
            }
            foreach (var minion in
                from minion in
                    minions.OrderByDescending(Minions => Minions.MaxHealth)
                        .Where(minion => minion.IsValidTarget(Q.Range))
                let predictedHealth = Q.GetHealthPrediction(minion)
                where
                    predictedHealth < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) * 0.85 &&
                    predictedHealth > 0
                select minion)
            {
                Q.CastOnUnit(minion, Config.Item("PCast").GetValue<bool>());
            }
        }

        private static int GetEnemiesInRange(Vector3 pos, float range)
        {
            //var Pos = pos;
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.Team != ObjectManager.Player.Team)
                    .Count(hero => Vector3.Distance(pos, hero.ServerPosition) <= range);
        }
    }
	public static class DevHelper
    {

        public static List<Obj_AI_Hero> GetEnemyList()
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid)
                .OrderBy(x => ObjectManager.Player.ServerPosition.Distance(x.ServerPosition))
                .ToList();
        }

        public static List<Obj_AI_Hero> GetAllyList()
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsAlly && x.IsValid)
                .OrderBy(x => ObjectManager.Player.ServerPosition.Distance(x.ServerPosition))
                .ToList();
        }

        public static Obj_AI_Hero GetNearestEnemy(this Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid && x.NetworkId != unit.NetworkId)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        public static Obj_AI_Hero GetNearestAlly(this Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsAlly && x.IsValid && x.NetworkId != unit.NetworkId)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        public static Obj_AI_Hero GetNearestEnemyFromUnit(this Obj_AI_Base unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        public static float GetHealthPerc(this Obj_AI_Base unit)
        {
            return (unit.Health / unit.MaxHealth) * 100;
        }

        public static float GetManaPerc(this Obj_AI_Base unit)
        {
            return (unit.Mana / unit.MaxMana) * 100;
        }

        public static void SendMovePacket(this Obj_AI_Base v, Vector2 point)
        {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(point.X, point.Y)).Send();
        }

        public static bool IsUnderEnemyTurret(this Obj_AI_Base unit)
        {
            IEnumerable<Obj_AI_Turret> query;

            if (unit.IsEnemy)
            {
                query = ObjectManager.Get<Obj_AI_Turret>()
                    .Where(x => x.IsAlly && x.IsValid && !x.IsDead && unit.ServerPosition.Distance(x.ServerPosition) < 950);
            }
            else
            {
                query = ObjectManager.Get<Obj_AI_Turret>()
                    .Where(x => x.IsEnemy && x.IsValid && !x.IsDead && unit.ServerPosition.Distance(x.ServerPosition) < 950);
            }

            return query.Any();
        }

        public static void Ping(Vector3 pos)
        {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos.X, pos.Y, 0, 0, Packet.PingType.Normal)).Process();
        }

        public static float GetDistanceSqr(Obj_AI_Base source, Obj_AI_Base target)
        {
            return Vector2.DistanceSquared(source.ServerPosition.To2D(), target.ServerPosition.To2D());
        }

        //public static bool IsFacing(this Obj_AI_Base source, Obj_AI_Base target)
        //{
        //    if (!source.IsValid || !target.IsValid)
        //        return false;

        //    if (source.Path.Count() > 0 && source.Path[0].Distance(target.ServerPosition) < target.Distance(source))
        //        return true;
        //    else
        //        return false;
        //}

        public static bool IsKillable(this Obj_AI_Hero source, Obj_AI_Base target, IEnumerable<SpellSlot> spellCombo)
        {
            return Damage.GetComboDamage(source, target, spellCombo) * 0.9 > target.Health;
        }

        public static int CountEnemyInPositionRange(Vector3 position, float range)
        {
            return GetEnemyList().Where(x => x.ServerPosition.Distance(position) <= range).Count();
        }

        private static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };
        private static readonly string[] NoAttacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        private static readonly string[] Attacks = { "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };

        public static bool IsAutoAttack(string spellName)
        {
            return (spellName.ToLower().Contains("attack") && !NoAttacks.Contains(spellName.ToLower())) || Attacks.Contains(spellName.ToLower());
        }

        public static bool IsMinion(AttackableUnit unit, bool includeWards = false)
        {
            if (unit is Obj_AI_Minion)
            {
                var minion = unit as Obj_AI_Minion;
                var name = minion.BaseSkinName.ToLower();
                return name.Contains("minion") || (includeWards && (name.Contains("ward") || name.Contains("trinket")));
            }
            else
                return false;
        }

        public static float GetRealDistance(GameObject unit, GameObject target)
        {
            return unit.Position.Distance(target.Position) + unit.BoundingRadius + target.BoundingRadius;
        }
    }
}