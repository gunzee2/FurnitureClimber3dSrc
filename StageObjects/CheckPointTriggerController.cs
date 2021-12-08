using DarkTonic.MasterAudio;
using Players;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace StageObjects
{
    public class CheckPointTriggerController : MonoBehaviour
    {
        [SerializeField] private GameObject effectParticleSystemGO;


        // Start is called before the first frame update
        void Start()
        {
            this.OnTriggerEnterAsObservable().Where(x => x.GetComponent<PlayerCharacter>() != null).Take(1).Subscribe(_ =>
            {
                Instantiate(effectParticleSystemGO, transform.position, Quaternion.identity);
                MessageBroker.Default.Publish(new CheckPointData{Position = transform.position, Rotation = transform.rotation});
                Destroy(gameObject);

                MasterAudio.PlaySound("getBanana");

            }).AddTo(this);
        }

    }
}
