using Managers;
using Players;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace StageObjects
{
    public class RestartTrigger : MonoBehaviour
    {
    
        // Start is called before the first frame update
        void Start()
        {
            this.OnTriggerEnterAsObservable().Where(x => x.GetComponent<PlayerCharacter>() != null).Subscribe(_ =>
            {
                MessageBroker.Default.Publish(GameManager.GlobalEvent.MoveToCheckPoint);
            }).AddTo(this);
        }

    }
}
