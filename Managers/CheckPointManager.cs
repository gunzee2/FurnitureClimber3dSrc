using StageObjects;
using UniRx;
using UnityEngine;

namespace Managers
{
    public class CheckPointManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerGO;

        private CheckPointData _checkPointData;

        private void Start()
        {
            _checkPointData = new CheckPointData { Position = playerGO.transform.position, Rotation = playerGO.transform.rotation };
        
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.MoveToCheckPoint)
                .Subscribe(_ =>
                {
                    playerGO.transform.position = _checkPointData.Position;
                    playerGO.transform.rotation = _checkPointData.Rotation;
                }).AddTo(this);

        }
    }
}
