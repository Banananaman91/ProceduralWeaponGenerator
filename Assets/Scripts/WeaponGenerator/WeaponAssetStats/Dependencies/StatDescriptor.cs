using System;
using UnityEngine;

namespace WeaponGenerator.WeaponAssetStats.Dependencies
{
    [Serializable]
    public class StatDescriptor
    {
        #pragma warning disable 0649
        [SerializeField] private string _statName;
        [SerializeField] protected float _statValue;
        #pragma warning restore 0649

        public float StatValue
        {
            get => _statValue;
            set => _statValue = value;
        }

        public string StatName => _statName;
    }
}