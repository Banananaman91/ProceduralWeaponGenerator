using UnityEngine;

namespace WeaponGenerator.WeaponAssetStats
{
    public class WeaponStats : MonoBehaviour
    {
        [SerializeField] private Stats _stats;

        public Stats Stats
        {
            set => _stats = value;
        }
    }
}
