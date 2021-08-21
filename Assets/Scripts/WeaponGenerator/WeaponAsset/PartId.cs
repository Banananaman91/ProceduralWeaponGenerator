using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace WeaponGenerator.WeaponAsset
{
    public class PartId : MonoBehaviour
    {
        public string HashId => GetHashString(name);
        
        private static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        
        private static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (var b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
