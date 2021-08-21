using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeaponGenerator.WeaponAsset
{
    public class WeaponId : MonoBehaviour
    {
        //Used purely to allow visibility of part identifications in inspector for debugging
        [SerializeField] private List<string> _weaponIdentification = new List<string>();

        //Weapon Identification List from Weapon Part Identification (PartId)
        public List<string> WeaponIdentification
        {
            get => _weaponIdentification; 
            set => _weaponIdentification = value;
        }

        //Identify specific weapon parts in file explorer, used for more accurate reloading
        public int InstanceId => GetInstanceID();
    
        //Allows identification of multiple instances of the same weapon
        public int VersionId => Random.Range(0, Int32.MaxValue);
    }
}
