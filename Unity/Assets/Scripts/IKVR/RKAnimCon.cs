using Unity.Mathematics;
using UnityEngine;

namespace IKVR
{
    [RequireComponent(typeof(Animator))]
    public class RKAnimCon : MonoBehaviour
    {
        public int targetFrameRate = -1;
        //private float _fps, _fpsSmooth, _fpsUnscaled;

        [Header("LOCOMOTION")]
        public Motion motion = new (1.725f);
        private Transform _transform;
        private Animator _animator;
        private bool _initialized;
        private float _pFps;
        private float _pIdle;
        private float _pLocomotion;
        private float _pLocomotionLastEval;
        private float _pMagnitude;
        private float _pMagnitudeSmooth;
        private float _pFeetOffset;
        private float _pFeetOffsetTolerance;
        private float _pFootContactL, _pFootContactR;
        [Range(0, 2)] public int speedMode = 2;
        [Range(0f, 5f)] public float speedScalar = 1f;

        [Header("LEGS | FEET")]
        public bool footContactLock = true;
        [Range(0f, 0.1f)] public float footContactOffsetTolerance = 0.01f;
        [Range(0f, 1f)] public float footContactThreshold;
        [Range(0f, 1f)] public float footContactActivationThreshold = 1f;
        [Range(0f, 1f)] public float footContactDeactivationThreshold = 1f;
        public bool footFloorLock;
        [Range(-10f, 10f)] public float footFloorHeight;
        [Range(0f, 0.1f)] public float footFloorOffsetTolerance = 0.005f;
        public IKLeg legIKLeft;
        public IKLeg legIKRight;
        [Range(0f, 1f)] public float footContactWeightScalarPos = 1f;
        [Range(0f, 1f)] public float footContactWeightScalarRot = 1f;
        public Color visEffectorColorGreyedOut = new(0.3f, 0.3f, 0.3f, 0.7f);

        [Header("ARMS | HANDS")]
        [Range(0f, 1f)] public float handsPosWeight = 1f;
        [Range(0f, 1f)] public float handsRotWeight = 1f;
        public IKHand handIKLeft = new (new Vector3(0f, 45f, 22.5f));
        public IKHand handIKRight = new (new Vector3(0f, 135f, -45f));
        
        [Header("BODY")]
        public IKBody bodyIK = new (true, false, false);
        
        private void ResetCP()
        {
            _animator.SetFloat(CP.Advance, 0f);
            _animator.SetFloat(CP.Turn, 0f);
            _animator.SetFloat(CP.Speed, 1f);
            _animator.SetFloat(CP.LocoSpeed, 0f);
        }

        private void Awake()
        {
            _transform = transform;

            _animator = transform.GetComponent<Animator>();

            motion.OnAwakeInitialize(_transform);
            
            bodyIK.OnAwakeInitialize();
            
            handIKLeft.OnAwakeInitialize(_animator.GetBoneTransform(HumanBodyBones.LeftHand));
            handIKRight.OnAwakeInitialize(_animator.GetBoneTransform(HumanBodyBones.RightHand));
        }
        
        private void OnEnable()
        {
            ResetCP();
        }
        
        private void Start()
        {
            if (targetFrameRate != -1)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = targetFrameRate;
                //_currentFrameTime = Time.realtimeSinceStartup;
                //StartCoroutine("WaitForNextFrame");
            }
            
            handIKLeft.OnStart();
            handIKRight.OnStart();
        }

        /*private void ComputeFPS()
        {
            _fps = 1f / Time.deltaTime;
            _fpsSmooth = 1f / Time.smoothDeltaTime;
            _fpsUnscaled = 1f / Time.unscaledDeltaTime;
        }*/

        private void OnAnimatorMoveInitialize()
        {
            _pFps = _animator.GetFloat(CP.FPS);
            _pFeetOffset = _animator.GetFloat(CP.FeetOffset);

            _initialized = true;
                
            _pFeetOffsetTolerance = _pFeetOffset + footContactOffsetTolerance;
            
            legIKLeft.Setup(footContactThreshold, footContactActivationThreshold, footContactDeactivationThreshold, 
                _pFeetOffset, footFloorLock, footFloorHeight, footFloorOffsetTolerance, visEffectorColorGreyedOut);
            legIKRight.Setup(footContactThreshold, footContactActivationThreshold, footContactDeactivationThreshold, 
                _pFeetOffset, footFloorLock, footFloorHeight, footFloorOffsetTolerance, visEffectorColorGreyedOut);
        }

