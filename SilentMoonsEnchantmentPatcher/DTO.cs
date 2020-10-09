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

using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using Newtonsoft.Json;

namespace SilentMoonsEnchantmentPatcher
{
    public class LunarEnchantmentData
    {
        [JsonProperty("tiers")]
        public List<Tier>? Tiers { get; set; }
        
        [JsonProperty("edidMod")]
        public string? EDIDMod { get; set; }
        
        [JsonProperty("LevelPts")]
        public int LevelPts { get; set; }
        
        [JsonProperty("BasePts")]
        public int BasePts { get; set; }
    }

    public class Tier
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        
        [JsonProperty("prefix")]
        public string? Prefix { get; set; }
        
        [JsonProperty("Suffix")]
        public string? Suffix { get; set; }
        
        [JsonProperty("levelMod")]
        public Dictionary<string, int>? LevelMod { get; set; }
    }

    public class EnchantmentData
    {
        private readonly LunarEnchantmentData _lunarEnchantmentData;
        public readonly Tier Tier;
        public readonly IObjectEffectGetter ObjectEffectGetter;
        
        public ushort Pts { get; set; }
        
        public EnchantmentData(LunarEnchantmentData lunarEnchantmentData, Tier tier, IObjectEffectGetter objectEffectGetter)
        {
            _lunarEnchantmentData = lunarEnchantmentData;
            Tier = tier;
            ObjectEffectGetter = objectEffectGetter;
        }

        public string NewEDID(IWeaponGetter record)
        {
            return $"Ench{record.EditorID}{_lunarEnchantmentData.EDIDMod}0{Tier.ID}";
        }

        public string NewName(IWeaponGetter record)
        {
            return $"{Tier.Prefix} {record.Name} {Tier.Suffix}".Trim();
        }
    }

    public readonly struct WeaponTierData
    {
        public readonly ushort IronSteel;
        public readonly ushort OrcishDwarven;
        public readonly ushort ElvenGlass;
        public readonly ushort EbonyDaedric;
        public readonly ushort Dragon;

        public WeaponTierData(ushort ironSteel, ushort orcishDwarven, ushort elvenGlass, ushort ebonyDaedric,
            ushort dragon)
        {
            IronSteel = ironSteel;
            OrcishDwarven = orcishDwarven;
            ElvenGlass = elvenGlass;
            EbonyDaedric = ebonyDaedric;
            Dragon = dragon;
        }
    }

    public readonly struct EnchantmentWeaponTier
    {
        public readonly int BaseTier;
        public readonly IReadOnlyList<int> Tiers;

        public EnchantmentWeaponTier(int baseTier, IReadOnlyList<int> tiers)
        {
            BaseTier = baseTier;
            Tiers = tiers;
        }
    }

    public readonly struct WeaponDamageLevel
    {
        public readonly int Level;
        public readonly int Damage;

        public WeaponDamageLevel(int level, int damage)
        {
            Level = level;
            Damage = damage;
        }
    }
}
