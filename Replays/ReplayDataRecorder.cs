using System;
using System.Text;
using Animations;
using Managers;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utilities;

namespace Replays
{
    public class ReplayDataRecorder : MonoBehaviour
    {
        [SerializeField] private ReplayDataPlayer replayDataPlayer;
    
        private IAnimatorStateProvider _animatorStateProvider;
        private IAnimatorDataProvider _animatorDataProvider;

        private ReplayData _replayData;
        private bool _canRecord = false;

    
        // Start is called before the first frame update
        void Start()
        {
            _animatorStateProvider = GetComponent<IAnimatorStateProvider>();
            _animatorDataProvider = GetComponent<IAnimatorDataProvider>();
            _replayData = new ReplayData();

            this.FixedUpdateAsObservable().Where(_ => _canRecord).Subscribe(_ =>
            {
                _replayData.Add(transform, _animatorDataProvider.Speed > 0, _animatorDataProvider.ChargeRatio > 0, _animatorStateProvider.State.Value);
            }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerStart)
                .Subscribe(_ =>
                {
                    _canRecord = true;
                }).AddTo(this);
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerGoal)
                .Subscribe(_ =>
                {
                    _canRecord = false;
                }).AddTo(this);
        
            MessageBroker.Default.Receive<TimeSpan>()
                .Subscribe(x =>
                {
                    Debug.Log($"_replaydata.totalTime => {x}");
                    _replayData.totalTimeMilliSeconds = x.TotalMilliseconds;
                    //goalTimeText.text = x.ToString(@"hh\:mm\:ss\.fff");
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.TimeOver)
                .Subscribe(_ =>
                {
                    _replayData.totalTimeMilliSeconds = TimeSpan.FromMinutes(3).TotalMilliseconds;
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.SaveReplayData);
                }).AddTo(this);
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.SaveReplayData)
                .Take(1)
                .Subscribe(_ =>
                {
                    if (replayDataPlayer)
                    {
                        if (replayDataPlayer.UseDeveloperReplay)
                        {
                            // 開発者と対戦モードのときは違うリプレイデータを読み込んでいるためリプレイデータをロードし直す
                            if (ES3.FileExists("replayDataCompress.raw"))
                            {
                                var bytes = ES3.LoadRawBytes("replayDataCompress.raw");
                                var j = Encoding.UTF8.GetString(DataOperation.Decompress(bytes));
                                var r = JsonUtility.FromJson<ReplayData>(j);
                                // 現在のタイムのほうが過去のタイムよりも遅いのでリプレイを更新しない
                                if (_replayData.totalTimeMilliSeconds > r.totalTimeMilliSeconds) return;
                            }
                        }
                        else
                        {
                            // 現在のタイムのほうが過去のタイムよりも遅いのでリプレイを更新しない
                            if (_replayData.totalTimeMilliSeconds > replayDataPlayer.TotalTimeMilliSeconds) return;
                        }
                    }

                    //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                    //var compress = MessagePackSerializer.Serialize(_replayData, lz4Options);
                    var json = JsonUtility.ToJson(_replayData);
                    var compress = DataOperation.CompressFromStr(json);
                    Debug.Log($"Replay Data Save File Size:{compress.Length} byte.");
                    if (ES3.FileExists("replayDataCompress.raw"))
                    {
                        ES3.DeleteFile("replayDataCompress.raw");
                    }
                    ES3.SaveRaw(compress, "replayDataCompress.raw");
                
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.ReplayDataSaved);
                }).AddTo(this);
        }

    }
}