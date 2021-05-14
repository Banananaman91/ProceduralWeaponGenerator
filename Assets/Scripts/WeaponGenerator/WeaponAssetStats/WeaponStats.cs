using UnityEngine;

namespace WeaponGenerator.WeaponAssetStats
{
    public class WeaponStats : MonoBehaviour
    {
        [SerializeField] private Stats _stats;

        public Stats Stats
        {
            get => _stats;
            set => _stats = value;
        }
    }
}
