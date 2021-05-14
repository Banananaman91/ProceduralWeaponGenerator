using UnityEngine;

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
