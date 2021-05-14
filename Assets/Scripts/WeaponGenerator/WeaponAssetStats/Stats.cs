using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponGenerator.WeaponAssetStats
{
    [Serializable]
    public class Stats
    {
        [SerializeField] private List<StatDescriptor> _statDescriptors = new List<StatDescriptor>();

        public List<StatDescriptor> StatDescriptors => _statDescriptors;
    }
}
