using System.Threading;
using Cysharp.Threading.Tasks;

namespace Team_Capture.Pooling
{
    public class PoolReturnTimed : PoolReturn
    {
        public int timeTillReturn = 5;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private GameObjectPool pool;
        
        private void OnEnable()
        {
            TimeTask().Forget();
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
        }

        private async UniTask TimeTask()
        {
            await UniTask.Delay(timeTillReturn * 1000, cancellationToken: cancellationTokenSource.Token);
            
            if(cancellationTokenSource.IsCancellationRequested || pool == null)
                return;
            
            pool.ReturnPooledObject(gameObject);
        }

        internal override void Setup(GameObjectPool setupPool)
        {
            pool = setupPool;
        }
    }
}
