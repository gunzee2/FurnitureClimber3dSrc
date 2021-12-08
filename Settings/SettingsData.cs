using System;

namespace Settings
{
    [Serializable]
    public struct SettingsData
    {
        public bool isReverseX;
        public bool isReverseY;
        public bool isApplyPostEffect;

        public SettingsData(bool reverseX,bool reverseY,bool applyPostEffect)
        {
            isReverseX = reverseX;
            isReverseY = reverseY;
            isApplyPostEffect = applyPostEffect;
        }
    }
}