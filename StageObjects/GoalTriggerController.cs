using DarkTonic.MasterAudio;
using Managers;
using Players;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace StageObjects
{
    public class GoalTriggerController : MonoBehaviour
    {
        [SerializeField] private GameObject effectParticleSystemGO;

        // Start is called before the first frame update
        void Start()
        {
            this.OnTriggerEnterAsObservable().Where(x => x.GetComponent<PlayerCharacter>() != null).Take(1).Subscribe(x =>
            {
                Instantiate(effectParticleSystemGO, x.transform.position, Quaternion.identity);
                MessageBroker.Default.Publish(GameManager.GlobalEvent.PlayerGoal);
                Destroy(gameObject);
            
                MasterAudio.PlaySound("getCrown");

            }).AddTo(this);
        }
    }
}
