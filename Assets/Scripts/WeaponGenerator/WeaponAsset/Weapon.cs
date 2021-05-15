using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace WeaponGenerator.WeaponAsset
{
    [Serializable]
    public class Weapon
    {
#pragma warning disable 0649
        [SerializeField] private List<WeaponPiece> _parts = new List<WeaponPiece>();
    
        public List<WeaponPiece> Parts
        {
            get => _parts;
            set => _parts = value;
        }
#pragma warning restore 0649

        public void UpdateNames()
        {
            if (_parts == null || _parts.Count == 0 || _parts[0].VariantPieces == null || _parts[0].VariantPieces.Count == 0)
                throw new WarningException("Please attach weapon main body to the first weapon part");

            WeaponMainBody mainBody = _parts[0].VariantPieces[0].GetComponent<WeaponMainBody>();

            if (!mainBody) throw new WarningException("Weapon main body not attached");

            for (int i = 1; i < _parts.Count; i++)
            {
                _parts[i].VariantName = mainBody.AttachmentNames[i - 1];
            }
        }
    }
}