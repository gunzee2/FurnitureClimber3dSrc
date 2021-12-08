using System;
using Animations;
using UnityEngine;

namespace Replays
{
    [Serializable]
    public struct MotionFrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isWalking;
        public bool isCharging;
        public AnimatorState animatorState;
    
    }
}
