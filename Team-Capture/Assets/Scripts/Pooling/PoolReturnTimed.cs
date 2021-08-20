using Cysharp.Threading.Tasks;

namespace Team_Capture.Pooling
{
    public class PoolReturnTimed : PoolReturn
    {
        public int timeTillReturn = 5;
        
        private GameObjectPool pool;
        
        private void OnEnable()
        {
            TimeTask().Forget();
        }

        private async UniTask TimeTask()
        {
            await Integrations.UniTask.UniTask.Delay(timeTillReturn * 1000);
            pool.ReturnPooledObject(gameObject);
        }

        internal override void Setup(GameObjectPool setupPool)
        {
            pool = setupPool;
        }
    }
}
