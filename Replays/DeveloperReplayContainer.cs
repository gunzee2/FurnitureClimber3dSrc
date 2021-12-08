using UnityEngine;

namespace Replays
{
    public class DeveloperReplayContainer : MonoBehaviour
    {
        public bool useDeveloperReplay = false;
        public byte[] DeveloperReplay { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
        }

        public void LoadDeveloperReplay()
        {
            var textAsset = Resources.Load<TextAsset>("developerReplay");
            DeveloperReplay = textAsset.bytes;

            useDeveloperReplay = true;

        }

    }
}