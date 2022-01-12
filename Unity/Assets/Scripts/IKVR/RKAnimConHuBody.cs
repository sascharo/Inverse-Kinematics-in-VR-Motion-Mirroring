using System;
using UnityEngine;

namespace IKVR
{
    [Serializable]
    public class IKBody
    {
        public Transform bodyIKRoot;
        private Vector3 _hipsPos;
        private bool _initialized;
        [Range(0f, 15f)] public float rootPosSmoothTimeScalar = 1f;
        private Vector3 _headConstraintLocalEulerOffset;
        [Range(0f, 10f)] public float bodyIKForwardFScalar = 5f;
        [Range(0f, 10f)] public float bodyIKForwardBScalar = 0.5f;
        [Range(0f, 10f)] public float bodyIKLeftScalar = 1f;
        private float _bodyIKForwardSmoothDamp;
        private float _bodyIKLeftSmoothDamp;
        [Range(0f, 15f)] public float bodyIKForwardSmoothTimeScalar = 5f;
        [Range(0f, 15f)] public float bodyIKLeftSmoothTimeScalar = 5f;
        [Header("WEIGHTS")]
        [Range(0f, 1f)] public float weightIK = 0.95f;
        [Range(0f, 1f)] public float weightBody = 0.925f;
        [Range(0f, 1f)] public float weightHead = 0.6f;
        [Range(0f, 1f)] public float weightEyes = 0.1f;
        [Range(0f, 1f)] public float weightClamp;
        [Header("DEBUG | VISUALISATION")]
        public bool rootRender;
        private Renderer _rootRenderer;
        public bool boneRender;
        private UnityEngine.Animations.Rigging.BoneRenderer _boneRenderer;
        [HideInInspector] public Transform effector;
        private Renderer _effectorRenderer;
        public bool effectorRender;

        public IKBody(bool rootRe = true, bool boneRe = true, bool effectorRe = true)
        {
            rootRender = rootRe;
            boneRender = boneRe;
            effectorRender = effectorRe;
        }
        
        internal void OnAwakeInitialize()
        {
            effector = bodyIKRoot.GetChild(0).GetChild(0);

            _rootRenderer = bodyIKRoot.GetComponent<Renderer>();
            _boneRenderer = bodyIKRoot.GetComponent<UnityEngine.Animations.Rigging.BoneRenderer>();
            _effectorRenderer = effector.GetComponent<Renderer>();
            _rootRenderer.enabled = rootRender;
            _boneRenderer.drawBones = boneRender;
            _effectorRenderer.enabled = effectorRender;
        }

        internal void OnUpdate(Transform headConstraint, Transform hipsTrans)
        {
            if (!_initialized)
            {
                _hipsPos = hipsTrans.position;
                _headConstraintLocalEulerOffset = headConstraint.localRotation.eulerAngles;
                _initialized = true;
            }
            
            Vector3 hipsVelocityCurrent = default;
            _hipsPos = Vector3.SmoothDamp(_hipsPos, hipsTrans.position, ref hipsVelocityCurrent, Time.deltaTime * rootPosSmoothTimeScalar);
            bodyIKRoot.position = _hipsPos;

            float forwardVelocityCurrent = default, leftVelocityCurrent = default;
            var headConstraintLocalEulerAngles = headConstraint.localEulerAngles;

            var bodyIKForward = -(headConstraintLocalEulerAngles.z - _headConstraintLocalEulerOffset.z);
            bodyIKForward *= bodyIKForward > 0f ? bodyIKForwardFScalar : bodyIKForwardBScalar;
            _bodyIKForwardSmoothDamp = Mathf.SmoothDampAngle(_bodyIKForwardSmoothDamp, bodyIKForward, ref forwardVelocityCurrent, Time.deltaTime * bodyIKForwardSmoothTimeScalar);
            
            var bodyIKLeft = headConstraintLocalEulerAngles.x * bodyIKLeftScalar;
            _bodyIKLeftSmoothDamp = Mathf.SmoothDampAngle(_bodyIKLeftSmoothDamp, bodyIKLeft, ref leftVelocityCurrent, Time.deltaTime * bodyIKLeftSmoothTimeScalar);
            
            var bodyIKRot = Quaternion.Euler(_bodyIKForwardSmoothDamp, 0f, _bodyIKLeftSmoothDamp);
            bodyIKRoot.localRotation = bodyIKRot;
        }
    }
}
