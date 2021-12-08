using Animations;
using DarkTonic.MasterAudio;
using UniRx;
using UnityEngine;

namespace Players
{
    public class SoundPlayer : MonoBehaviour
    {
        private IPlayerEventProvider _playerEventProvider;
        private IAnimatorStateProvider _animatorStateProvider;

        private const string FOOTSTEP_FILENAME = "footstep_loop2";
        private const string CHARGE_FILENAME = "charge";

        private void Start()
        {
            _playerEventProvider = GetComponent<IPlayerEventProvider>();
            _animatorStateProvider = GetComponent<IAnimatorStateProvider>();

        
            MasterAudio.StartPlaylistOnClip("bgmPlaylist", "bgm");
            _playerEventProvider.OnJumpStart.Subscribe(_ =>
            {
                MasterAudio.StopAllOfSound(CHARGE_FILENAME);
                MasterAudio.PlaySound("jump4");
            }).AddTo(this);
            _playerEventProvider.OnDown.Subscribe(_ =>
            {
                MasterAudio.PlaySound("hit2");
            }).AddTo(this);
            _playerEventProvider.OnCollisionEnterObject.Subscribe(_ =>
            {
                MasterAudio.PlaySound("bound");
            }).AddTo(this);
            _playerEventProvider.OnLanded.Subscribe(_ =>
            {
                MasterAudio.PlaySound("landed");
            }).AddTo(this);
            _playerEventProvider.OnJumpAttack.Subscribe(_ =>
            {
                MasterAudio.PlaySound("swing");
            }).AddTo(this);
            _playerEventProvider.OnJumpAttackHit.Subscribe(_ =>
            {
                MasterAudio.PlaySound("walljump");
            }).AddTo(this);
        
            _playerEventProvider.MoveDirection.Where(x => x.sqrMagnitude > 0).Where(_ => !MasterAudio.IsSoundGroupPlaying(FOOTSTEP_FILENAME)).Subscribe(_ =>
            {
                MasterAudio.PlaySound(FOOTSTEP_FILENAME);
            }).AddTo(this);
            _playerEventProvider.MoveDirection.Where(x => x.sqrMagnitude <= 0).Where(_ => MasterAudio.IsSoundGroupPlaying(FOOTSTEP_FILENAME)).Subscribe(_ =>
            {
                MasterAudio.StopAllOfSound(FOOTSTEP_FILENAME);
            }).AddTo(this);
            _animatorStateProvider.State.Where(x => x != AnimatorState.Idle).Where(_ => MasterAudio.IsSoundGroupPlaying(FOOTSTEP_FILENAME)).Subscribe(_ =>
            {
                MasterAudio.StopAllOfSound(FOOTSTEP_FILENAME);
            }).AddTo(this);
        
        

            _animatorStateProvider.State.Where(x => x == AnimatorState.Charging).Subscribe(_ =>
            {
                MasterAudio.PlaySound(CHARGE_FILENAME);
            }).AddTo(this);
            _animatorStateProvider.State
                .Pairwise()
                .Where(x => x.Previous == AnimatorState.Charging && x.Current != AnimatorState.Charging)
                .Where(_ => MasterAudio.IsSoundGroupPlaying(CHARGE_FILENAME))
                .Subscribe(_ =>
                {
                    MasterAudio.StopAllOfSound(CHARGE_FILENAME);
                }).AddTo(this);
        }
    }
}
