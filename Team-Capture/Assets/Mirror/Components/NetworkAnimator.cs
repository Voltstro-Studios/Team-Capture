using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror
{
    /// <summary>
    ///     A component to synchronize Mecanim animation states for networked objects.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The animation of game objects can be networked by this component. There are two models of authority for
    ///         networked movement:
    ///     </para>
    ///     <para>
    ///         If the object has authority on the client, then it should animated locally on the owning client. The
    ///         animation state information will be sent from the owning client to the server, then broadcast to all of the
    ///         other clients. This is common for player objects.
    ///     </para>
    ///     <para>
    ///         If the object has authority on the server, then it should be animated on the server and state information
    ///         will be sent to all clients. This is common for objects not related to a specific client, such as an enemy
    ///         unit.
    ///     </para>
    ///     <para>
    ///         The NetworkAnimator synchronizes the animation parameters that are checked in the inspector view. It does not
    ///         automatically sychronize triggers. The function SetTrigger can by used by an object with authority to fire an
    ///         animation trigger on other clients.
    ///     </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkAnimator")]
    [RequireComponent(typeof(NetworkIdentity))]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkAnimator.html")]
    public class NetworkAnimator : NetworkBehaviour
    {
        private int[] animationHash; // multiple layers

        /// <summary>
        ///     The animator component to synchronize.
        /// </summary>
        [FormerlySerializedAs("m_Animator")] public Animator animator;

        [Tooltip(
            "Set to true if animations come from owner client,  set to false if animations always come from server")]
        public bool clientAuthority;

        private bool[] lastBoolParameters;
        private float[] lastFloatParameters;

        // Note: not an object[] array because otherwise initialization is real annoying
        private int[] lastIntParameters;
        private AnimatorControllerParameter[] parameters;
        private float sendTimer;
        private int[] transitionHash;

        private bool sendMessagesAllowed
        {
            get
            {
                if (isServer)
                {
                    if (!clientAuthority)
                        return true;

                    // This is a special case where we have client authority but we have not assigned the client who has
                    // authority over it, no animator data will be sent over the network by the server.
                    //
                    // So we check here for a connectionToClient and if it is null we will
                    // let the server send animation data until we receive an owner.
                    if (netIdentity != null && netIdentity.connectionToClient == null)
                        return true;
                }

                return hasAuthority;
            }
        }

        private void Awake()
        {
            // store the animator parameters in a variable - the "Animator.parameters" getter allocates
            // a new parameter array every time it is accessed so we should avoid doing it in a loop
            parameters = animator.parameters;
            lastIntParameters = new int[parameters.Length];
            lastFloatParameters = new float[parameters.Length];
            lastBoolParameters = new bool[parameters.Length];

            animationHash = new int[animator.layerCount];
            transitionHash = new int[animator.layerCount];
        }

        private void FixedUpdate()
        {
            if (!sendMessagesAllowed)
                return;

            CheckSendRate();

            for (int i = 0; i < animator.layerCount; i++)
            {
                int stateHash;
                float normalizedTime;
                if (!CheckAnimStateChanged(out stateHash, out normalizedTime, i)) continue;

                NetworkWriter writer = new NetworkWriter();
                WriteParameters(writer);

                SendAnimationMessage(stateHash, normalizedTime, i, writer.ToArray());
            }
        }

        private bool CheckAnimStateChanged(out int stateHash, out float normalizedTime, int layerId)
        {
            stateHash = 0;
            normalizedTime = 0;

            if (animator.IsInTransition(layerId))
            {
                AnimatorTransitionInfo tt = animator.GetAnimatorTransitionInfo(layerId);
                if (tt.fullPathHash != transitionHash[layerId])
                {
                    // first time in this transition
                    transitionHash[layerId] = tt.fullPathHash;
                    animationHash[layerId] = 0;
                    return true;
                }

                return false;
            }

            AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(layerId);
            if (st.fullPathHash != animationHash[layerId])
            {
                // first time in this animation state
                if (animationHash[layerId] != 0)
                {
                    // came from another animation directly - from Play()
                    stateHash = st.fullPathHash;
                    normalizedTime = st.normalizedTime;
                }

                transitionHash[layerId] = 0;
                animationHash[layerId] = st.fullPathHash;
                return true;
            }

            return false;
        }

        private void CheckSendRate()
        {
            if (sendMessagesAllowed && syncInterval != 0 && sendTimer < Time.time)
            {
                sendTimer = Time.time + syncInterval;

                NetworkWriter writer = new NetworkWriter();
                if (WriteParameters(writer)) SendAnimationParametersMessage(writer.ToArray());
            }
        }

        private void SendAnimationMessage(int stateHash, float normalizedTime, int layerId, byte[] parameters)
        {
            if (isServer)
                RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, parameters);
            else if (ClientScene.readyConnection != null)
                CmdOnAnimationServerMessage(stateHash, normalizedTime, layerId, parameters);
        }

        private void SendAnimationParametersMessage(byte[] parameters)
        {
            if (isServer)
                RpcOnAnimationParametersClientMessage(parameters);
            else if (ClientScene.readyConnection != null) CmdOnAnimationParametersServerMessage(parameters);
        }

        private void HandleAnimMsg(int stateHash, float normalizedTime, int layerId, NetworkReader reader)
        {
            if (hasAuthority)
                return;

            // usually transitions will be triggered by parameters, if not, play anims directly.
            // NOTE: this plays "animations", not transitions, so any transitions will be skipped.
            // NOTE: there is no API to play a transition(?)
            if (stateHash != 0) animator.Play(stateHash, layerId, normalizedTime);

            ReadParameters(reader);
        }

        private void HandleAnimParamsMsg(NetworkReader reader)
        {
            if (hasAuthority)
                return;

            ReadParameters(reader);
        }

        private void HandleAnimTriggerMsg(int hash)
        {
            animator.SetTrigger(hash);
        }

        private ulong NextDirtyBits()
        {
            ulong dirtyBits = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter par = parameters[i];
                bool changed = false;
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    int newIntValue = animator.GetInteger(par.nameHash);
                    changed = newIntValue != lastIntParameters[i];
                    if (changed) lastIntParameters[i] = newIntValue;
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    float newFloatValue = animator.GetFloat(par.nameHash);
                    changed = Mathf.Abs(newFloatValue - lastFloatParameters[i]) > 0.001f;
                    if (changed) lastFloatParameters[i] = newFloatValue;
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    bool newBoolValue = animator.GetBool(par.nameHash);
                    changed = newBoolValue != lastBoolParameters[i];
                    if (changed) lastBoolParameters[i] = newBoolValue;
                }

                if (changed) dirtyBits |= 1ul << i;
            }

            return dirtyBits;
        }

        private bool WriteParameters(NetworkWriter writer, bool forceAll = false)
        {
            ulong dirtyBits = forceAll ? ~0ul : NextDirtyBits();
            writer.WritePackedUInt64(dirtyBits);
            for (int i = 0; i < parameters.Length; i++)
            {
                if ((dirtyBits & (1ul << i)) == 0)
                    continue;

                AnimatorControllerParameter par = parameters[i];
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    int newIntValue = animator.GetInteger(par.nameHash);
                    writer.WritePackedInt32(newIntValue);
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    float newFloatValue = animator.GetFloat(par.nameHash);
                    writer.WriteSingle(newFloatValue);
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    bool newBoolValue = animator.GetBool(par.nameHash);
                    writer.WriteBoolean(newBoolValue);
                }
            }

            return dirtyBits != 0;
        }

        private void ReadParameters(NetworkReader reader)
        {
            ulong dirtyBits = reader.ReadPackedUInt64();
            for (int i = 0; i < parameters.Length; i++)
            {
                if ((dirtyBits & (1ul << i)) == 0)
                    continue;

                AnimatorControllerParameter par = parameters[i];
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    int newIntValue = reader.ReadPackedInt32();
                    animator.SetInteger(par.nameHash, newIntValue);
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    float newFloatValue = reader.ReadSingle();
                    animator.SetFloat(par.nameHash, newFloatValue);
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    bool newBoolValue = reader.ReadBoolean();
                    animator.SetBool(par.nameHash, newBoolValue);
                }
            }
        }

        /// <summary>
        ///     Custom Serialization
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="forceAll"></param>
        /// <returns></returns>
        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                for (int i = 0; i < animator.layerCount; i++)
                    if (animator.IsInTransition(i))
                    {
                        AnimatorStateInfo st = animator.GetNextAnimatorStateInfo(i);
                        writer.WriteInt32(st.fullPathHash);
                        writer.WriteSingle(st.normalizedTime);
                    }
                    else
                    {
                        AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(i);
                        writer.WriteInt32(st.fullPathHash);
                        writer.WriteSingle(st.normalizedTime);
                    }

                WriteParameters(writer, forceAll);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Custom Deserialization
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="initialState"></param>
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                for (int i = 0; i < animator.layerCount; i++)
                {
                    int stateHash = reader.ReadInt32();
                    float normalizedTime = reader.ReadSingle();
                    animator.Play(stateHash, i, normalizedTime);
                }

                ReadParameters(reader);
            }
        }

        /// <summary>
        ///     Causes an animation trigger to be invoked for a networked object.
        ///     <para>
        ///         If local authority is set, and this is called from the client, then the trigger will be invoked on the server
        ///         and all clients. If not, then this is called on the server, and the trigger will be called on all clients.
        ///     </para>
        /// </summary>
        /// <param name="triggerName">Name of trigger.</param>
        public void SetTrigger(string triggerName)
        {
            SetTrigger(Animator.StringToHash(triggerName));
        }

        /// <summary>
        ///     Causes an animation trigger to be invoked for a networked object.
        /// </summary>
        /// <param name="hash">Hash id of trigger (from the Animator).</param>
        public void SetTrigger(int hash)
        {
            if (hasAuthority && clientAuthority)
            {
                if (ClientScene.readyConnection != null) CmdOnAnimationTriggerServerMessage(hash);
                return;
            }

            if (isServer && !clientAuthority) RpcOnAnimationTriggerClientMessage(hash);
        }

        #region server message handlers

        [Command]
        private void CmdOnAnimationServerMessage(int stateHash, float normalizedTime, int layerId, byte[] parameters)
        {
            if (LogFilter.Debug) Debug.Log("OnAnimationMessage for netId=" + netId);

            // handle and broadcast
            HandleAnimMsg(stateHash, normalizedTime, layerId, new NetworkReader(parameters));
            RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, parameters);
        }

        [Command]
        private void CmdOnAnimationParametersServerMessage(byte[] parameters)
        {
            // handle and broadcast
            HandleAnimParamsMsg(new NetworkReader(parameters));
            RpcOnAnimationParametersClientMessage(parameters);
        }

        [Command]
        private void CmdOnAnimationTriggerServerMessage(int hash)
        {
            // handle and broadcast
            HandleAnimTriggerMsg(hash);
            RpcOnAnimationTriggerClientMessage(hash);
        }

        #endregion

        #region client message handlers

        [ClientRpc]
        private void RpcOnAnimationClientMessage(int stateHash, float normalizedTime, int layerId, byte[] parameters)
        {
            HandleAnimMsg(stateHash, normalizedTime, layerId, new NetworkReader(parameters));
        }

        [ClientRpc]
        private void RpcOnAnimationParametersClientMessage(byte[] parameters)
        {
            HandleAnimParamsMsg(new NetworkReader(parameters));
        }

        // server sends this to one client
        [ClientRpc]
        private void RpcOnAnimationTriggerClientMessage(int hash)
        {
            HandleAnimTriggerMsg(hash);
        }

        #endregion
    }
}