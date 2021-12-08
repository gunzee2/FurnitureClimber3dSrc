using System;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using Managers;
using PhotoMode;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Utilities
{
    public class TweetAndCapture : MonoBehaviour
    {
        [SerializeField] string imgurClientId;

        [FormerlySerializedAs("_photoModePauser")] [SerializeField] private PhotoModeController photoModeController;
    

        private string _goalTimeString;

        private void Start()
        {
        
            MessageBroker.Default.Receive<TimeSpan>()
                .Subscribe(x =>
                {
                    _goalTimeString = StopWatchManager.TimeSpanToStopwatchFormatString(x);
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.TakeScreenShotResult)
                .Subscribe(x =>
                {
                    TweetWithScreenshotAsync(this.GetCancellationTokenOnDestroy(), $"{_goalTimeString}で走破しました！").Forget();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.TakeScreenShotPhotoMode)
                .Where(_ => photoModeController.GamePaused)
                .Subscribe(x =>
                {
                    Debug.Log("take screenshot photo mode");
                    TweetWithScreenshotAsync(this.GetCancellationTokenOnDestroy(), $"フォトモードで撮影しました！").Forget();
                }).AddTo(this);
        }


#if !UNITY_EDITOR && UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern string TweetFromUnity(string rawMessage);
#endif

        public async UniTaskVoid TweetWithScreenshotAsync(CancellationToken token, string text)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);

            var tex = ScreenCapture.CaptureScreenshotAsTexture();

            var wwwForm = new WWWForm();
            wwwForm.AddField("image", Convert.ToBase64String(tex.EncodeToJPG()));
            wwwForm.AddField("type", "base64");

            // Upload to Imgur
            var www = UnityWebRequest.Post("https://api.imgur.com/3/image.xml", wwwForm);
            www.SetRequestHeader("AUTHORIZATION", "Client-ID " + imgurClientId);

            await www.SendWebRequest().WithCancellation(token);

            var uri = "";

            if (!www.isNetworkError)
            {
                var xDoc = XDocument.Parse(www.downloadHandler.text);
                uri = xDoc.Element("data")?.Element("link")?.Value;

                if (uri != null)
                {
                    // Remove ext
                    uri = uri.Remove(uri.Length - 4, 4);
                }
            }

#if !UNITY_EDITOR && UNITY_WEBGL
        TweetFromUnity($"{text}%0a https://t.co/wVyVYzvvLW?amp=1 %0a%23家具クライマー3D %23unity1week%0a{uri}");
#endif
        }
    }
}
