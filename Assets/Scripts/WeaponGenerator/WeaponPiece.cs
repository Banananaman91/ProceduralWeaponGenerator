using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponGenerator
{
    [Serializable]
    public class WeaponPiece
    {
        [SerializeField] private String _variantName;
        [SerializeField] private List<GameObject> _variantPieces;

        public string VariantName
        {
            get => _variantName;
            set => _variantName = value;
        }

        public List<GameObject> VariantPieces
        {
            get => _variantPieces;
            set => _variantPieces = value;
        }
    }
}