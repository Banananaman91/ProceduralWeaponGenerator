using UnityEngine;
using WeaponGenerator.WeaponAssetRarity.Dependencies;

namespace WeaponGenerator.WeaponAssetRarity
{
    public class WeaponRarityLevel : MonoBehaviour
    {
        [SerializeField] private WeaponRarity _rarity;

        public WeaponRarity Rarity
        {
            get => _rarity;
            set => _rarity = value;
        }
    }
}