        private void Update()
        {
            //ComputeFPS();

            motion.OnUpdate();
            _transform.position = motion.rig.posSmoothDamp;
            _transform.forward = motion.rig.forwardSmoothDamp;

            motion.OnUpdateConstraints();

            bodyIK.OnUpdate(motion.rig.headConstraint.transform, _animator.GetBoneTransform(HumanBodyBones.Hips));
        }

        private void LateUpdate()
        {
            _animator.SetFloat(CP.HandGripLeft, handIKLeft.OnUpdateGrip());
            _animator.SetFloat(CP.HandGripRight, handIKRight.OnUpdateGrip());
            _animator.SetFloat(CP.HandTriggerLeft, handIKLeft.OnUpdateTrigger());
            _animator.SetFloat(CP.HandTriggerRight, handIKRight.OnUpdateTrigger());
        }

        private void OnAnimatorMove()
        {
            if (!_initialized)
            {
                OnAnimatorMoveInitialize();
            }
            
            _pIdle = _animator.GetFloat(CP.Idle);
            _pMagnitude = _animator.GetFloat(CP.Magnitude);
            _pMagnitudeSmooth = _animator.GetFloat(CP.MagnitudeSmooth);

            // Directional
            _animator.SetFloat(CP.Advance, motion.rig.directionalVelZLerp);
            _animator.SetFloat(CP.Strafe, motion.rig.directionalVelXLerp);

            // Angular
            _animator.SetFloat(CP.Turn, motion.rig.angularVelLerp);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            /*if (layerIndex != 1)
            {
                return;
            }*/
            
            switch (speedMode)
            {
                case 1:
                    _animator.SetFloat(CP.Speed, _pMagnitude * speedScalar);
                    break;
                case 2:
                    _animator.SetFloat(CP.Speed, _pMagnitudeSmooth * Mathf.Abs(motion.rig.directionalVelZLerp) * speedScalar);
                    break;
                default:
                    _animator.SetFloat(CP.Speed, speedScalar);
                    break;
            }
            
            if (footContactLock)
            {
                OnLegIK();
            }

            OnHandIK();

            OnBodyIK();
        }

        private void OnLegIK()
        {
            _pFootContactL = math.clamp(_animator.GetFloat(CP.FootContactL), 0f, 1f);
            _pFootContactR = math.clamp(_animator.GetFloat(CP.FootContactR), 0f, 1f);
            _pFootContactL = _animator.GetFloat(CP.FootContactL);
            _pFootContactR = _animator.GetFloat(CP.FootContactR);
            
            legIKLeft.UpdateTR(_animator.GetIKPosition(AvatarIKGoal.LeftFoot), _animator.GetIKRotation(AvatarIKGoal.LeftFoot));
            legIKRight.UpdateTR(_animator.GetIKPosition(AvatarIKGoal.RightFoot), _animator.GetIKRotation(AvatarIKGoal.RightFoot));

            legIKLeft.Compute(_pFootContactL);
            legIKRight.Compute(_pFootContactR);
            
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, legIKLeft.posWeight * footContactWeightScalarPos);
            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, legIKLeft.pos);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, legIKLeft.rotWeight * footContactWeightScalarRot);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, legIKLeft.rot);
            
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, legIKRight.posWeight * footContactWeightScalarPos);
            _animator.SetIKPosition(AvatarIKGoal.RightFoot, legIKRight.pos);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, legIKRight.rotWeight * footContactWeightScalarRot);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, legIKRight.rot);
        }

        private void OnHandIK()
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handsPosWeight);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, handIKLeft.PosWithDelta());
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handsRotWeight);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, handIKLeft.RotWithDelta());
            
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handsPosWeight);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, handIKRight.PosWithDelta());
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handsRotWeight);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, handIKRight.RotWithDelta());
        }

        private void OnBodyIK()
        {
            _animator.SetLookAtWeight(bodyIK.weightIK, bodyIK.weightBody, bodyIK.weightHead, bodyIK.weightEyes, bodyIK.weightClamp);
            _animator.SetLookAtPosition(bodyIK.effector.position);
        }

        private void OnDisable()
        {
            ResetCP();
        }
    }
}
