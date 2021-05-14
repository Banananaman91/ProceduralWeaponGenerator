using System;
using UnityEngine;

namespace WeaponGenerator.WeaponAssetStats
{
    public class WeaponStatsContribution : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private Stats _weaponStats;
        #pragma warning restore 0649
        public Stats WeaponStats => _weaponStats;
    }
}
