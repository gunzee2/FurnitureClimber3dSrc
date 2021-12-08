using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace StageObjects
{
    /// <summary>
    /// プレイヤーがトリガーに入ったら回り始めるGameObject(足場ではない)
    /// 洗濯機の中で回る洗濯物で使用している
    /// </summary>
    public class TriggerRotateObject : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeedX = 0.0f;
        [SerializeField] private float _rotationSpeedY = 30.0f;
        [SerializeField] private float _rotationSpeedZ = 0.0f;

        private bool isTriggerd = false;
    
        // Start is called before the first frame update
        void Start()
        {
            this.OnTriggerEnterAsObservable().Where(x => x.CompareTag("Player")).Where(_ => !isTriggerd).Subscribe(_ =>
            {
                isTriggerd = true;
            }).AddTo(this);
            this.UpdateAsObservable().Where(_ => isTriggerd).Subscribe(_ =>
            {
                transform.Rotate(_rotationSpeedX * Time.deltaTime, _rotationSpeedY * Time.deltaTime, _rotationSpeedZ * Time.deltaTime);
            }).AddTo(this);
        }

    }
}
