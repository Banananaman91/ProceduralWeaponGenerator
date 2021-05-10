using System.Collections.Generic;
using UnityEngine;

namespace WeaponGenerator
{
    public class WeaponMainBody : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private List<Transform> _attachmentPoints;
        public List<Transform> AttachmentPoints => _attachmentPoints;

        public List<string> AttachmentNames => GetNames();
#pragma warning restore 0649
    
        private List<string> GetNames()
        {
            var str = new List<string>();

            for (int i = 0; i < _attachmentPoints.Count; i++)
            {
                str.Add(_attachmentPoints[i].name);
            }

            return str;
        }
    }
}