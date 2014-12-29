#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Fizz : Plugin
    {
        public Items.Item Dfg;
        public Spell E;
        public Spell Q;
        public Spell W;
		public Spell R;

        public Fizz()
        {
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 400);
			E2 = new Spell(SpellSlot.E, 400);
			R = new Spell(SpellSlot.R, 1200);

			E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);

            Dfg = new Items.Item(3128, 750);

            Game.OnGameUpdate += GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;

            PrintChat("Fizz loaded.");
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawE = GetBool("drawE");
			var drawR = GetBool("drawR");
            var p = Player.Position;

            if (drawQ)
                Utility.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);

            if (drawE)
                Utility.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
				
			if (drawR)
                Utility.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
        }


        private void castEGapclose(Obj_AI_Hero target)
        {
            if (!GetBool("gapcloseE"))
                return;

           // E.Cast(gapcloser.Sender, Packets);
			 if (E.IsReady()) {
                if (jumpStage == FizzJump.PLAYFUL && player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump") {
                    E.Cast(target.ServerPosition, Packets);
                }
            }
            if (jumpStage == FizzJump.TRICKSTER && player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo") {
                E2.Cast(target.ServerPosition, Packets);
            }
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
			if (menu.Item("initiateR").GetValue<KeyBind>().Active) {
                if (player.Distance(target) > Q.Range) {
                    if (R.IsReady()) {
                        R.Cast(target);
                    }
                }
            }
			
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var useQ = GetBool("useQ");
            var useW = GetBool("useW");
            var useE = GetBool("useE");
			var useR = GetBool("useR");
            var useDfg = GetBool("useDFG");
			var useCastEGap = GetBool("castEGap");

            if (useDfg && Dfg.IsReady())
                Dfg.Cast(target);

            if (player.Distance(target) <= Q.Range)
			{
				if (useQwithR && Q.IsReady() && R.IsReady())
				{
					if (useQ && Q.IsReady())
					Q.Cast(target)
					if (useR && R.IsReady())
					{
					R.Cast(target.Position)
					}
				}
				else {
					if (useQ && Q.IsReady())
					Q.Cast(target)
				}
				if (useW && W.IsReady())
				W.Cast(player)
			}
				
         

            if (useE && useCastEGap && E.IsReady()){
                if (player.Distance(target) < 800) {
				if (jumpStage == FizzJump.PLAYFUL && player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump") {
                        E.Cast(target.ServerPosition, true);
                    }

                    if (jumpStage == FizzJump.TRICKSTER && player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo") {
                        E2.Cast(target.ServerPosition, true);
                    }
                }
            }
				
			if (useE && player.Distance(target) > 800 && useCastEGap && E.IsReady())
				 castEGapclose(target);
}

        private void DoHarass()
        {
		//give credit for closest tower
		    Obj_AI_Turret closestTower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly)
                    .OrderBy(tur => tur.Distance(player.Position))
                    .First();
             
			var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            var useQ = GetBool("useQHarass");
            var useW = GetBool("useWHarass");
            var useE = GetBool("eToTower");

            if (useQ)
            {
                Q.Cast(target);
            }

            if (useW)
            {
                W.Cast(player);
            }

            if (useE)
            sendMovementPacket(closestTower.ServerPosition.To2D());
                if (jumpStage == FizzJump.PLAYFUL && player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump") {
                    E.Cast(closestTower.ServerPosition);
                }
                if (jumpStage == FizzJump.TRICKSTER && player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo") {
                    E2.Cast(closestTower.ServerPosition);
                }
        }
	
        public override float GetComboDamage(Obj_AI_Hero target)
        {
             double dmg = 0;

            if (Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.E);

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
            config.AddItem(new MenuItem("initateR", "Initiate with R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
			config.AddItem(new MenuItem("useWHarass", "Use W").SetValue(true));
			config.AddItem(new MenuItem("eToTower", "E back to tower nearby").SetValue(true));
		}

        public override void ItemMenu(Menu config)
        {
            config.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("qWithR", "Use R whilst Q").SetValue(false));
            config.AddItem(new MenuItem("castEGap", "Gapclose with E").SetValue(false));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
        }
		
		private void onSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            //TODO Test the new jumpstage detection method

            if (sender.IsMe) {
                if (args.SData.Name == "FizzJump") {
                    jumpStage = FizzJump.TRICKSTER;
                    time = Game.Time;
                    isCalled = false;
                }
            }
		}
		
		private enum FizzJump {
            PLAYFUL,
            TRICKSTER
        }
    }
}