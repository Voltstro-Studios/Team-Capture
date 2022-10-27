using UnityEngine;

namespace Team_Capture.Pooling
{
    /// <summary>
    ///     Allows the object to be returned to a pool
    /// </summary>
    public abstract class PoolReturn : MonoBehaviour
    {
        internal abstract void Setup(GameObjectPoolBase pool);
    }
}