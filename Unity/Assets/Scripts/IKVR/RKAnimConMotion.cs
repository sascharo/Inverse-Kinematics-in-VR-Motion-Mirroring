using System;
using UnityEngine;

namespace IKVR
{
    [Serializable]
    public class Motion
    {
        [Serializable]
        public class XRRig
        {
            [Range(0f, 2.5f)] public float actorHeight;
            private Transform _root;
            [Header("XR")]
            public Transform main;
            [Header("AR")]
            public Transform headConstraint;
            private Quaternion _headConstraintRotDelta;
            [Header("POSITION")]
            [Range(0f, 15f)] public float posSmoothTimeScalar = 1f;
            [HideInInspector] public Vector3 posSmoothDamp;
            private Vector3 _posDelta;
            [Header("FORWARD | ORIENTATION")]
            [Range(0f, 30f)] public float forwardSmoothTimeScalar = 1f / 0.3f;
            [HideInInspector] public Vector3 forwardSmoothDamp;
            private Vector3 _directionalVelocityCurrent;
            //[Header("PARAMETERS …")]
            [Header("DIRECTIONAL Z")]
            [Range(0f, 10f)] public float directionalVelZSmoothDampScalar = 1f;
            [Range(0f, 15f)] public float directionalVelZSmoothTimeScalar = 1f / 30f;
            [HideInInspector] public float directionalVelZSmoothDamp;
            [Range(0f, 1f)] public float directionalVelZLerpInterpolator = 2f / 3f;
            [HideInInspector] public float directionalVelZLerp;
            [Header("DIRECTIONAL X")]
            [Range(0f, 10f)] public float directionalVelXSmoothDampScalar = 1f;
            [Range(0f, 15f)] public float directionalVelXSmoothTimeScalar = 1f / 30f;
            [HideInInspector] public float directionalVelXSmoothDamp;
            [Range(0f, 1f)] public float directionalVelXLerpInterpolator = 2f / 3f;
            [HideInInspector] public float directionalVelXLerp;
            [Header("ANGULAR")]
            [Range(0f, 10f)] public float angularVelScalar = 1f;
            [Range(0f, 1f)] public float angularVelLerpInterpolator = 2f / 30f;
            [HideInInspector] public float angularVelLerp;
            [HideInInspector] public float angularVel;
            private Vector3 _forwardSmoothDampLastEval;

            protected internal XRRig(float height = default)
            {
                actorHeight = height;
            }
            
            internal void Initialize(Transform root)
            {
                _root = root;
                
                var pos = main.position;
                _posDelta = _root.position - pos;
                posSmoothDamp = pos + _posDelta;
                
                forwardSmoothDamp = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;

                _headConstraintRotDelta = headConstraint.rotation;
            }

            internal void Compute()
            {
                var pos = main.position + _posDelta + new Vector3(0f, - actorHeight, 0f);
                posSmoothDamp = Vector3.SmoothDamp(posSmoothDamp, pos, ref _directionalVelocityCurrent, Time.deltaTime * posSmoothTimeScalar);

                var forward = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;
                Vector3 angularVelocityCurrent = default;
                forwardSmoothDamp = Vector3.SmoothDamp(forwardSmoothDamp, forward, ref angularVelocityCurrent, Time.deltaTime * forwardSmoothTimeScalar).normalized;

                ComputeParameters();
            }

            internal void UpdateConstraints()
            {
                headConstraint.rotation = main.rotation * _headConstraintRotDelta;
            }

            internal void ComputeParameters()
            {
                // Directional
                var directionalVelTransfDir = _root.InverseTransformDirection(_directionalVelocityCurrent);
                
                var directionalVelTransfDirZScaled = directionalVelTransfDir.z * directionalVelZSmoothDampScalar;
                float directionalVelZSmoothDampVel = default;
                directionalVelZSmoothDamp = Mathf.SmoothDamp(directionalVelZSmoothDamp, directionalVelTransfDirZScaled, ref directionalVelZSmoothDampVel, directionalVelZSmoothTimeScalar);
                directionalVelZLerp = Mathf.Lerp(directionalVelZLerp, directionalVelZSmoothDamp, directionalVelZLerpInterpolator);
                    
                var directionalVelTransfDirXScaled = directionalVelTransfDir.x * directionalVelXSmoothDampScalar;
                float directionalVelXSmoothDampVel = default;
                directionalVelXSmoothDamp = Mathf.SmoothDamp(directionalVelXSmoothDamp, directionalVelTransfDirXScaled, ref directionalVelXSmoothDampVel, directionalVelXSmoothTimeScalar);
                directionalVelXLerp = Mathf.Lerp(directionalVelXLerp, directionalVelXSmoothDamp, directionalVelXLerpInterpolator);
                
                // Angular
                var deltaRad = Vector3.SignedAngle(forwardSmoothDamp, _forwardSmoothDampLastEval, Vector3.up) * Mathf.Deg2Rad;
                angularVel = (deltaRad * angularVelScalar) / Time.deltaTime;
                angularVelLerp = Mathf.Lerp(angularVelLerp, angularVel, angularVelLerpInterpolator);

                _forwardSmoothDampLastEval = forwardSmoothDamp;
            }
        }
        
        public XRRig rig;

        public Motion(float actorHeight = default)
        {
            rig = new XRRig(actorHeight);
        }

        internal void OnAwakeInitialize(Transform root)
        {
            rig.Initialize(root);
        }

        internal void OnUpdate()
        {
            rig.Compute();
        }
        
        internal void OnUpdateConstraints()
        {
            rig.UpdateConstraints();
        }
    }
}
