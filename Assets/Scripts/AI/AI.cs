using System;
using UnityEngine;

namespace AIs
{
    interface IAIActions
    {
        public void DoInput();
    }
    [Serializable]
    public struct AIModeRanges
    {
        public float[] easyRange;
        public float[] mediumRange;
        public float[] hardRange;
    }
    public class AI : SingletonLocal<AI>
    {
        #region Enums
        public enum  AILevels
        {
            Simple = 0,
            Medium = 1,
            Hard   = 2
        }
        #endregion
        
        #region conts

        #endregion

        #region public variables

        public AILevels AILevel { get; set; }
        public bool isAIMatch { get; set; }
        
        #endregion

    }
}
