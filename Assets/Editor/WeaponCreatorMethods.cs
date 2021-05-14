﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaponGenerator.WeaponAsset;
using WeaponGenerator.WeaponAssetRarity;
using WeaponGenerator.WeaponAssetStats;

namespace Editor
{
    public static class WeaponCreatorMethods
    {
        public static Dictionary<int, List<string>> ListBuilder(Weapon weapon)
        {
            Dictionary<int, List<string>> tags = new Dictionary<int, List<string>>();
            //Build dictionary of items, each entry corresponds to each weapon part with the string forming each index to be used when generating combinations
            //e.g. weapon part one may contain 2 pieces, so string will = "01". When generating combinations it will first use item 0, then item 1, to generate combinations of indexes
            for (var i = 0; i < weapon.Parts.Count; i++)
            {
                var str = new List<string>();
                for (var j = 0; j < weapon.Parts[i].VariantPieces.Count; j++)
                {
                    str.Add(j.ToString());
                }

                tags.Add(i, str);
            }

            return tags;
        }

        //recursive function to generate all combinations
        public static List<string> GetCombos(IEnumerable<KeyValuePair<int, List<string>>> remainingTags)
        {
            if (remainingTags.Count() == 1)
            {
                return remainingTags.First().Value;
            }

            var current = remainingTags.First();
            var combos = GetCombos(remainingTags.Where(tag => tag.Key != current.Key));

            return (from tagPart in current.Value from combo in combos select tagPart + combo).ToList();
        }
        
        public static Stats GenerateWeaponStats(IReadOnlyList<GameObject> parts)
        {
            var weaponStats = new Stats();
            for (var i = 0; i < parts.Count; i++)
            {
                var partStat = parts[i].GetComponent<WeaponStatsContribution>().WeaponStats;

                for (var j = 0; j < partStat.StatDescriptors.Count; j++)
                {
                    if (weaponStats.StatDescriptors.Count == 0)
                    {
                        weaponStats.StatDescriptors.Add(partStat.StatDescriptors[i]);
                    }
                    else
                    {
                        var partAdded = false;
                        foreach (var t in weaponStats.StatDescriptors.Where(t => String.Equals(t.StatName, partStat.StatDescriptors[j].StatName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            t.StatValue += partStat.StatDescriptors[j].StatValue;
                            partAdded = true;
                            break;
                        }
                        if (!partAdded) weaponStats.StatDescriptors.Add(partStat.StatDescriptors[j]);
                    }
                }
            }

            return weaponStats;
        }

        public static WeaponRarity GenerateWeaponRarity(IReadOnlyList<GameObject> parts,
            RarityCalculationType rarityCalculationType)
        {
            WeaponRarity weaponRarity;
            var numbers = new int[parts.Count];

            for (var i = 0; i < parts.Count; i++)
            {
                numbers[i] = (int) parts[i].GetComponent<WeaponRarityLevel>().Rarity;
            }

            switch (rarityCalculationType)
            {
                case RarityCalculationType.MostCommon:
                    weaponRarity = (WeaponRarity) numbers.GroupBy(item => item).OrderByDescending(g => g.Count())
                        .Select(g => g.Key).First();
                    break;
                case RarityCalculationType.Middle:
                    Array.Sort(numbers.ToArray());
                    var middle = Mathf.RoundToInt(numbers.Length / 2);
                    weaponRarity = (WeaponRarity) numbers[middle];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return weaponRarity;
        }
    }
}