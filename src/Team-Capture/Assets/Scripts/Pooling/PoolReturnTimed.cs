using System.Threading;
using Cysharp.Threading.Tasks;

namespace Team_Capture.Pooling
{
    /// <summary>
    ///     A timed pool returner
    /// </summary>
    public class PoolReturnTimed : PoolReturn
    {
        /// <summary>
        ///     How longer to wait until the object is returned back to the pool
        /// </summary>
        public int timeTillReturn = 5;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private GameObjectPoolBase pool;

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

            if (cancellationTokenSource.IsCancellationRequested || pool == null)
                return;

            pool.ReturnPooledObject(gameObject);
        }

        internal override void Setup(GameObjectPoolBase setupPool)
        {
            pool = setupPool;
        }
    }
}