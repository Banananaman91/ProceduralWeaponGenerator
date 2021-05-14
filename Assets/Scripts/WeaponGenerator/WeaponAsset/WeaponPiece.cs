using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponGenerator.WeaponAsset
{
    [Serializable]
    public class WeaponPiece
    {
    #pragma warning disable 0649
        [SerializeField] private String _variantName;
        [SerializeField] private List<GameObject> _variantPieces;
        [SerializeField] private bool _detachable;
    #pragma warning restore 0649
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

        public bool Detachable => _detachable;
    }
}