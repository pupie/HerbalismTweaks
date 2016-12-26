	using Styx;
	using Styx.Common;
	using Styx.CommonBot;
	using Styx.Plugins;
	using Styx.WoWInternals;
	using Styx.WoWInternals.WoWObjects;
	using Styx.TreeSharp;
	using Styx.Pathing;

	using CommonBehaviors.Actions;
	using Bots.Grind;

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	namespace KillCombat
	{
		public class KillCombat : HBPlugin
		{
			public override string Author { get { return "CptJesus"; } }
			public override string Name { get { return "Herbalism Tweaks"; } }
			public override Version Version { get { return new Version(1, 1); } }
			public Styx.WoWInternals.WoWObjects.LocalPlayer Me { get { return Styx.StyxWoW.Me; } }
			private Composite task;

			//setting
			public static int mehealthpercent = 80;
			public static bool mecombat = true;
			public static bool isfoxpresent = true;
			public static bool mobispresent = true;
			internal async Task<bool> FoxFlower()
			{
				WoWAreaTrigger scrap = GetFlowerScrap();
				if (scrap != null && !Me.Combat)
				{
					Navigator.MoveTo(scrap.Location);
					return true;
				}
				if (IsFoxPresent() && !Me.Combat)
				{
					return true;
				}
				return false;
			}

			private bool IsFoxPresent()
			{
				using (StyxWoW.Memory.AcquireFrame())
				{
					IEnumerable<WoWUnit> mobsOfInterestQuery =
					from wowUnit in ObjectManager.GetObjectsOfType<WoWUnit>(false, false)
					where
					(int)wowUnit.Entry == 98235
					&& wowUnit.IsAlive
					&& !wowUnit.TaggedByOther
					orderby
					wowUnit.DistanceSqr
					select
					wowUnit;
					return mobsOfInterestQuery.ToList().Count() > 0;
				}
			}

			private WoWAreaTrigger GetFlowerScrap()
			{
				IEnumerable<WoWAreaTrigger> t = 
				from trigger in ObjectManager.GetObjectsOfType<WoWAreaTrigger>(false, false)
				where
				(int)trigger.Entry == 9756
				orderby
				trigger.DistanceSqr
				select
				trigger;
				
				return t.FirstOrDefault();
			}

			private bool MobIsPresent()
			{
				int[] mobIdsOfInterest = { 98232, 98233, 98234, 114113, 104877 };
				
				using (StyxWoW.Memory.AcquireFrame())
				{
					IEnumerable<WoWUnit> mobsOfInterestQuery =
					from wowUnit in ObjectManager.GetObjectsOfType<WoWUnit>(false, false)
					where
					mobIdsOfInterest.Contains((int)wowUnit.Entry)
					&& wowUnit.IsAlive
					&& !wowUnit.TaggedByOther
					orderby
					wowUnit.DistanceSqr
					select
					wowUnit;
					return mobsOfInterestQuery.ToList().Count() > 0;
				}
			}

			private void enableCombat()
			{
				LevelBot.BehaviorFlags |= BehaviorFlags.Combat;
				Logging.Write(LevelBot.BehaviorFlags.ToString());
			}

			private void disableCombat()
			{
				LevelBot.BehaviorFlags &= ~BehaviorFlags.Combat;
			}

			public override void OnEnable()
			{
				base.OnEnable();
				Logging.Write("Starting Herbalism Tweaks");
				task = new ActionRunCoroutine(r => FoxFlower());
				TreeHooks.Instance.InsertHook("InGame_Check", 0, task);
				disableCombat();
			}

			public override void OnDisable()
			{
				base.OnDisable();
				TreeHooks.Instance.RemoveHook("InGame_Check", task);
				enableCombat();
			}

			public override void Pulse()
			{
				// if (!Me.HasAura("Water Walking") && !Me.Combat){
				// 	SpellManager.Cast("Water Walking", Me);
				// } //Only for Shaman
				if (Me.HealthPercent < mehealthpercent || !Me.Mounted)
				{
					enableCombat();
					return;
				}
				if (!Me.Combat && mecombat)
				{
					disableCombat();
					return;
				}

				if (IsFoxPresent() && isfoxpresent)
				{
					enableCombat();
					return;
				}

				if (MobIsPresent() && mobispresent)
				{
					enableCombat();
					return;
				}
			}
		}
	}
