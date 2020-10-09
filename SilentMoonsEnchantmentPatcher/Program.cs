// /*
//     Copyright (C) 2020  erri120
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Noggog;
using Wabbajack.Common;

namespace SilentMoonsEnchantmentPatcher
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args, RunPatch, new UserPreferences
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher
                    {
                        IdentifyingModKey = "LunarWeaponsPatch.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        //TODO: settings
        private const int DragonboneLevel = 50;
        
        private static readonly ModKey SkyrimModKey = ModKey.FromNameAndExtension("Skyrim.esm");
        private static readonly ModKey DawnguardModKey = ModKey.FromNameAndExtension("Dawnguard.esm");

        private static readonly ModKey SailorMoonModKey = ModKey.FromNameAndExtension("SailorMoonClub.esp");
        private static readonly FormKey LunarForgeUnenchantedFormIDListFormKey = new FormKey(SailorMoonModKey, 0x81EA);
        private static readonly FormKey LunarForgeLunarBasicFormIDListFormKey = new FormKey(SailorMoonModKey, 0x81EB);
        private static readonly FormKey LunarForgeAwakenedLunarFormIDListFormKey = new FormKey(SailorMoonModKey, 0x81EC);
        private static readonly FormKey LunarForgeLunarAbsorbFormIDListFormKey = new FormKey(SailorMoonModKey, 0x81ED);
        
        //was hardcoded in the original
        private static readonly WeaponTierData CrossbowDamage = new WeaponTierData(19, 22, 23, 27, 30);
        
        private static readonly FormKey[] WeaponKeywords = {
            Skyrim.Keyword.WeapTypeSword,
            Skyrim.Keyword.WeapTypeWarAxe,
            Skyrim.Keyword.WeapTypeMace,
            Skyrim.Keyword.WeapTypeDagger,
            Skyrim.Keyword.WeapTypeGreatsword,
            Skyrim.Keyword.WeapTypeBattleaxe,
            Skyrim.Keyword.WeapTypeWarhammer,
            Skyrim.Keyword.WeapTypeBow,
        };

        private static readonly string[] EnchantmentTypes =
        {
            "LunarDamage",
            "LunarDamagePlus",
            "LunarAbsorbHealth"
        };

        private static readonly string[] WeaponPrefixes =
        {
            "Iron",
            "Orcish",
            "Elven",
            "Ebony"
        };

        private static readonly string[] WeaponTypes =
        {
            "Sword",
            "WarAxe",
            "Mace",
            "Greatsword",
            "Battleaxe",
            "Warhammer",
            "Dagger",
            "Bow"
        };
        
        private static readonly IReadOnlyList<EnchantmentWeaponTier> EnchantmentWeaponTiers = new List<EnchantmentWeaponTier>
        {
            //really low quality
            new EnchantmentWeaponTier(1, new int[0]),
            //iron or steel quality
            new EnchantmentWeaponTier(1, new []{2, 3}),
            //orcish/dwarven quality
            new EnchantmentWeaponTier(1, new []{2, 3, 4}),
            //elven/glass quality
            new EnchantmentWeaponTier(2, new []{3, 4, 5}),
            //ebony/daedric quality
            new EnchantmentWeaponTier(3, new []{4, 5, 6}),
            //dragon quality
            new EnchantmentWeaponTier(5, new []{6})
        };

        private static IReadOnlyDictionary<string, List<EnchantmentData>> GetEnchantmentsData(ISkyrimModGetter mod, IReadOnlyDictionary<string, LunarEnchantmentData> enchantmentData)
        {
            return EnchantmentTypes.Select(enchantmentType =>
                {
                    var data = enchantmentData[enchantmentType];

                    List<EnchantmentData>? list = data.Tiers?.Select(tier =>
                    {
                        var edid = $"EnchWeapon{enchantmentType}0{tier.ID}";
                        var effect = mod.ObjectEffects.First(x =>
                            x.EditorID != null && x.EditorID.Equals(edid, StringComparison.OrdinalIgnoreCase));

                        var pts = data.BasePts + tier.ID * data.LevelPts;

                        var template = new EnchantmentData(data, tier, effect)
                        {
                            Pts = (ushort) pts,
                        };

                        return template;
                    }).ToList();

                    return (enchantmentType, list);
                }).Where(x => x.list != null)
                .ToDictionary(x => x.enchantmentType, x => x.list!, StringComparer.OrdinalIgnoreCase);
        }

        private static ushort GetDamage(string edid, IReadOnlyDictionary<string?, IWeaponGetter> weapons)
        {
            if (!weapons.TryGetValue(edid, out var weaponGetter)) return 0;
            return weaponGetter.BasicStats?.Damage ?? 0;
        }

        private static WeaponTierData GetWeaponTierData(string suffix, IReadOnlyDictionary<string?, IWeaponGetter> skyrimWeapons, IReadOnlyDictionary<string?, IWeaponGetter> dawnguardWeapons)
        {
            return new WeaponTierData(
                GetDamage($"Iron{suffix}", skyrimWeapons),
                GetDamage($"Orcish{suffix}", skyrimWeapons),
                GetDamage($"Elven{suffix}", skyrimWeapons),
                GetDamage($"Ebony{suffix}", skyrimWeapons),
                GetDamage($"DLC1Dragonbone{suffix}", dawnguardWeapons));
        }
        
        private static IReadOnlyDictionary<string, WeaponTierData> GetWeaponTierDataDictionary(ISkyrimModGetter skyrim, ISkyrimModGetter dawnguard)
        {
            IReadOnlyDictionary<string?, IWeaponGetter> skyrimWeapons = skyrim.Weapons
                .Where(x => x.EditorID != null && WeaponPrefixes.Any(y => x.EditorID.StartsWith(y, StringComparison.OrdinalIgnoreCase)))
                .ToDictionary(x => x.EditorID, x => x, StringComparer.OrdinalIgnoreCase);
            IReadOnlyDictionary<string?, IWeaponGetter> dragonBoneWeapons = dawnguard.Weapons
                .Where(x => x.EditorID != null && x.EditorID.StartsWith("DLC1Dragonbone"))
                .ToDictionary(x => x.EditorID, x => x, StringComparer.OrdinalIgnoreCase);

            Dictionary<string, WeaponTierData> tierData =
                new Dictionary<string, WeaponTierData>(StringComparer.OrdinalIgnoreCase)
                {
                    {"1HSword", GetWeaponTierData("Sword", skyrimWeapons, dragonBoneWeapons)},
                    {"1HAxe", GetWeaponTierData("WarAxe", skyrimWeapons, dragonBoneWeapons)},
                    {"1HHammer", GetWeaponTierData("Mace", skyrimWeapons, dragonBoneWeapons)},
                    {"1HDagger", GetWeaponTierData("Dagger", skyrimWeapons, dragonBoneWeapons)},
                    {"2HSword", GetWeaponTierData("Greatsword", skyrimWeapons, dragonBoneWeapons)},
                    {"2HAxe", GetWeaponTierData("Battleaxe", skyrimWeapons, dragonBoneWeapons)},
                    {"2HHammer", GetWeaponTierData("Warhammer", skyrimWeapons, dragonBoneWeapons)},
                    {
                        "Bow", new WeaponTierData(
                            GetDamage("LongBow", skyrimWeapons),
                            GetDamage("OrcishBow", skyrimWeapons),
                            GetDamage("ElvenBow", skyrimWeapons),
                            GetDamage("EbonyBow", skyrimWeapons),
                            GetDamage("DLC1DragonboneBow", dragonBoneWeapons))
                    },
                    {"Crossbow", CrossbowDamage}
                };

            return tierData;
        }

        private static List<WeaponDamageLevel> GetWeaponDamageLevel(FormKey formKey, ILinkCache linkCache)
        {
            if (!linkCache.TryLookup<ILeveledItemGetter>(formKey, out var leveledItem)) 
                return new List<WeaponDamageLevel>();
            if (leveledItem.Entries == null) 
                return new List<WeaponDamageLevel>();
            List<ILeveledItemEntryGetter> entries = leveledItem.Entries
                .Where(x => x.Data != null)
                .DistinctBy(x => x.Data!.Reference)
                .ToList();
            var minLevel = entries.Min(x => x.Data!.Level);
            List<WeaponDamageLevel> result = entries.Select(x =>
            {
                if (!x.Data!.Reference.TryResolve(linkCache, out var recordGetter)) return default;
                if (!(recordGetter is IWeaponGetter weaponGetter)) return default;
                //if (!weaponReference.TryResolve<IWeaponGetter>(linkCache, out var weaponGetter)) return default;
                if (weaponGetter.BasicStats == null) return default;
                var damage = weaponGetter.BasicStats.Damage;
                return new WeaponDamageLevel(minLevel, damage);
            }).Where(x => x.Level > 0).ToList();
            return result;
        }

        private static IReadOnlyDictionary<string, List<WeaponDamageLevel>> GetDamageLevelData(ILinkCache linkCache, IReadOnlyDictionary<string, WeaponTierData> weaponTiers)
        {
            var result = new Dictionary<string, List<WeaponDamageLevel>>
            {
                {"1HSword", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponSword, linkCache)},
                {"1HAxe", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponWarAxe, linkCache)},
                {"1HHammer", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponMace, linkCache)},
                {"1HDagger", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponDagger, linkCache)},
                {"2HSword", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponGreatSword, linkCache)},
                {"2HAxe", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponBattleAxe, linkCache)},
                {"2HHammer", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponWarhammer, linkCache)},
                {"Bow", GetWeaponDamageLevel(Skyrim.LeveledItem.LItemWeaponBow, linkCache)}
            };

            result.ForEach(x =>
            {
                var (key, value) = x;
                value.Add(new WeaponDamageLevel(DragonboneLevel, weaponTiers[key].Dragon));
            });

            return result;
        }
        
        private static string? GetWeaponTypeByKeywords(IEnumerable<IFormLink<IKeywordGetter>>? keywords)
        {
            if (keywords == null) return null;
            try
            {
                IFormLink<IKeywordGetter>? formKey = keywords.FirstOrDefault(x => WeaponKeywords.Contains(x.FormKey));
                if (formKey == null) return null;

                var id = formKey.FormKey.ID;
                if (id == Skyrim.Keyword.WeapTypeSword.ID)
                    return "1HSword";
                if (id == Skyrim.Keyword.WeapTypeWarAxe.ID)
                    return "1HAxe";
                if (id == Skyrim.Keyword.WeapTypeMace.ID)
                    return "1HHammer";
                if (id == Skyrim.Keyword.WeapTypeDagger.ID)
                    return "1HDagger";
                if (id == Skyrim.Keyword.WeapTypeGreatsword.ID)
                    return "2HSword";
                if (id == Skyrim.Keyword.WeapTypeBattleaxe.ID)
                    return "2HAxe";
                if (id == Skyrim.Keyword.WeapTypeWarhammer.ID)
                    return "2HHammer";
                if (id == Skyrim.Keyword.WeapTypeBow.ID)
                    return "Bow";
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        private static int GetDamageTier(IWeaponGetter weaponGetter, IReadOnlyDictionary<string, WeaponTierData> weaponTiers)
        {
            var type = GetWeaponTypeByKeywords(weaponGetter.Keywords);
            if (type == null) return 0;
            var damageTiers = weaponTiers[type];
            var damage = weaponGetter.BasicStats?.Damage ?? 0;
            if (damage < damageTiers.IronSteel)
                return 0;
            if (damage < damageTiers.OrcishDwarven)
                return 1;
            if (damage < damageTiers.ElvenGlass)
                return 2;
            if (damage < damageTiers.EbonyDaedric)
                return 3;
            if (damage < damageTiers.Dragon)
                return 4;
            return 5;
        }
        
        private static EnchantmentWeaponTier GetEnchantmentTiersToMake(string enchantmentType, IWeaponGetter weaponGetter, IReadOnlyDictionary<string, WeaponTierData> weaponTiers)
        {
            var damageTier = GetDamageTier(weaponGetter, weaponTiers);
            if (damageTier == 1 && enchantmentType.Equals("LunarAbsorbHealth", StringComparison.OrdinalIgnoreCase))
                return new EnchantmentWeaponTier(1, new int[0]);
            return EnchantmentWeaponTiers[damageTier];
        }

        private static Weapon MakeEnchantedWeapon(IWeaponGetter weaponGetter, EnchantmentData enchantmentData, SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var newEDID = enchantmentData.NewEDID(weaponGetter);
            Console.WriteLine($"Creating new enchanted weapon {newEDID}");
            
            var newWeapon = state.PatchMod.Weapons.AddNew();
            newWeapon.DeepCopyIn(weaponGetter);
            
            newWeapon.EditorID = newEDID;
            newWeapon.Name = enchantmentData.NewName(weaponGetter);
            newWeapon.ObjectEffect = new FormLinkNullable<IEffectRecordGetter>(enchantmentData.ObjectEffectGetter.FormKey);
            newWeapon.EnchantmentAmount = enchantmentData.Pts;
            newWeapon.Template = new FormLinkNullable<IWeaponGetter>(weaponGetter.FormKey);
            
            return newWeapon;
        }

        private static void AddWeaponToLeveledList(ILeveledItem lItemForge, IMajorRecordInternal baseWeapon, short level)
        {
            lItemForge.Entries ??= new ExtendedList<LeveledItemEntry>();
                            
            lItemForge.Entries.Add(new LeveledItemEntry
            {
                Data = new LeveledItemEntryData
                {
                    Count = 1,
                    Level = level,
                    Reference = new FormLink<IItemGetter>(baseWeapon.FormKey)
                }
            });
        }

        private static short GetWeaponLevel(IWeapon weapon, EnchantmentData enchantmentData, IReadOnlyDictionary<string, List<WeaponDamageLevel>> damageLevelData, IReadOnlyDictionary<string, WeaponTierData> weaponTiers)
        {
            var weaponType = GetWeaponTypeByKeywords(weapon.Keywords);
            if (weapon.BasicStats == null)
                return 0;
            if (weaponType == null)
                return 0;
            if (enchantmentData.Tier.LevelMod == null)
                return 0;
            
            List<WeaponDamageLevel> damageLevel = damageLevelData[weaponType];
            var damage = weapon.BasicStats.Damage;
            var baseLevel = damageLevel
                .Where(x => x.Damage >= damage)
                .OrderBy(x => x.Damage)
                .First()
                .Level;
            var damageTier = GetDamageTier(weapon, weaponTiers);
            var enchantmentLevel = enchantmentData.Tier.LevelMod[$"{damageTier}"];
            return (short) (baseLevel + enchantmentLevel);
        }
        
        private static short GetWeaponLevel(IWeaponGetter weapon, EnchantmentData enchantmentData, IReadOnlyDictionary<string, List<WeaponDamageLevel>> damageLevelData, IReadOnlyDictionary<string, WeaponTierData> weaponTiers)
        {
            var weaponType = GetWeaponTypeByKeywords(weapon.Keywords);
            if (weapon.BasicStats == null)
                return 0;
            if (weaponType == null)
                return 0;
            if (enchantmentData.Tier.LevelMod == null)
                return 0;
            
            List<WeaponDamageLevel> damageLevel = damageLevelData[weaponType];
            var damage = weapon.BasicStats.Damage;
            var baseLevel = damageLevel
                .Where(x => x.Damage >= damage)
                .OrderBy(x => x.Damage)
                .First()
                .Level;
            var damageTier = GetDamageTier(weapon, weaponTiers);
            if (!enchantmentData.Tier.LevelMod.ContainsKey($"{damageTier}"))
                throw new NotImplementedException();
            var enchantmentLevel = enchantmentData.Tier.LevelMod[$"{damageTier}"];
            return (short) (baseLevel + enchantmentLevel);
        }
        
        private static short MinLevel(ILeveledItem leveledItem)
        {
            if (leveledItem.Entries == null) return 0;
            return leveledItem.Entries
                .Where(x => x.Data != null)
                .Select(x => x.Data!.Level)
                .Min();
        }
        
        private static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var lunarEnchantmentData = JsonUtils.FromJson<Dictionary<string, LunarEnchantmentData>>("LunarEnchData.json");
            if (lunarEnchantmentData.Count == 0)
                throw new ArgumentException("Enchantment Data List Count is null!");
            
            if (!state.LoadOrder.ContainsKey(SailorMoonModKey))
                throw new ArgumentException("Required plugin SailorMoonClub.esp does not exist in load order!");
            
            if (!state.LoadOrder.TryGetValue(SailorMoonModKey, out IModListing<ISkyrimModGetter>? sailorMoonMod))
                throw new ArgumentException("Unable to get SailorMoonClub.esp plugin");

            if (!state.LoadOrder.TryGetValue(SkyrimModKey, out IModListing<ISkyrimModGetter>? skyrimListing))
                throw new NotImplementedException();
            
            if (!state.LoadOrder.TryGetValue(DawnguardModKey, out IModListing<ISkyrimModGetter>? dawnguardListing))
                throw new NotImplementedException();
            
            if (!state.LinkCache.TryLookup<IFormListGetter>(LunarForgeUnenchantedFormIDListFormKey, out var unenchantedFormIDListGetter))
                throw new Exception($"Unable to find {LunarForgeUnenchantedFormIDListFormKey}");
            if (!state.LinkCache.TryLookup<IFormListGetter>(LunarForgeAwakenedLunarFormIDListFormKey, out var awakenedFormIDListGetter))
                throw new Exception($"Unable to find {LunarForgeAwakenedLunarFormIDListFormKey}");
            if (!state.LinkCache.TryLookup<IFormListGetter>(LunarForgeLunarAbsorbFormIDListFormKey, out var lunarAbsorbFormIDListGetter))
                throw new Exception($"Unable to find {LunarForgeLunarAbsorbFormIDListFormKey}");
            if (!state.LinkCache.TryLookup<IFormListGetter>(LunarForgeLunarBasicFormIDListFormKey, out var lunarBasicFormIDListGetter))
                throw new Exception($"Unable to find {LunarForgeLunarBasicFormIDListFormKey}");

            IReadOnlyDictionary<string, List<EnchantmentData>> enchantments = GetEnchantmentsData(sailorMoonMod.Mod!, lunarEnchantmentData);
            IReadOnlyDictionary<string, WeaponTierData> weaponTiers = GetWeaponTierDataDictionary(skyrimListing.Mod!, dawnguardListing.Mod!);
            IReadOnlyDictionary<string, List<WeaponDamageLevel>> damageLevelData = GetDamageLevelData(state.LinkCache, weaponTiers);
            
            var noEnchantmentFormList = state.PatchMod.FormLists.GetOrAddAsOverride(unenchantedFormIDListGetter.DeepCopy());
            var lunarDamageFormList = state.PatchMod.FormLists.GetOrAddAsOverride(lunarBasicFormIDListGetter.DeepCopy());
            var lunarDamagePlusFormList = state.PatchMod.FormLists.GetOrAddAsOverride(awakenedFormIDListGetter.DeepCopy());
            var lunarAbsorbHealthList = state.PatchMod.FormLists.GetOrAddAsOverride(lunarAbsorbFormIDListGetter.DeepCopy());

            FormList GetFormList(string name)
            {
                return name switch
                {
                    "NoEnch" => noEnchantmentFormList!,
                    "LunarDamage" => lunarDamageFormList!,
                    "LunarDamagePlus" => lunarDamagePlusFormList!,
                    "LunarAbsorbHealth" => lunarAbsorbHealthList!,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            //filtering
            ParallelQuery<IWeaponGetter> weaponRecordsToPatch = state.LoadOrder.PriorityOrder
                .WinningOverrides<IConstructibleObjectGetter>()
                .AsParallel()
                //.WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                //.WithMergeOptions(ParallelMergeOptions.Default)
                .Select(constructibleObject =>
                {
                    //filter recipes that create un-enchanted weapons at forges
                    if (!constructibleObject.CreatedObject.TryResolve(state.LinkCache, out var createdObject))
                        return null;
                    if (!constructibleObject.WorkbenchKeyword.TryResolve(state.LinkCache, out var workbenchKeyword))
                        return null;

                    if (!workbenchKeyword.FormKey.Equals(Skyrim.Keyword.CraftingSmithingForge)) return null;

                    if (!(createdObject is IWeaponGetter weaponRecord)) return null;
                    if (!weaponRecord.ObjectEffect.IsNull) return null;
                    if (weaponRecord.Keywords == null) return null;
                    if (!weaponRecord.Keywords.Any(x => WeaponKeywords.Contains(x.FormKey))) return null;

                    return weaponRecord;
                })
                .Where(x => x != null)
                .Select(x => x!);
            
            //patching
            foreach (var weaponRecord in weaponRecordsToPatch)
            {
                Console.WriteLine($"Patching weapon {weaponRecord.EditorID} ({weaponRecord.FormKey})");

                noEnchantmentFormList.Items.Add(weaponRecord);

                foreach (KeyValuePair<string, List<EnchantmentData>> pair in enchantments)
                {
                    var (type, _) = pair;
                    var enchantmentTier = GetEnchantmentTiersToMake(type, weaponRecord, weaponTiers);
                    List<EnchantmentData>? enchantmentData = enchantments[type];
                    var enchantment = enchantmentData.First(x => x.Tier.ID == enchantmentTier.BaseTier);
                    var enchantedFormList = GetFormList(type);

                    var baseWeapon = MakeEnchantedWeapon(weaponRecord, enchantment, state);
                    if (enchantmentTier.Tiers.Count == 0)
                    {
                        enchantedFormList.Items.Add(baseWeapon);
                    } else if (type.Equals("LunarDamage", StringComparison.OrdinalIgnoreCase))
                    {
                        var lvlWorldEDID = $"LItemEnch{weaponRecord.EditorID}{type}";
                        var lItemWorld = state.PatchMod.LeveledItems.AddNew(lvlWorldEDID);
                        var lvlForgeEDID = $"LItemEnch{weaponRecord.EditorID}{type}Forge";
                        var lItemForge = state.PatchMod.LeveledItems.AddNew(lvlForgeEDID);
                        
                        enchantedFormList.Items.Add(new FormLink<ISkyrimMajorRecordGetter>(lItemForge.FormKey));
                        AddWeaponToLeveledList(lItemForge, baseWeapon, 1);
                        if (enchantmentTier.Tiers.Count < 3)
                        {
                            var level = GetWeaponLevel(baseWeapon, enchantment, damageLevelData, weaponTiers);
                            AddWeaponToLeveledList(lItemWorld, baseWeapon, level);
                        }

                        foreach (var tier in enchantmentTier.Tiers)
                        {
                            enchantment = enchantmentData.First(x => x.Tier.ID == tier);
                            var newWeapon = MakeEnchantedWeapon(weaponRecord, enchantment, state);
                            var level = GetWeaponLevel(weaponRecord, enchantment, damageLevelData, weaponTiers);
                            AddWeaponToLeveledList(lItemWorld, newWeapon, level);
                            AddWeaponToLeveledList(lItemForge, newWeapon, level);
                        }
                    }
                    else
                    {
                        var lvlForgeEDID = $"LItemEnch{weaponRecord.EditorID}{type}Forge";
                        var lItemForge = state.PatchMod.LeveledItems.AddNew(lvlForgeEDID);
                        enchantedFormList.Items.Add(lItemForge);
                        AddWeaponToLeveledList(lItemForge, baseWeapon, 1);
                        
                        foreach (var tier in enchantmentTier.Tiers)
                        {
                            enchantment = enchantmentData.First(x => x.Tier.ID == tier);
                            var newWeapon = MakeEnchantedWeapon(weaponRecord, enchantment, state);
                            var level = GetWeaponLevel(weaponRecord, enchantment, damageLevelData, weaponTiers);
                            AddWeaponToLeveledList(lItemForge, newWeapon, level);
                        }
                    }
                }
            }

            //finalizing
            Dictionary<string, LeveledItem> baseLItems = sailorMoonMod!.Mod!.LeveledItems
                .Select(x =>
                {
                    if (x.EditorID == null) return ("", x);
                    if (!x.EditorID.StartsWith("LunarEnchWeapon")) return ("", x);
                    var wType = x.EditorID.Substring("LunarEnchWeapon".Length);
                    var match = WeaponTypes.Any(y => y.Equals(wType, StringComparison.OrdinalIgnoreCase));
                    return !match ? ("", x) : (wType, x);
                })
                .Where(x => !x.Item1.Equals(string.Empty))
                .Select(x =>
                {
                    var copy = state.PatchMod.LeveledItems.GetOrAddAsOverride(x.x);
                    return (x.Item1, copy);
                })
                .ToDictionary(x => x.Item1, x => x.Item2, StringComparer.OrdinalIgnoreCase);
            
            List<LeveledItem> records = state.PatchMod.LeveledItems
                .Where(x => x.EditorID != null && x.EditorID.EndsWith("LunarDamage", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (KeyValuePair<string, LeveledItem> pair in baseLItems)
            {
                List<LeveledItem> lItems = records
                    .Where(x => 
                        x.EditorID != null &&
                        x.EditorID.EndsWith($"{pair.Key}LunarDamage", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                lItems.ForEach(x =>
                {
                    AddWeaponToLeveledList(pair.Value, x, MinLevel(x));
                });
            }
        }
    }
}
