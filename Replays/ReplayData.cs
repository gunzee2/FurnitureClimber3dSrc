using System;
using System.Collections.Generic;
using Animations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Replays
{
    [Serializable]
    public class ReplayData
    {
        public List<MotionFrameData> motionFrameDataList;

        [FormerlySerializedAs("totalTimeSecond")] public double totalTimeMilliSeconds;


        public ReplayData()
        {
            motionFrameDataList = new List<MotionFrameData>();
        }
    
        public void Add(Transform transform, bool isWalking, bool isCharging, AnimatorState animatorState)
        {
            motionFrameDataList.Add(
                new MotionFrameData
                {
                    position = transform.position, 
                    rotation = transform.rotation, 
                    isWalking = isWalking,
                    isCharging = isCharging,
                    animatorState = animatorState
                });
        }
    }
}