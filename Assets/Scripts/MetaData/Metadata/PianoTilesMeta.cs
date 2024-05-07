using System.Collections.Generic;
using UnityEngine;

namespace MetaData
{
    [System.Serializable]
    public class PianoTilesMeta : MonoBehaviour
    {
        public string name;
        public List<string> soundIds = new List<string>();
    }
}
