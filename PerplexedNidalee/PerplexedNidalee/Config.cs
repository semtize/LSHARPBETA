using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace PerplexedNidalee
{
    class Config
    {
        public static Menu Settings = new Menu("Perplexed Nidalee", "menu", true);
        public static Orbwalking.Orbwalker Orbwalker;

        public static void Initialize()
        {
            //Orbwalker
            {
                Settings.AddSubMenu(new Menu("Orbwalker", "orbMenu"));
                Orbwalker = new Orbwalking.Orbwalker(Settings.SubMenu("orbMenu"));
            }
            //Target Selector
            {
                Settings.AddSubMenu(new Menu("Target Selector", "ts"));
                TargetSelector.AddToMenu(Settings.SubMenu("ts"));
            }
            //Combo
            {
                Settings.AddSubMenu(new Menu("Combo", "menuCombo"));
                Settings.SubMenu("menuCombo").AddSubMenu(new Menu("Human", "menuComboHuman"));
                Settings.SubMenu("menuCombo").AddSubMenu(new Menu("Cougar", "menuComboCougar"));
                //Human
                {
                    Settings.SubMenu("menuCombo").SubMenu("menuComboHuman").AddItem(new MenuItem("comboJavelin", "Javelin").SetValue(true));
                    Settings.SubMenu("menuCombo").SubMenu("menuComboHuman").AddItem(new MenuItem("comboBushwhack", "Bushwhack").SetValue(true));
                }
                //Cougar
                {
                    Settings.SubMenu("menuCombo").SubMenu("menuComboCougar").AddItem(new MenuItem("comboTakedown", "Takedown").SetValue(true));
                    Settings.SubMenu("menuCombo").SubMenu("menuComboCougar").AddItem(new MenuItem("comboPounce", "Pounce").SetValue(true));
                    Settings.SubMenu("menuCombo").SubMenu("menuComboCougar").AddItem(new MenuItem("comboSwipe", "Swipe").SetValue(true));
                }
            }
            //Harass
            {
                Settings.AddSubMenu(new Menu("Harass", "menuHarass"));
                Settings.SubMenu("menuHarass").AddSubMenu(new Menu("Human", "menuHarassHuman"));
                Settings.SubMenu("menuHarass").AddSubMenu(new Menu("Cougar", "menuHarassCougar"));
                //Human
                {
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassHuman").AddItem(new MenuItem("harassJavelin", "Javelin").SetValue(true));
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassHuman").AddItem(new MenuItem("harassBushwhack", "Bushwhack").SetValue(false));
                }
                //Cougar
                {
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassCougar").AddItem(new MenuItem("harassTakedown", "Takedown").SetValue(true));
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassCougar").AddItem(new MenuItem("harassPounce", "Pounce").SetValue(true));
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassCougar").AddItem(new MenuItem("harassSwipe", "Swipe").SetValue(true));
                }
                //Mana
                Settings.SubMenu("menuHarass").AddItem(new MenuItem("harassManaPct", "Minimum Mana %").SetValue(new Slider(70, 10, 90)));
            }
            //Heal
            {
                Settings.AddSubMenu(new Menu("Healing", "menuHealing"));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("enableHeal", "Enabled").SetValue(true));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healSelf", "Heal Self").SetValue(true));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healAllies", "Heal Allies").SetValue(true));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healPct", "% health to heal self/allies").SetValue(new Slider(50, 10, 90)));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healManaPct", "Minimum Mana %").SetValue(new Slider(50, 10, 90)));
            }
            //Lane Clear
            {
                Settings.AddSubMenu(new Menu("Lane Clear", "menuLaneClear"));
                Settings.SubMenu("menuLaneClear").AddSubMenu(new Menu("Human", "menuLaneHuman"));
                Settings.SubMenu("menuLaneClear").AddSubMenu(new Menu("Cougar", "menuLaneCougar"));
                //Human
                {
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneHuman").AddItem(new MenuItem("laneJavelin", "Javelin").SetValue(false));
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneHuman").AddItem(new MenuItem("laneBushwhack", "Bushwhack").SetValue(false));
                }
                //Cougar
                {
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneCougar").AddItem(new MenuItem("laneTakedown", "Takedown").SetValue(true));
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneCougar").AddItem(new MenuItem("lanePounce", "Pounce").SetValue(true));
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneCougar").AddItem(new MenuItem("laneSwipe", "Swipe").SetValue(true));
                }
                //Mana
                Settings.SubMenu("menuLaneClear").AddItem(new MenuItem("laneManaPct", "Minimum Mana %").SetValue(new Slider(70, 10, 90)));
            }
            //Jungle Clear
            {
                Settings.AddSubMenu(new Menu("Jungle Clear", "menuJungleClear"));
                Settings.SubMenu("menuJungleClear").AddSubMenu(new Menu("Human", "menuJungleHuman"));
                Settings.SubMenu("menuJungleClear").AddSubMenu(new Menu("Cougar", "menuJungleCougar"));
                //Human
                {
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleHuman").AddItem(new MenuItem("jungleJavelin", "Javelin").SetValue(true));
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleHuman").AddItem(new MenuItem("jungleBushwhack", "Bushwhack").SetValue(false));
                }
                //Cougar
                {
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleCougar").AddItem(new MenuItem("jungleTakedown", "Takedown").SetValue(true));
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleCougar").AddItem(new MenuItem("junglePounce", "Pounce").SetValue(true));
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleCougar").AddItem(new MenuItem("jungleSwipe", "Swipe").SetValue(true));
                }
                //Mana
                Settings.SubMenu("menuJungleClear").AddItem(new MenuItem("jungleManaPct", "Minimum Mana %").SetValue(new Slider(70, 10, 90)));
            }
            //Summoners
            {
                Settings.AddSubMenu(new Menu("Summoners", "menuSumms"));
                //Heal
                {
                    Settings.SubMenu("menuSumms").AddSubMenu(new Menu("Heal", "summHeal"));
                    Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("useHeal", "Enabled").SetValue(true));
                    Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("summHealPct", "Use On % Health").SetValue(new Slider(35, 10, 90)));
                }
                //Ignite
                {
                    Settings.SubMenu("menuSumms").AddSubMenu(new Menu("Ignite", "summIgnite"));
                    Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("useIgnite", "Enabled").SetValue(true));
                    Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("igniteMode", "Use Ignite For").SetValue(new StringList(new string[] { "Execution", "Combo" })));
                }
            }
            //Items
            {
                Settings.AddSubMenu(new Menu("Items", "menuItems"));
                //Offensive
                {
                    Settings.SubMenu("menuItems").AddSubMenu(new Menu("Offensive", "offItems"));
                    foreach (var offItem in ItemManager.Items.Where(item => item.Type == ItemType.Offensive))
                        Settings.SubMenu("menuItems").SubMenu("offItems").AddItem(new MenuItem("use" + offItem.ShortName, offItem.Name).SetValue(true));
                }
                //Defensive
                {
                    Settings.SubMenu("menuItems").AddSubMenu(new Menu("Defensive", "defItems"));
                    foreach (var defItem in ItemManager.Items.Where(item => item.Type == ItemType.Defensive))
                    {
                        Settings.SubMenu("menuItems").SubMenu("defItems").AddSubMenu(new Menu(defItem.Name, "menu" + defItem.ShortName));
                        Settings.SubMenu("menuItems").SubMenu("defItems").SubMenu("menu" + defItem.ShortName).AddItem(new MenuItem("use" + defItem.ShortName, "Enable").SetValue(true));
                        Settings.SubMenu("menuItems").SubMenu("defItems").SubMenu("menu" + defItem.ShortName).AddItem(new MenuItem("pctHealth" + defItem.ShortName, "Use On % Health").SetValue(new Slider(35, 10, 90)));
                    }
                }
                //Cleanse
                {
                    Settings.SubMenu("menuItems").AddSubMenu(new Menu("Cleanse", "cleanseItems"));
                    foreach (var cleanseItem in ItemManager.Items.Where(item => item.Type == ItemType.Cleanse))
                        Settings.SubMenu("menuItems").SubMenu("cleanseItems").AddItem(new MenuItem("use" + cleanseItem.ShortName, cleanseItem.Name).SetValue(true));
                }
            }
            //Anti-Gapcloser
            {
                Settings.AddSubMenu(new Menu("Anti-Gapcloser", "menuGapcloser"));
                Settings.SubMenu("menuGapcloser").AddItem(new MenuItem("dodgePounce", "Dodge With Pounce").SetValue(true));
            }
            //Drawing
            {
                Settings.AddSubMenu(new Menu("Drawing", "menuDrawing"));
                Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawJavelin", "Javelin Range").SetValue(new Circle(true, Color.Yellow)));
            }
            //Finish
            Settings.AddToMainMenu();
        }

        //Combo
        public static bool ComboJavelin { get { return Settings.Item("comboJavelin").GetValue<bool>(); } }
        public static bool ComboBushwhack { get { return Settings.Item("comboBushwhack").GetValue<bool>(); } }
        public static bool ComboTakedown { get { return Settings.Item("comboTakedown").GetValue<bool>(); } }
        public static bool ComboPounce { get { return Settings.Item("comboPounce").GetValue<bool>(); } }
        public static bool ComboSwipe { get { return Settings.Item("comboSwipe").GetValue<bool>(); } }
        
        //Harass
        public static bool HarassJavelin { get { return Settings.Item("harassJavelin").GetValue<bool>(); } }
        public static bool HarassBushwhack { get { return Settings.Item("harassBushwhack").GetValue<bool>(); } }
        public static bool HarassTakedown { get { return Settings.Item("harassTakedown").GetValue<bool>(); } }
        public static bool HarassPounce { get { return Settings.Item("harassPounce").GetValue<bool>(); } }
        public static bool HarassSwipe { get { return Settings.Item("harassBushwhack").GetValue<bool>(); } }
        public static int HarassManaPct { get { return Settings.Item("harassManaPct").GetValue<Slider>().Value; } }

        //Heal
        public static bool EnableHeal { get { return Settings.Item("enableHeal").GetValue<bool>(); } }
        public static bool HealSelf { get { return Settings.Item("healSelf").GetValue<bool>(); } }
        public static bool HealAllies { get { return Settings.Item("healAllies").GetValue<bool>(); } }
        public static int HealPct { get { return Settings.Item("healPct").GetValue<Slider>().Value; } }
        public static int HealManaPct { get { return Settings.Item("healManaPct").GetValue<Slider>().Value; } }

        //Lane Clear
        public static bool LaneJavelin { get { return Settings.Item("laneJavelin").GetValue<bool>(); } }
        public static bool LaneBushwhack { get { return Settings.Item("laneBushwhack").GetValue<bool>(); } }
        public static bool LaneTakedown { get { return Settings.Item("laneTakedown").GetValue<bool>(); } }
        public static bool LanePounce { get { return Settings.Item("lanePounce").GetValue<bool>(); } }
        public static bool LaneSwipe { get { return Settings.Item("laneSwipe").GetValue<bool>(); } }
        public static int LaneManaPct { get { return Settings.Item("laneManaPct").GetValue<Slider>().Value; } }

        //Jungle Clear
        public static bool JungleJavelin { get { return Settings.Item("jungleJavelin").GetValue<bool>(); } }
        public static bool JungleBushwhack { get { return Settings.Item("jungleBushwhack").GetValue<bool>(); } }
        public static bool JungleTakedown { get { return Settings.Item("jungleTakedown").GetValue<bool>(); } }
        public static bool JunglePounce { get { return Settings.Item("junglePounce").GetValue<bool>(); } }
        public static bool JungleSwipe { get { return Settings.Item("jungleSwipe").GetValue<bool>(); } }
        public static int JungleManaPct { get { return Settings.Item("jungleManaPct").GetValue<Slider>().Value; } }

        //Summoners
        public static bool UseSummHeal { get { return Settings.Item("useHeal").GetValue<bool>(); } }
        public static int SummHealPct { get { return Settings.Item("summHealPct").GetValue<Slider>().Value; } }
        public static bool UseIgnite { get { return Settings.Item("useIgnite").GetValue<bool>(); } }
        public static string IgniteMode { get { return Settings.Item("igniteMode").GetValue<StringList>().SelectedValue; } }

        //Items
        public static bool ShouldUseItem(string shortName)
        {
            return Settings.Item("use" + shortName).GetValue<bool>();
        }
        public static int UseOnPercent(string shortName)
        {
            return Settings.Item("pctHealth" + shortName).GetValue<Slider>().Value;
        }
        
        //Anti-Gapcloser
        public static bool DodgeWithPounce { get { return Settings.Item("dodgePounce").GetValue<bool>(); } }

        //Drawing
        public static Circle DrawJavelin { get { return Settings.Item("drawJavelin").GetValue<Circle>(); } }
    }
}
