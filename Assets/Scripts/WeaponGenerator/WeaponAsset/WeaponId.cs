using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeaponGenerator.WeaponAsset
{
    public class WeaponId : MonoBehaviour
    {
        [SerializeField] private List<string> _weaponIdentification = new List<string>();

        public List<string> WeaponIdentification
        {
            get => _weaponIdentification; 
            set => _weaponIdentification = value;
        }

        public int InstanceId => GetInstanceID();
    
        public int VersionId => Random.Range(0, Int32.MaxValue);
    }
}
