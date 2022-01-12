using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace IKVR
{
    [Serializable]
    public class IKHand
    {
        public Transform effector;
        private Vector3 _posDelta;
        private Quaternion _rotDelta;
        public Vector3 jointOrientOffset;
        [Range(0f, 7.5f)] public float posSmoothTimeScalar = 1f;
        [HideInInspector] public Vector3 posSmoothDamp;
        [Range(0f, 1f)] public float rotSlerpInterpolator = 0.9f;
        [HideInInspector] public Quaternion rotSlerp;
        private ActionBasedController _controller;

        public IKHand(Vector3 orientOffset = default)
        {
            jointOrientOffset = orientOffset;
        }
        
        internal void OnAwakeInitialize(Transform handTrans)
        {
            var pos = effector.position;
            
            _posDelta = handTrans.position - pos;
            _rotDelta = handTrans.rotation * Quaternion.Inverse(effector.rotation);

            posSmoothDamp = pos + _posDelta;
        }

        internal void OnStart()
        {
            _controller = effector.GetComponent<ActionBasedController>();
        }

        internal float OnUpdateGrip()
        {
            return _controller.selectActionValue.action.ReadValue<float>();
        }
        
        internal float OnUpdateTrigger()
        {
            return _controller.activateActionValue.action.ReadValue<float>();
        }

        internal Vector3 PosWithDelta()
        {
            Vector3 posVelocityCurrent = default;
            posSmoothDamp = Vector3.SmoothDamp(posSmoothDamp, effector.position, ref posVelocityCurrent, Time.deltaTime * posSmoothTimeScalar);
            return posSmoothDamp;
        }
        
        internal Quaternion RotWithDelta()
        {
            rotSlerp = Quaternion.Slerp(rotSlerp, effector.rotation * _rotDelta * Quaternion.Euler(jointOrientOffset), rotSlerpInterpolator);
            return rotSlerp;
        }
    }
}
