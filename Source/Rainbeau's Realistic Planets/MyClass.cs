﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;

namespace Planets_Code {

	[StaticConstructorOnStartup]
	internal static class Planets_Initializer {
		static Planets_Initializer() {
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("My Little Planet"))) {
				Controller.Settings.usingMLP = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Faction Control"))) {
				Controller.Settings.usingFactionControl = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Nature's Pretty Sweet"))) {
				Controller.Settings.otherGrassland = true;
				Controller.Settings.otherSavanna = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Terra Project (Core)"))) {
				Controller.Settings.otherSavanna = true;
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Advanced Biomes"))) {
				Controller.Settings.otherSavanna = true;
			}
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefsListForReading) {
				if (current.plant != null) {
					if (current.plant.wildBiomes != null) {
						for (int j = 0; j < current.plant.wildBiomes.Count; j++) {
							if (current.plant.wildBiomes[j].biome.defName == "Tundra") {
								PlantBiomeRecord newRecord = new PlantBiomeRecord();
								newRecord.biome = BiomeDef.Named("RRP_Permafrost");
								newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
								current.plant.wildBiomes.Add(newRecord);
							}
							if (current.plant.wildBiomes[j].biome.defName == "TemperateForest") {
								if (!current.defName.Contains("Tree")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_Grassland");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
								}
							}
							if (current.plant.wildBiomes[j].biome.defName == "BorealForest") {
								if (!current.defName.Contains("Tree")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_Steppes");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
								}
							}
							if (current.plant.wildBiomes[j].biome.defName == "AridShrubland") {
								if (!current.defName.Contains("Acacia")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_Savanna");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
								}
							}
							if (current.plant.wildBiomes[j].biome.defName == "Desert") {
								if (!current.defName.Contains("Acacia")) {
									PlantBiomeRecord newRecord = new PlantBiomeRecord();
									newRecord.biome = BiomeDef.Named("RRP_TemperateDesert");
									newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
									current.plant.wildBiomes.Add(newRecord);
									PlantBiomeRecord newRecord2 = new PlantBiomeRecord();
									newRecord2.biome = BiomeDef.Named("RRP_Oasis");
									newRecord2.commonality = current.plant.wildBiomes[j].commonality;
									current.plant.wildBiomes.Add(newRecord2);
								}
							}
						}
					}
				}
			}
			foreach (PawnKindDef current in DefDatabase<PawnKindDef>.AllDefs) {
				if (current.RaceProps.wildBiomes != null && current.defName != "Cobra") {
					for (int j = 0; j < current.RaceProps.wildBiomes.Count; j++) {
						if (current.RaceProps.wildBiomes[j].biome.defName == "Tundra") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Permafrost");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "TemperateForest") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Grassland");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "BorealForest") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Steppes");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "AridShrubland") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_Savanna");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
						if (current.RaceProps.wildBiomes[j].biome.defName == "Desert") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("RRP_TemperateDesert");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
							AnimalBiomeRecord newRecord2 = new AnimalBiomeRecord();
							newRecord2.biome = BiomeDef.Named("RRP_Oasis");
							newRecord2.commonality = current.RaceProps.wildBiomes[j].commonality;
							current.RaceProps.wildBiomes.Add(newRecord2);
						}
					}
				}
			}
		}
	}
	
	public class Controller : Mod {
		public static Dictionary<Faction, int> factionCenters = new Dictionary<Faction, int>();
		public static Dictionary<Faction, int> failureCount = new Dictionary<Faction, int>();
		public static double maxFactionSprawl = 0;
		public static double minFactionSeparation = 0;
		public static Settings Settings;
		public override string SettingsCategory() { return "Planets.ModName".Translate(); }
		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		public Controller(ModContentPack content) : base(content) {
			Harmony harmony = new Harmony("net.rainbeau.rimworld.mod.realisticplanets");
			harmony.PatchAll( Assembly.GetExecutingAssembly() );
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public bool otherGrassland = false;
		public bool otherSavanna = false;
		public bool usingMLP = false;
		public bool usingFactionControl = false;
		public bool randomPlanet = false;
		public bool checkTemp = true;
		public float factionGrouping = 2.5f;
		public void DoWindowContents(Rect canvas) {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap(24);
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				list.Label("Planets.SettingsDisabled".Translate());
			}
			else {
				list.CheckboxLabeled( "Planets.CheckTemp".Translate(), ref checkTemp, "Planets.CheckTempTip".Translate() );
				list.Gap(24);
				factionGrouping = list.Slider(factionGrouping, 0, 3.99f);
				if (factionGrouping < 1) {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingNone".Translate());
				}
				else if (factionGrouping < 2 ) {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingLoose".Translate());
				}
				else if (factionGrouping < 3) {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingTight".Translate());
				}
				else {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingVeryTight".Translate());
				}
			}
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref checkTemp, "checkTemp", true);
			Scribe_Values.Look(ref factionGrouping, "factionGrouping", 2.5f);
		}
	}

	public class Planets_GameComponent : GameComponent {
		public static AxialTilt axialTilt = AxialTilt.Normal;
		public static WorldType worldType = WorldType.Vanilla;
		public static int subcount = 10;
		public Planets_GameComponent() { }
		public Planets_GameComponent(Game game) { }
		public override void ExposeData() {
			Scribe_Values.Look(ref axialTilt, "axialTilt", AxialTilt.Normal, true);
			Scribe_Values.Look(ref worldType, "worldType", WorldType.Vanilla, true);
			Scribe_Values.Look(ref subcount, "subcount", 10, true);
		}
	}
	
	//
	// NEW PLANET SETTINGS STUFF
	//
	
	public enum WorldType {
		Waterworld,
		Islands,
		Earthlike,
		Vanilla,
		Dry,
		VeryDry,
		Barren
	}

	public static class WorldTypeUtility {
		private static int cachedEnumValuesCount = -1;
		public static int EnumValuesCount {
			get {
				if (WorldTypeUtility.cachedEnumValuesCount < 0) {
					WorldTypeUtility.cachedEnumValuesCount = Enum.GetNames(typeof(WorldType)).Length;
				}
				return WorldTypeUtility.cachedEnumValuesCount;
			}
		}	
	}
	
	public enum AxialTilt {
		VeryLow,
		Low,
		Normal,
		High,
		VeryHigh
	}
	
	public static class AxialTiltUtility {
		private static int cachedEnumValuesCount = -1;
		public static int EnumValuesCount {
			get {
				if (AxialTiltUtility.cachedEnumValuesCount < 0) {
					AxialTiltUtility.cachedEnumValuesCount = Enum.GetNames(typeof(AxialTilt)).Length;
				}
				return AxialTiltUtility.cachedEnumValuesCount;
			}
		}
	}
	
	public enum RainfallModifier {
		Little,
		LittleBitLess,
		Normal,
		LittleBitMore,
		High
	}

	public static class RainfallModifierUtility {
		private static int cachedEnumValuesCount = -1;
		public static int EnumValuesCount {
			get {
				if (RainfallModifierUtility.cachedEnumValuesCount < 0) {
					RainfallModifierUtility.cachedEnumValuesCount = Enum.GetNames(typeof(RainfallModifier)).Length;
				}
				return RainfallModifierUtility.cachedEnumValuesCount;
			}
		}	
	}

	//
	// REPLACEMENT BIOMEWORKERS FOR VANILLA BIOMES
	//
	
	public class BiomeWorker_RRP_AridShrubland : BiomeWorker {
		public BiomeWorker_RRP_AridShrubland() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.rainfall < 170f) {
				return 0f;
			}
			if (tile.temperature > 0f && (tile.rainfall/tile.temperature < 16f)) {
				return 0f;
			}
			return tile.temperature + 0.0002f;
		}
	}

	public class BiomeWorker_RRP_BorealForest : BiomeWorker {
		public BiomeWorker_RRP_BorealForest() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < -10f) {
				return 0f;
			}
			if (tile.rainfall < 600f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature > 5f && (tile.temperature + tempAdjust >= 18f)) {
				return 0f;
			}
			if (tile.temperature + tempAdjust < 12f) {
				return 0f;
			}
			return 15f;
		}
	}

	public class BiomeWorker_RRP_ColdBog : BiomeWorker {
		public BiomeWorker_RRP_ColdBog() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < -10f) {
				return 0f;
			}
			if (tile.rainfall < 600f) {
				return 0f;
			}
			if (tile.swampiness < 0.5f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature > 5f && (tile.temperature + tempAdjust >= 18f)) {
				return 0f;
			}
			if (tile.temperature + tempAdjust < 12f) {
				return 0f;
			}
			return -tile.temperature + 13f + tile.swampiness * 8f;
		}
	}
	
	public class BiomeWorker_RRP_Desert : BiomeWorker {
		public BiomeWorker_RRP_Desert() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < 30 && tile.rainfall >= 600f) {
				return 0f;
			}
			return tile.temperature + 0.0001f;
		}
	}

	public class BiomeWorker_RRP_IceSheet : BiomeWorker {
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust > 3f) {
				return 0f;
			}
			return BiomeWorker_IceSheet.PermaIceScore(tile);
		}
	}

	public class BiomeWorker_RRP_SeaIce : BiomeWorker {
		private ModuleBase cachedSeaIceAllowedNoise;
		private int cachedSeaIceAllowedNoiseForSeed;
		public BiomeWorker_RRP_SeaIce() { }
		private bool AllowedAt(int tile) {
			Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
			Vector3 worldGrid = Find.WorldGrid.viewCenter;
			float single = Vector3.Angle(worldGrid, tileCenter);
			float worldGrid1 = Find.WorldGrid.viewAngle;
			float single1 = Mathf.Min(7.5f, worldGrid1 * 0.12f);
			float single2 = Mathf.InverseLerp(worldGrid1 - single1, worldGrid1, single);
			if (single2 <= 0f) {
				return true;
			}
			if (this.cachedSeaIceAllowedNoise == null || this.cachedSeaIceAllowedNoiseForSeed != Find.World.info.Seed) {
				this.cachedSeaIceAllowedNoise = new Perlin(0.017000000923872, 2, 0.5, 6, Find.World.info.Seed, QualityMode.Medium);
				this.cachedSeaIceAllowedNoiseForSeed = Find.World.info.Seed;
			}
			float headingFromTo = Find.WorldGrid.GetHeadingFromTo(worldGrid, tileCenter);
			float value = (float)this.cachedSeaIceAllowedNoise.GetValue((double)headingFromTo, 0, 0) * 0.5f + 0.5f;
			return single2 <= value;
		}
		public override float GetScore(Tile tile, int tileID) {
			if (!tile.WaterCovered) {
				return -100f;
			}
			if (!this.AllowedAt(tileID)) {
				return -100f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust > 3f) {
				return -100f;
			}
			return BiomeWorker_IceSheet.PermaIceScore(tile) - 23f;
		}
	}

	public class BiomeWorker_RRP_TemperateForest : BiomeWorker {
		public BiomeWorker_RRP_TemperateForest() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature <= 5f || tile.temperature > 18f) {
				return 0f;
			}
			if (tile.rainfall < 600f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust < 18f) {
				return 0f;
			}
			return 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f;
		}
	}

	public class BiomeWorker_RRP_TemperateSwamp : BiomeWorker {
		public BiomeWorker_RRP_TemperateSwamp() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature <= 5f || tile.temperature > 18f) {
				return 0f;
			}
			if (tile.rainfall < 600f) {
				return 0f;
			}
			if (tile.swampiness < 0.5f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust < 18f) {
				return 0f;
			}
			return 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f + tile.swampiness * 3f;
		}
	}

	public class BiomeWorker_RRP_TropicalRainforest : BiomeWorker {
		public BiomeWorker_RRP_TropicalRainforest() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < 15f) {
				return 0f;
			}
			if (tile.rainfall < 2000f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature - tempAdjust > 42f) {
				return 0f;
			}
			return 28f + (tile.temperature - 20f) * 1.5f + (tile.rainfall - 600f) / 165f;
		}
	}

	public class BiomeWorker_RRP_TropicalSwamp : BiomeWorker {
		public BiomeWorker_RRP_TropicalSwamp() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < 15f) {
				return 0f;
			}
			if (tile.rainfall < 2000f) {
				return 0f;
			}
			if (tile.swampiness < 0.5f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature - tempAdjust > 42f) {
				return 0f;
			}
			return 28f + (tile.temperature - 20f) * 1.5f + (tile.rainfall - 600f) / 165f + tile.swampiness * 3f;
		}
	}

	//
	// REPLACEMENT BIOMEWORKERS FOR OTHER MODS' BIOMES
	//
	
	public class BiomeWorker_RRP_Grassland_Mod : BiomeWorker {
		public BiomeWorker_RRP_Grassland_Mod() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < -10f) {
				return 0f;
			}
			if (tile.rainfall < 170f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust < 6f) {
				return 0f;
			}
			if (tile.rainfall >= 600f && (tile.temperature + tempAdjust >= 12f)) {
				return 0f;
			}
			if (tile.temperature > 0f && (tile.rainfall/tile.temperature < 28f)) {
				return 0f;
			}
			if (tile.temperature < 0f && (tile.rainfall/-tile.temperature < 50f)) {
				return 0f;
			}
			if (tile.temperature < 0f) {
				return -tile.temperature + 0.0003f;
			}
			return tile.temperature + 0.0003f;
		}
	}

	public class BiomeWorker_RRP_Savanna_Mod : BiomeWorker {
		public BiomeWorker_RRP_Savanna_Mod() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature <= 18f) {
				return 0f;
			}
			if (tile.temperature > 0f && (tile.rainfall/tile.temperature < 28f)) {
				return 0f;
			}
			if (tile.rainfall < 600f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.rainfall >= 2000f && (tile.temperature - tempAdjust <= 42f)) {
				return 0f;
			}
			return 22.5f + (tile.temperature - 20f) * 2.2f + (tile.rainfall - 600f) / 100f;
		}
	}

	public class BiomeWorker_NPS_SaltFlats : BiomeWorker {
		public BiomeWorker_NPS_SaltFlats() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < 30 && tile.rainfall >= 600f) {
				return 0f;
			}
			if (tile.temperature < 10) {
				return 0f;
			}
			if (tile.rainfall/tile.temperature >= 16f) {
				return 0f;
			}
			if (Rand.Value > .006) {
				return 0f;
			}
			return tile.temperature + 15;
		}
    }

	public class BiomeWorker_NPS_Oasis : BiomeWorker {
		public BiomeWorker_NPS_Oasis() { }
        public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < 30 && tile.rainfall >= 600f) {
				return 0f;
			}
			if (tile.temperature < 10) {
				return 0f;
			}
			if (tile.rainfall/tile.temperature >= 16f) {
				return 0f;
			}
			if (Rand.Value > .006) {
				return 0f;
			}
			return tile.temperature + 20;
		}
	}

	public class BiomeWorker_BoFoUrban : BiomeWorker_RRP_BorealForest {
		public BiomeWorker_BoFoUrban() { }
		public override float GetScore(Tile tile, int tileID) {
			float single;
			float single1 = 0.0008f;
			float score = base.GetScore(tile, tileID);
			single = ((score <= 0f ? false : Rand.Value <= single1) ? score + 0.1f : score - 1f);
			return single;
		}
	}

	public class BiomeWorker_TeFoUrban : BiomeWorker_RRP_TemperateForest {
		public BiomeWorker_TeFoUrban() { }
		public override float GetScore(Tile tile, int tileID) {
			float single;
			float single1 = 0.0008f;
			float score = base.GetScore(tile, tileID);
			single = ((score <= 0f ? false : Rand.Value <= single1) ? score + 0.1f : score - 1f);
			return single;
		}
	}

	public class BiomeWorker_TeSwUrban : BiomeWorker_RRP_TemperateSwamp {
		public BiomeWorker_TeSwUrban() { }
		public override float GetScore(Tile tile, int tileID) {
			float single;
			float single1 = 0.002f;
			float score = base.GetScore(tile, tileID);
			single = ((score <= 0f ? false : Rand.Value <= single1) ? score + 0.1f : score - 1f);
			return single;
		}
	}

	public class BiomeWorker_TrRaUrban : BiomeWorker_RRP_TropicalRainforest {
		public BiomeWorker_TrRaUrban() { }
		public override float GetScore(Tile tile, int tileID) {
			float single;
			float single1 = 0.0008f;
			float score = base.GetScore(tile, tileID);
			single = ((score <= 0f ? false : Rand.Value <= single1) ? score + 0.1f : score - 1f);
			return single;
		}
	}

	public class BiomeWorker_TrSwUrban : BiomeWorker_RRP_TropicalSwamp {
		public BiomeWorker_TrSwUrban() { }
		public override float GetScore(Tile tile, int tileID) {
			float single;
			float single1 = 0.002f;
			float score = base.GetScore(tile, tileID);
			single = ((score <= 0f ? false : Rand.Value <= single1) ? score + 0.1f : score - 1f);
			return single;
		}
	}
	
	//
	// BIOMEWORKERS FOR NEW BIOMES
	//
	
	public class BiomeWorker_RRP_Grassland : BiomeWorker {
		public BiomeWorker_RRP_Grassland() { }
		public override float GetScore(Tile tile, int tileID) {
			if (Controller.Settings.otherGrassland.Equals(true)) {
				return -100f;
			}
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < -10f) {
				return 0f;
			}
			if (tile.rainfall < 170f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust < 6f) {
				return 0f;
			}
			if (tile.rainfall >= 600f && (tile.temperature + tempAdjust >= 12f)) {
				return 0f;
			}
			if (tile.temperature > 0f && (tile.rainfall/tile.temperature < 28f)) {
				return 0f;
			}
			if (tile.temperature < 0f && (tile.rainfall/-tile.temperature < 50f)) {
				return 0f;
			}
			if (tile.temperature < 0f) {
				return -tile.temperature + 0.0003f;
			}
			return tile.temperature + 0.0003f;
		}
	}

	public class BiomeWorker_RRP_Oasis : BiomeWorker {
		public BiomeWorker_RRP_Oasis() { }
        public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < 30 && tile.rainfall >= 600f) {
				return 0f;
			}
			if (tile.temperature < 10) {
				return 0f;
			}
			float num = tile.rainfall/tile.temperature;
			if (num < 4 || num > 16) {
				return 0f;
			}
			if (num > 12) {
				if (Rand.Value > .008) {
					return 0f;
				}
				return tile.temperature + 5;
			}
			else if (num > 8) {
				if (Rand.Value > .004) {
					return 0f;
				}
				return tile.temperature + 5;
			}
			else {
				if (Rand.Value > .001) {
					return 0f;
				}
				return tile.temperature + 5;
			}
		}
	}
	
	public class BiomeWorker_RRP_Permafrost : BiomeWorker {
		public BiomeWorker_RRP_Permafrost() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.rainfall < 170f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust > 6f) {
				return 0f;
			}
			if (tile.temperature < -18f && tile.temperature > -24f) {
				return 100f;
			}
			return 0f;
		}
	}
	
	public class BiomeWorker_RRP_Savanna : BiomeWorker {
		public BiomeWorker_RRP_Savanna() { }
		public override float GetScore(Tile tile, int tileID) {
			if (Controller.Settings.otherSavanna.Equals(true)) {
				return -100f;
			}
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature <= 18f) {
				return 0f;
			}
			if (tile.temperature > 0f && (tile.rainfall/tile.temperature < 28f)) {
				return 0f;
			}
			if (tile.rainfall < 600f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.rainfall >= 2000f && (tile.temperature - tempAdjust <= 42f)) {
				return 0f;
			}
			return 22.5f + (tile.temperature - 20f) * 2.2f + (tile.rainfall - 600f) / 100f;
		}
	}
	
	public class BiomeWorker_RRP_Steppes : BiomeWorker {
		public BiomeWorker_RRP_Steppes() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.temperature < -10f) {
				return 0f;
			}
			if (tile.rainfall < 170f) {
				return 0f;
			}
			float tempAdjust = TempCheck.SeasonalTempChange(tileID);
			if (tile.temperature + tempAdjust < 6f) {
				return 0f;
			}
			if (tile.rainfall >= 600f && (tile.temperature + tempAdjust >= 12f)) {
				return 0f;
			}
			if (tile.temperature > 0f && (tile.rainfall/tile.temperature < 28f)) {
				return 0f;
			}
			if (tile.temperature < 0f && (tile.rainfall/-tile.temperature < 50f)) {
				return 0f;
			}
			if (tile.elevation > 1000f) {
				if (tile.temperature < 0f) {
					return -tile.temperature + 0.0004f;
				}
				return tile.temperature + 0.0004f;
			}
			return 0f;
		}
	}

	public class BiomeWorker_RRP_TemperateDesert : BiomeWorker {
		public BiomeWorker_RRP_TemperateDesert() { }
		public override float GetScore(Tile tile, int tileID) {
			if (tile.WaterCovered) {
				return -100f;
			}
			if (tile.rainfall >= 170f) {
				return 0f;
			}
			if (tile.temperature > 10f) {
				return 0f;
			}
			return tile.temperature + 0.0005f;
		}
	}
	
	//
	// PLANET GENERATION CHANGES
	//
	
	[HarmonyPatch(typeof(PlanetShapeGenerator), "DoGenerate", new Type[] { })]
	public static class PlanetShapeGenerator_DoGenerate {
		[HarmonyPriority(Priority.Low)]
		public static bool Prefix() {
			FieldInfo subdivisionsCount = typeof(PlanetShapeGenerator).GetField("subdivisionsCount", AccessTools.all);
			subdivisionsCount.SetValue(null, Planets_GameComponent.subcount);
			return true;
		}
	}

	[HarmonyPatch(typeof(OverallTemperatureUtility), "GetTemperatureCurve", null)]
	public static class OverallTemperatureUtility_GetTemperatureCurve {
		public static void Postfix(this OverallTemperature overallTemperature, ref SimpleCurve __result) {
			if (overallTemperature == OverallTemperature.VeryCold) {
				SimpleCurve Curve_VeryCold_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-50f, -90f), true },
					{ new CurvePoint(-40f, -75f), true },
					{ new CurvePoint(0f, -50f), true },
					{ new CurvePoint(20f, -43f), true },
					{ new CurvePoint(25f, -33f), true },
					{ new CurvePoint(30f, -23.5f), true },
					{ new CurvePoint(50f, -22f), true }
				};
				__result = Curve_VeryCold_Revised;
			}
			if (overallTemperature == OverallTemperature.Cold) {
				SimpleCurve Curve_Cold_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-50f, -78f), true },
					{ new CurvePoint(-25f, -48f), true },
					{ new CurvePoint(-20f, -33f), true },
					{ new CurvePoint(-13f, -23f), true },
					{ new CurvePoint(0f, -20f), true },
					{ new CurvePoint(30f, -11f), true },
					{ new CurvePoint(60f, 17f), true }
				};
				__result = Curve_Cold_Revised;
			}
			if (overallTemperature == OverallTemperature.LittleBitColder) {
				SimpleCurve Curve_LittleBitColder_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-20f, -25f), true },
					{ new CurvePoint(-15f, -18f), true },
					{ new CurvePoint(-5f, -16f), true },
					{ new CurvePoint(40f, 27f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_LittleBitColder_Revised;
			}
			if (overallTemperature == OverallTemperature.LittleBitWarmer) {
				SimpleCurve Curve_LittleBitWarmer_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-45f, -32f), true },
					{ new CurvePoint(40f, 53f), true },
					{ new CurvePoint(120f, 123f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_LittleBitWarmer_Revised;
			}
			if (overallTemperature == OverallTemperature.Hot) {
				SimpleCurve Curve_Hot_Revised = new SimpleCurve {
					{ new CurvePoint(-45f, -14f), true },
					{ new CurvePoint(-25f, -4f), true },
					{ new CurvePoint(-22f, 10f), true },
					{ new CurvePoint(-10f, 33f), true },
					{ new CurvePoint(40f, 65f), true },
					{ new CurvePoint(120f, 128f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_Hot_Revised;
			}
			if (overallTemperature == OverallTemperature.VeryHot) {
				SimpleCurve Curve_VeryHot_Revised = new SimpleCurve {
					{ new CurvePoint(-45f, 40f), true },
					{ new CurvePoint(0f, 55f), true },
					{ new CurvePoint(33f, 95f), true },
					{ new CurvePoint(40f, 103f), true },
					{ new CurvePoint(120f, 135f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_VeryHot_Revised;
			}
		}
	}
	
	[HarmonyPatch(typeof(GenTemperature), "SeasonalShiftAmplitudeAt", null)]
	public static class GenTemperature_SeasonalShiftAmplitudeAt {
		public static void Postfix(int tile, ref float __result) {
			if (Find.WorldGrid.LongLatOf(tile).y >= 0f) {
				__result = Planets_TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
				return;
			}
			__result = -Planets_TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
		}
	}

	public static class TempCheck {
		public static float SeasonalTempChange(int tile) {
			return Math.Abs(Planets_TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile)));
		}
	}

	public static class Planets_TemperatureTuning {
        public static SimpleCurve SeasonalTempVariationCurve;
		static Planets_TemperatureTuning() {
        	SetSeasonalCurve();
        }
        public static void SetSeasonalCurve() {
        	if (Planets_GameComponent.axialTilt == AxialTilt.VeryLow) {
				SimpleCurve veryLowTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 0.75f), true },
					{ new CurvePoint(0.1f, 1f), true },
					{ new CurvePoint(1f, 7f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = veryLowTilt;
        	}
        	else if (Planets_GameComponent.axialTilt == AxialTilt.Low) {
				SimpleCurve lowTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 1.5f), true },
					{ new CurvePoint(0.1f, 2f), true },
					{ new CurvePoint(1f, 14f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = lowTilt;
        	}
			else if (Planets_GameComponent.axialTilt == AxialTilt.Normal) {
				SimpleCurve normalTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 3f), true },
					{ new CurvePoint(0.1f, 4f), true },
					{ new CurvePoint(1f, 28f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = normalTilt;
        	}
        	else if (Planets_GameComponent.axialTilt == AxialTilt.High) {
				SimpleCurve highTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 4.5f), true },
					{ new CurvePoint(0.1f, 6f), true },
					{ new CurvePoint(1f, 42f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = highTilt;
        	}
        	else {
				SimpleCurve veryHighTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 6f), true },
					{ new CurvePoint(0.1f, 8f), true },
					{ new CurvePoint(1f, 56f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = veryHighTilt;
        	}
		}
	}

	[HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow", null)]
	public static class SavedGameLoaderNow_LoadGameFromSaveFileNow {
		public static void Postfix() {
			Planets_TemperatureTuning.SetSeasonalCurve();
		}
	}
	
	[HarmonyPatch(typeof(PageUtility), "StitchedPages", null)]
	public static class PageUtility_StitchedPages {
		[HarmonyPriority(Priority.High)]
		public static void Postfix(ref Page __result) {
			if (__result == null) {
				return;
			}
			if (TutorSystem.TutorialMode) {
				return;
			}
			Page _Result = __result;
			while (true) {
				Page page = _Result.next;
				if (page == null) {
					break;
				}
				if (!(page is Page_CreateWorldParams)) {
					_Result = page;
				}
				else {
					Page page1 = page.next;
					Page page2 = page.prev;
					Planets_CreateWorldParams createWorldParam = new Planets_CreateWorldParams();
					page2.next = createWorldParam;
					page1.prev = createWorldParam;
					createWorldParam.prev = page2;
					createWorldParam.next = page1;
					break;
				}
			}
		}
	}	

	public class Planets_CreateWorldParams : Page {
		private bool initialized;
		private string seedString;
		private float planetCoverage;
		private string worldPreset;
		private RainfallModifier rainfallMod;
		private OverallRainfall rainfall;
		private OverallTemperature temperature;
        private OverallPopulation population;
		private readonly static float[] PlanetCoverages;
		private readonly static string[] WorldPresets;
		public override string PageTitle {
			get { return "CreateWorld".Translate(); }
		}
		static Planets_CreateWorldParams() {
			Planets_CreateWorldParams.PlanetCoverages = new float[] { 
				0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 
				0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
			};
			Planets_CreateWorldParams.WorldPresets = new string[] { 
				"Planets.Vanilla", 
				"Planets.Desert",
				"Planets.Frozen",
				"Planets.Earthlike",
				"Planets.Forest",
				"Planets.Iceball", 
				"Planets.Jungle",
				"Planets.Ocean", 
				"Planets.Custom"
			};
		}
		public Planets_CreateWorldParams() { }
		protected override bool CanDoNext() {
			if (!base.CanDoNext()) {
				return false;
			}
			if (Planets_GameComponent.worldType == WorldType.Waterworld) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.High; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.VeryHigh; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.VeryHigh; }
				else { this.rainfall = OverallRainfall.VeryHigh; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Islands) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.Normal; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.High; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.VeryHigh; }
				else { this.rainfall = OverallRainfall.VeryHigh; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Earthlike) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.LittleBitLess; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.Normal; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.High; }
				else { this.rainfall = OverallRainfall.VeryHigh; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Vanilla) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.Little; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.LittleBitLess; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.High) { this.rainfall = OverallRainfall.High; }
				else { this.rainfall = OverallRainfall.Normal; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Dry) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.Little; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.LittleBitLess; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.Normal; }
				else { this.rainfall = OverallRainfall.LittleBitMore; }
			}
			else if (Planets_GameComponent.worldType == WorldType.VeryDry) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.Little; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.LittleBitLess; }
				else { this.rainfall = OverallRainfall.Normal; }
			}
			else {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.Little; }
				else { this.rainfall = OverallRainfall.LittleBitLess; }
			}
			Planets_TemperatureTuning.SetSeasonalCurve();
			string generationString = "GeneratingWorld";
			if (Controller.Settings.randomPlanet.Equals(true)) {
				generationString = "Planets.GeneratingRandom";
				Controller.Settings.randomPlanet = false;
			}
			LongEventHandler.QueueLongEvent(() => {
				Find.GameInitData.ResetWorldRelatedMapInitData();
				Current.Game.World = WorldGenerator.GenerateWorld(this.planetCoverage, this.seedString, this.rainfall, this.temperature, this.population);
				LongEventHandler.ExecuteWhenFinished(() => {
					if (this.next != null) {
						Find.WindowStack.Add(this.next);
					}
					MemoryUtility.UnloadUnusedUnityAssets();
					Find.World.renderer.RegenerateAllLayersNow();
					this.Close(true);
				});
			}, generationString, true, null);
			return false;
		}
		public override void DoWindowContents(Rect rect) {
			base.DrawPageTitle(rect);
			GUI.BeginGroup(base.GetMainRect(rect, 0f, false));
			Text.Font = GameFont.Small;
			//
			// PLANET SEED
			//
			float single = 0f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "WorldSeed".Translate());
			Rect rect1 = new Rect(200f, single, 200f, 30f);
			this.seedString = Widgets.TextField(rect1, this.seedString);
			single += 40f;
			Rect rect2 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect2, "RandomizeSeed".Translate(), true, false, true)) {
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
				this.seedString = GenText.RandomSeedString();
			}
			//
			// PLANET SIZE (IF "MY LITTLE PLANET" IS IN USE)
			//
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("My Little Planet"))) {
				single += 80f;
				Widgets.Label(new Rect(0f, single, 200f, 30f), "MLPWorldPlanetSize".Translate());
				Rect rect7 = new Rect(200f, single, 200f, 30f);
				Planets_GameComponent.subcount = Mathf.RoundToInt(Widgets.HorizontalSlider(rect7, Planets_GameComponent.subcount, 6f, 10f, true, null, "MLPWorldTiny".Translate(), "MLPWorldDefault".Translate(), 1f));
			}
			//
			// PLANET COVERAGE
			//
			single += 80f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetCoverage".Translate());
			Rect rect3 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect3, this.planetCoverage.ToStringPercent(), true, false, true)) {
				List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
				float[] singleArray = Planets_CreateWorldParams.PlanetCoverages;
				for (int i = 0; i < (int)singleArray.Length; i++) {
					float single1 = singleArray[i];
					string stringPercent = single1.ToStringPercent();
					FloatMenuOption floatMenuOption = new FloatMenuOption(stringPercent, () => {
						if (this.planetCoverage != single1) {
							this.planetCoverage = single1;
							if (this.planetCoverage < 0.15f) {
								Messages.Message("Planets.MessageLowPlanetCoverageWarning".Translate(), MessageTypeDefOf.CautionInput, false);
							}
							if (this.planetCoverage > 0.7f) {
								Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, false);
							}
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
					floatMenuOptions.Add(floatMenuOption);
				}
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			}
			TooltipHandler.TipRegion(new Rect(0f, single, rect3.xMax, rect3.height), "PlanetCoverageTip".Translate());
			//
			// WORLD TYPE PRESETS
			//
			single += 80f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.WorldPresets".Translate());
			Rect rect8 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect8, this.worldPreset.Translate(), true, false, true)) {
				List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
				string[] singleArray = Planets_CreateWorldParams.WorldPresets;
				for (int i = 0; i < (int)singleArray.Length; i++) {
					string single1 = singleArray[i];
					string single1Translated = single1.Translate();
					FloatMenuOption floatMenuOption = new FloatMenuOption(single1Translated, () => {
						if (this.worldPreset != single1) {
							this.worldPreset = single1;
							if (this.worldPreset == "Planets.Vanilla") {
								Planets_GameComponent.worldType = WorldType.Vanilla;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.Normal;
                                this.population = OverallPopulation.Normal;
							}
							else if (this.worldPreset == "Planets.Desert") {
								Planets_GameComponent.worldType = WorldType.Dry;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitLess;
								this.temperature = OverallTemperature.LittleBitWarmer;
                                this.population = OverallPopulation.Little;
							}
							else if (this.worldPreset == "Planets.Frozen") {
								Planets_GameComponent.worldType = WorldType.VeryDry;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitLess;
								this.temperature = OverallTemperature.Cold;
                                this.population = OverallPopulation.Little;
							}
							else if (this.worldPreset == "Planets.Earthlike") {
								Planets_GameComponent.worldType = WorldType.Earthlike;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.Normal;
                                this.population = OverallPopulation.Normal;
							}
							else if (this.worldPreset == "Planets.Forest") {
								Planets_GameComponent.worldType = WorldType.Vanilla;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitMore;
								this.temperature = OverallTemperature.LittleBitColder;
                                this.population = OverallPopulation.LittleBitLess;
							}
							else if (this.worldPreset == "Planets.Iceball") {
								Planets_GameComponent.worldType = WorldType.Vanilla;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.VeryCold;
                                this.population = OverallPopulation.AlmostNone;
							}
							else if (this.worldPreset == "Planets.Jungle") {
								Planets_GameComponent.worldType = WorldType.Earthlike;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitMore;
								this.temperature = OverallTemperature.LittleBitWarmer;
                                this.population = OverallPopulation.Little;
							}
							else if (this.worldPreset == "Planets.Ocean") {
								Planets_GameComponent.worldType = WorldType.Waterworld;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.Normal;
                                this.population = OverallPopulation.AlmostNone;
							}
							else { }
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
					floatMenuOptions.Add(floatMenuOption);
				}
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			}
			TooltipHandler.TipRegion(new Rect(0f, single, rect8.xMax, rect8.height), "Planets.WorldPresetsTip".Translate());
			//
			// OCEAN SLIDER
			//
			single += 80f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.OceanType".Translate());
			Rect rect6 = new Rect(200f, single, 200f, 30f);
			WorldType worldTypeCheck = Planets_GameComponent.worldType;
			Planets_GameComponent.worldType = (WorldType)Mathf.RoundToInt(Widgets.HorizontalSlider(rect6, (float)Planets_GameComponent.worldType, 0f, (float)(WorldTypeUtility.EnumValuesCount - 1), true, "Planets.OceanType_Earthlike".Translate(), "Planets.OceanType_Waterworld".Translate(), "Planets.OceanType_Barren".Translate(), 1f));
			if (Planets_GameComponent.worldType != worldTypeCheck) { this.worldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect6.xMax, rect6.height), "Planets.OceanTypeTip".Translate());
			//
			// RAINFALL SLIDER
			//
			single += 40f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetRainfall".Translate());
			Rect rect4 = new Rect(200f, single, 200f, 30f);
			RainfallModifier rainfallModCheck = this.rainfallMod;
			this.rainfallMod = (RainfallModifier)Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, (float)this.rainfallMod, 0f, (float)(RainfallModifierUtility.EnumValuesCount - 1), true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
			if (this.rainfallMod != rainfallModCheck) { this.worldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect4.xMax, rect4.height), "Planets.RainfallTip".Translate());
            //
            // TEMPERATURE SLIDER
            //
            single += 40f;
            Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetTemperature".Translate());
            Rect rect5 = new Rect(200f, single, 200f, 30f);
            OverallTemperature temperatureCheck = this.temperature;
            this.temperature = (OverallTemperature)Mathf.RoundToInt(Widgets.HorizontalSlider(rect5, (float)this.temperature, 0f, (float)(OverallTemperatureUtility.EnumValuesCount - 1), true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
            if (this.temperature != temperatureCheck) { this.worldPreset = "Planets.Custom"; }

            //
            // POPULATION SLIDER
            //
            single += 40f;
            Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetPopulation".Translate());
            Rect rect55 = new Rect(200f, single, 200f, 30f);
            OverallPopulation populationCheck = this.population;
            this.population = (OverallPopulation)Mathf.RoundToInt(Widgets.HorizontalSlider(rect55, (float)this.population, 0f, (float)(OverallPopulationUtility.EnumValuesCount - 1), true, "PlanetPopulation_Normal".Translate(), "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate(), 1f));
            if (this.population != populationCheck) { this.worldPreset = "Planets.Custom"; }

            //
            // AXIAL TILT
            //
            single += 40f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.AxialTilt".Translate());
			Rect rect9 = new Rect(200f, single, 200f, 30f);
			AxialTilt axialTiltCheck = Planets_GameComponent.axialTilt;
			Planets_GameComponent.axialTilt = (AxialTilt)Mathf.RoundToInt(Widgets.HorizontalSlider(rect9, (float)Planets_GameComponent.axialTilt, 0f, (float)(AxialTiltUtility.EnumValuesCount - 1), true, "Planets.AxialTilt_Normal".Translate(), "Planets.AxialTilt_Low".Translate(), "Planets.AxialTilt_High".Translate(), 1f));
			if (Planets_GameComponent.axialTilt != axialTiltCheck) { this.worldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect9.xMax, rect9.height), "Planets.AxialTiltTip".Translate());
			GUI.EndGroup();
			//
			// BOTTOM BUTTONS
			//
			base.DoBottomButtons(rect, "WorldGenerate".Translate(), "Planets.Random".Translate(), new Action(this.Randomize), true, true);
		}
		public override void PostOpen() {
			base.PostOpen();
			TutorSystem.Notify_Event("PageStart-CreateWorldParams");
		}
		public override void PreOpen() {
			base.PreOpen();
			if (!this.initialized) {
				this.Reset();
				this.initialized = true;
			}
		}
		public void Reset() {
			this.seedString = GenText.RandomSeedString();
			this.planetCoverage = 0.3f;
			this.worldPreset = "Planets.Vanilla";
			Planets_GameComponent.axialTilt = AxialTilt.Normal;
			Planets_GameComponent.worldType = WorldType.Vanilla;
			Planets_GameComponent.subcount = 10;
			this.rainfallMod = RainfallModifier.Normal;
			this.temperature = OverallTemperature.Normal;
            this.population = OverallPopulation.Normal;
		}
		public void Randomize() {
			this.seedString = GenText.RandomSeedString();
			float randTilt = Rand.Value;
			if (randTilt > 0.8) { Planets_GameComponent.axialTilt = AxialTilt.VeryLow; }
			else if (randTilt > 0.6) { Planets_GameComponent.axialTilt = AxialTilt.Low; }
			else if (randTilt > 0.4) { Planets_GameComponent.axialTilt = AxialTilt.Normal; }
			else if (randTilt > 0.2) { Planets_GameComponent.axialTilt = AxialTilt.High; }
			else { Planets_GameComponent.axialTilt = AxialTilt.VeryHigh; }
			float randType = Rand.Value;
			if (randType > 0.86) { Planets_GameComponent.worldType = WorldType.Waterworld; }
			else if (randType > 0.72) { Planets_GameComponent.worldType = WorldType.Islands; }
			else if (randType > 0.58) { Planets_GameComponent.worldType = WorldType.Earthlike; }
			else if (randType > 0.42) { Planets_GameComponent.worldType = WorldType.Vanilla; }
			else if (randType > 0.28) { Planets_GameComponent.worldType = WorldType.Dry; }
			else if (randType > 0.14) { Planets_GameComponent.worldType = WorldType.VeryDry; }
			else { Planets_GameComponent.worldType = WorldType.Barren; }
			float randRain = Rand.Value;
			if (randRain > 0.8) { this.rainfallMod = RainfallModifier.Little; }
			else if (randRain > 0.6) { this.rainfallMod = RainfallModifier.LittleBitLess; }
			else if (randRain > 0.4) { this.rainfallMod = RainfallModifier.Normal; }
			else if (randRain > 0.2) { this.rainfallMod = RainfallModifier.LittleBitMore; }
			else { this.rainfallMod = RainfallModifier.High; }
			float randTemp = Rand.Value;
			if (randTemp > 0.86) { this.temperature = OverallTemperature.VeryCold; }
			else if (randTemp > 0.72) { this.temperature = OverallTemperature.Cold; }
			else if (randTemp > 0.58) { this.temperature = OverallTemperature.LittleBitColder; }
			else if (randTemp > 0.42) { this.temperature = OverallTemperature.Normal; }
			else if (randTemp > 0.28) { this.temperature = OverallTemperature.LittleBitWarmer; }
			else if (randTemp > 0.14) { this.temperature = OverallTemperature.Hot; }
			else { this.temperature = OverallTemperature.VeryHot; }
            float randPop = Rand.Value;
            if (randPop > 0.86) { this.population = OverallPopulation.AlmostNone; }
            else if (randPop > 0.72) { this.population = OverallPopulation.Little; }
            else if (randPop > 0.58) { this.population = OverallPopulation.LittleBitLess; }
            else if (randPop > 0.42) { this.population = OverallPopulation.Normal; }
            else if (randPop > 0.28) { this.population = OverallPopulation.LittleBitMore; }
            else if (randPop > 0.14) { this.population = OverallPopulation.High; }
            else { this.population = OverallPopulation.VeryHigh; }
            Controller.Settings.randomPlanet = true;
			if (this.CanDoNext()) {
				this.DoNext();
			}
		}
	}

	[HarmonyPatch(typeof(WorldGenStep_Terrain), "SetupElevationNoise", null)]
	public static class WorldGenStep_Terrain_SetupElevationNoise {
		public static bool Prefix(ref FloatRange ___ElevationRange) {
			if (Planets_GameComponent.worldType == WorldType.Waterworld) {
				___ElevationRange = new FloatRange(-2100f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Islands) {
				___ElevationRange = new FloatRange(-1500f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Earthlike) {
				___ElevationRange = new FloatRange(-1000f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Vanilla) {
				___ElevationRange = new FloatRange(-600f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Dry) {
				___ElevationRange = new FloatRange(-300f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.VeryDry) {
				___ElevationRange = new FloatRange(-100f, 5000f);
			}
			else {
				___ElevationRange = new FloatRange(0f, 5000f);
			}
			return true;
		}
	}

	//
	// FACTION GENERATION CHANGES
	//
	
	[HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld", null)]
	public static class FactionGenerator_GenerateFactionsIntoWorld {
		[HarmonyPriority(Priority.High)]
		public static bool Prefix() {
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				return true;
			}
			Controller.factionCenters.Clear();
			int actualFactionCount = 0;
			foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs) {
				if (!allDef.hidden) {
					actualFactionCount += allDef.requiredCountAtGameStart;
				}
			}
			Controller.minFactionSeparation = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*2);
			if (Controller.Settings.factionGrouping < 1) {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount);
			}
			else if (Controller.Settings.factionGrouping < 2) {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*1.5);
			}
			else if (Controller.Settings.factionGrouping < 3) {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*2.25);
			}
			else {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*3);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction", new Type[] { typeof(FactionDef) })]
	public static class FactionGenerator_NewGeneratedFaction {
		[HarmonyPriority(Priority.High)]
		public static bool Prefix(FactionDef facDef, ref Faction __result) {
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				return true;
			}
			Faction faction = new Faction();
			faction.def = facDef;
			faction.loadID = Find.UniqueIDsManager.GetNextFactionID();
			faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
			if (!facDef.isPlayer) {
				if (facDef.fixedName == null) {
					faction.Name = NameGenerator.GenerateName(facDef.factionNameMaker, 
					from fac in Find.FactionManager.AllFactionsVisible
					select fac.Name, false, null);
				}
				else {
					faction.Name = facDef.fixedName;
				}
			}
			faction.centralMelanin = Rand.Value;
			foreach (Faction allFactionsListForReading in Find.FactionManager.AllFactionsListForReading) {
				faction.TryMakeInitialRelationsWith(allFactionsListForReading);
			}
			faction.GenerateNewLeader();
			if (!facDef.hidden && !facDef.isPlayer) {
				Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				settlement.SetFaction(faction);
				settlement.Tile = TileFinder.RandomSettlementTileFor(faction, false, null);
				settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
				Find.WorldObjects.Add(settlement);
			}
			__result = faction;
			return false;
		}
	}

	[HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor", null)]
	public static class TileFinder_RandomSettlementTileFor {
		[HarmonyPriority(Priority.High)]
		public static bool Prefix(Faction faction, ref int __result, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null) {
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				return true;
			}
			float minTemp = -500f;
			float maxTemp = 500f;
			if (faction != null) {
				if (!faction.IsPlayer) {
					if (faction.leader != null) {
						minTemp = faction.leader.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null);
						maxTemp = faction.leader.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
					}
				}
			}
			int num;
			for (int i = 0; i < 2500; i++) {
				if ((
				from _ in Enumerable.Range(0, 100)
				select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight<int>((int x) => {
					Tile item = Find.WorldGrid[x];
					if (!item.biome.canBuildBase || !item.biome.implemented || item.hilliness == Hilliness.Impassable) {
						return 0f;
					}
					if (mustBeAutoChoosable && !item.biome.canAutoChoose) {
						return 0f;
					}
					if (extraValidator != null && !extraValidator(x)) {
						return 0f;
					}
					if (Controller.Settings.checkTemp.Equals(true)) {
						if (i < 1000) {
							if (item.temperature < (minTemp-45) || item.temperature > (maxTemp+45)) {
								return 0f;
							}
							if (item.temperature < (minTemp-36) || item.temperature > (maxTemp+36)) {
								if (Rand.Value > 0.1f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-28) || item.temperature > (maxTemp+28)) {
								if (Rand.Value > 0.2f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-21) || item.temperature > (maxTemp+21)) {
								if (Rand.Value > 0.3f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-15) || item.temperature > (maxTemp+15)) {
								if (Rand.Value > 0.4f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-10) || item.temperature > (maxTemp+10)) {
								if (Rand.Value > 0.5f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-6) || item.temperature > (maxTemp+6)) {
								if (Rand.Value > 0.6f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-3) || item.temperature > (maxTemp+3)) {
								if (Rand.Value > 0.7f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-1) || item.temperature > (maxTemp+1)) {
								if (Rand.Value > 0.8f) {
									return 0f;
								}
							}
							if (item.temperature < minTemp || item.temperature > maxTemp) {
								if (Rand.Value > 0.9f) {
									return 0f;
								}
							}
						}
						else if (i < 1500) {
							if (item.temperature < (minTemp-45) || item.temperature > (maxTemp+45)) {
								if (Rand.Value > 0.2f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-36) || item.temperature > (maxTemp+36)) {
								if (Rand.Value > 0.4f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-28) || item.temperature > (maxTemp+28)) {
								if (Rand.Value > 0.6f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-21) || item.temperature > (maxTemp+21)) {
								if (Rand.Value > 0.8f) {
									return 0f;
								}
							}
						}
					}
					return item.biome.settlementSelectionWeight;
				}, out num)) {
					if (TileFinder.IsValidTileForNewSettlement(num, null)) {
						if (faction == null || faction.def.hidden.Equals(true) || faction.def.isPlayer.Equals(true)) {
							__result = num;
							return false;
						}
						else if (Controller.factionCenters.ContainsKey(faction)) { 
							float test = Find.WorldGrid.ApproxDistanceInTiles(Controller.factionCenters[faction],num);
							if (faction.def.defName == "Pirate" || faction.def.defName == "TribalRaiders") {
								if (test < (Controller.maxFactionSprawl*3)) {
									__result = num;
									return false;
								}
							}
							else {	
								if (test < Controller.maxFactionSprawl) { 
									__result = num;
									return false;
								}
							}
						}
						else {
							bool locationOK = true;
							foreach (KeyValuePair<Faction,int> factionCenter in Controller.factionCenters) {
								float test = Find.WorldGrid.ApproxDistanceInTiles(factionCenter.Value,num);
								if (test < Controller.minFactionSeparation) {
									locationOK = false;
								}
							}
							if (locationOK.Equals(true)) {
								__result = num;
								Controller.factionCenters.Add(faction, num);
								return false;
							}
						}
					}
				}
			}
			Log.Warning(string.Concat("Failed to find faction base tile for ", faction));
			if (Controller.failureCount.ContainsKey(faction)) {
				Controller.failureCount[faction]++;
				if (Controller.failureCount[faction] == 10) {
					Controller.failureCount.Remove(faction);
					if (Controller.factionCenters.ContainsKey(faction)) {
						Controller.factionCenters.Remove(faction);
						Log.Warning("  Relocating faction center.");
					}
				}
			}
			else {
				Log.Warning("  Retrying.");
				Controller.failureCount.Add(faction,1);
			}
			__result = 0;
			return false;
		}
	}
	
}
