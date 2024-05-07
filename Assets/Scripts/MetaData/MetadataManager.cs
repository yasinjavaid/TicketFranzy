using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MetaData
{
    public class MetadataManager : MonoBehaviour
    {
        PianoTilesMeta auldlang;
        public void Init()
        {
            TextAsset auldlangTextAsset = Resources.Load<TextAsset>("JSONS/auldlang");
            auldlang    = JsonConvert.DeserializeObject<PianoTilesMeta>(auldlangTextAsset.text);
        }

        public List<string> GetAuldlandSong()
        {
            return auldlang.soundIds;
        }
    }
}
