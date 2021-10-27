// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Core;
using UnityEngine;

namespace Team_Capture.Player.Movement
{
    internal class PlayerGfxMover : MonoBehaviour
    {
        [SerializeField] private float smoothRate = 20f;

        private Vector3 position;
        private Quaternion rotation;

        private bool subscribed;

        private void Update()
        {
            Smooth();
        }

        private void OnEnable()
        {
            SubscribeToFixedUpdateManager(true);
        }

        private void OnDisable()
        {
            SubscribeToFixedUpdateManager(false);
        }

        private void Smooth()
        {
            Transform localTransform = transform;
            Vector3 localPosition = localTransform.localPosition;

            float distance = Mathf.Max(0.01f, Vector3.Distance(localPosition, Vector3.zero));
            localTransform.localPosition =
                Vector3.MoveTowards(localPosition, Vector3.zero, distance * smoothRate * Time.deltaTime);

            distance = Mathf.Max(1f, Quaternion.Angle(transform.localRotation, Quaternion.identity));
            localTransform.localRotation = Quaternion.RotateTowards(localTransform.localRotation, Quaternion.identity,
                distance * smoothRate * Time.deltaTime);
        }

        private void SubscribeToFixedUpdateManager(bool subscribe)
        {
            if (subscribe == subscribed)
                return;

            if (subscribe)
            {
                FixedUpdateManager.OnPreFixedUpdate += OnPreFixedUpdate;
                FixedUpdateManager.OnPostFixedUpdate += OnPostFixedUpdate;
            }
            else
            {
                FixedUpdateManager.OnPreFixedUpdate -= OnPreFixedUpdate;
                FixedUpdateManager.OnPostFixedUpdate -= OnPostFixedUpdate;
            }

            subscribed = subscribe;
        }

        private void OnPostFixedUpdate()
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        private void OnPreFixedUpdate()
        {
            position = transform.position;
            rotation = transform.rotation;
        }
    }
}