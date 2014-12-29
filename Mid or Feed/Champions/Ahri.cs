#region

using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Ahri : Plugin
    {
        //TODO: Implment some type of Ult logic.

        public Items.Item Dfg;
        public Spell E;
        public Spell Q;
        public Spell W;
		public Spell R;

        public Ahri()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 1000);
			R = new Spell(SpellSlot.R,1000);

            Q.SetSkillshot(0.25f, 100, 2500, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60, 1500, true, SkillshotType.SkillshotLine);

            Dfg = new Items.Item(3128, 750);

            Game.OnGameUpdate += GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;

            PrintChat("Ahri loaded.");
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var p = Player.Position;

            if (drawQ)
                Utility.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);

            if (drawW)
                Utility.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);

            if (drawE)
                Utility.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
        }

        private void InterrupterOnOnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!GetBool("interruptE") || spell.DangerLevel != InterruptableDangerLevel.High)
                return;

            E.Cast(unit, Packets);
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("gapcloseE"))
                return;

            E.Cast(gapcloser.Sender, Packets);
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var useQ = GetBool("useQ");
            var useW = GetBool("useW");
            var useE = GetBool("useE");
			var useR = GetBool("useR");
            var useDfg = GetBool("useDFG");

            if (useDfg && Dfg.IsReady())
                Dfg.Cast(target);

            if (useE && E.IsReady())
                E.Cast(target);

            if (useQ && Q.IsReady())
                Q.Cast(target);

            if (useW && W.IsReady() && W.InRange(target.ServerPosition))
                W.Cast(Packets);
				
			if (useR && R.IsReady())
				if (OkToUlt())
					R.Cast(Game.CursorPos);
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            if (!GetBool("useQHarass") || !Q.IsReady())
                return;

            E.Cast(target, Packets);
			
			if (!GetBool("useEHarass") || !E.IsReady())
                return;

            E.Cast(target, Packets);
        }

		private bool OkToUlt()
        {
            if (Program.Helper.EnemyTeam.Any(x => x.Distance(ObjectManager.Player) < 500)) //any enemies around me?
                return true;

            Vector3 mousePos = Game.CursorPos;

            var enemiesNearMouse = Program.Helper.EnemyTeam.Where(x => x.Distance(ObjectManager.Player) < R.Range && x.Distance(mousePos) < 650);

            if (enemiesNearMouse.Count() > 0)
            {
                if (IsRActive()) //R already active
                    return true;

                bool enoughMana = ObjectManager.Player.Mana > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ManaCost + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

                if (!GetBool("comboROnlyUserInitiate") || !(Q.IsReady() && E.IsReady()) || !enoughMana) //dont initiate if user doesnt want to, also dont initiate if Q and E isnt ready or not enough mana for QER combo
                    return false;

                var friendsNearMouse = Program.Helper.OwnTeam.Where(x => x.IsMe || x.Distance(mousePos) < 650); //me and friends near mouse (already in fight)

                if (enemiesNearMouse.Count() == 1) //x vs 1 enemy
                {
                    Obj_AI_Hero enemy = enemiesNearMouse.FirstOrDefault();

                    bool underTower = Utility.UnderTurret(enemy);

                    return GetComboDamage(enemy) / enemy.Health >= (underTower ? 1.25f : 1); //if enemy under tower, only initiate if combo damage is >125% of enemy health
                }
                else //fight if enemies low health or 2 friends vs 3 enemies and 3 friends vs 3 enemies, but not 2vs4
                {
                    int lowHealthEnemies = enemiesNearMouse.Count(x => x.Health / x.MaxHealth <= 0.1); //dont count low health enemies

                    float totalEnemyHealth = enemiesNearMouse.Sum(x => x.Health);

                    return friendsNearMouse.Count() - (enemiesNearMouse.Count() - lowHealthEnemies) >= -1 || ObjectManager.Player.Health / totalEnemyHealth >= 0.8;
                }
            }

            return false;
        }
		
		private bool IsRActive()
        {
            return ObjectManager.Player.HasBuff("AhriTumble", true);
        }

        private int GetRStacks()
        {
            BuffInstance tumble = ObjectManager.Player.Buffs.FirstOrDefault(x => x.Name == "AhriTumble");
            return tumble != null ? tumble.Count : 0;
        }
		
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1);

            if (W.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
                dmg += dmg*0.2;
            }
			
			 if (R.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.R);

            if (!Dfg.IsReady()) return (float) dmg;
            dmg += Player.GetItemDamage(target, Damage.DamageItems.Dfg);
            dmg += dmg*0.2;

            return (float) dmg;
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            config.AddItem(new MenuItem("useE", "Use E").SetValue(true));
			config.AddItem(new MenuItem("useR", "Use R").SetValue(true));
			config.AddItem(new MenuItem("comboROnlyUserInitiate", "Use R only if user initiated").SetValue(false));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
			config.AddItem(new MenuItem("useEHarass", "Use E").SetValue(false));
			
        }

        public override void ItemMenu(Menu config)
        {
            config.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("gapcloseE", "E on Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("interruptE", "E to Interrupt", true).SetValue(true));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
        }
    }
}