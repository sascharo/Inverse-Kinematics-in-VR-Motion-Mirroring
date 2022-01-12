using System;
using UnityEngine;

namespace IKVR
{
    [Serializable]
    public class IKLeg
    {
        [HideInInspector] [Range(0f, 1f)] public float posWeight;
        [HideInInspector] [Range(0f, 1f)] public float rotWeight;
        [HideInInspector] public Vector3 pos;
        [HideInInspector] public Vector3 posLastEval;
        [HideInInspector] public Quaternion rot;
        [HideInInspector] public Quaternion rotLastEval;
        //private float _posFootRelY = default;
        [HideInInspector] public bool lastEvalSet;
        private float _contactThreshold;
        private float _contactActivationThreshold;
        private float _contactDeactivationThreshold;
        private float _pFeetOffset;
        private bool _floorLock;
        private float _floorHeight;
        private float _floorOffsetTolerance;
        [Header("VISUALISATION")]
        public bool visToggle = true;
        public GameObject visEffector;
        private Material _visEffectorMat;
        private Color _visEffectorColor;
        private Color _visEffectorColorGreyed;

        internal void UpdateTR(Vector3 ikPos, Quaternion ikRot)
        {
            pos = ikPos;
            rot = ikRot;
        }
        
        /*internal void UpdateTR(Vector3 ikPos, Quaternion ikRot, float footPosRelY)
        {
            pos = ikPos;
            rot = ikRot;
            _posFootRelY = footPosRelY;
        }*/
        
        internal void Compute(float pFootContact)
        {
            var posWeightFloorLock = false;
            var feetOffsetTotal = (_pFeetOffset + _floorOffsetTolerance) + _floorHeight;
            //var feetOffsetTotal = _posFootRelY;
            if (_floorLock && pos.y < feetOffsetTotal)
            {
                pos.y = feetOffsetTotal;
                posWeightFloorLock = true;
            }
            
            if (pFootContact > _contactThreshold)
            {
                if (pFootContact >= _contactActivationThreshold && !lastEvalSet)
                {
                    SetFootLastEval();
                }
                else if (pFootContact < _contactDeactivationThreshold && lastEvalSet)
                {
                    lastEvalSet = false;
                    DebugResetColor();
                }
                else if (lastEvalSet)
                {
                    SetFootFromLastEval();
                }
                
                DebugResetColor();
            }
            else
            {
                lastEvalSet = false;
                DebugDeColor();
            }

            posWeight = pFootContact;
            rotWeight = pFootContact;
            
            if (posWeightFloorLock)
            {
                posWeight = 1f;
                
                rot = rotLastEval;
                rotWeight = 1f;
            }
            
            SetEffectorPos();
        }
        
        internal void SetFootLastEval()
        {
            posLastEval = pos;
            rotLastEval = rot;
            
            lastEvalSet = true;
        }
        
        internal void SetFootFromLastEval()
        {
            pos = posLastEval;
            rot = rotLastEval;
        }
        
        internal void Setup(float contactThreshold, float contactActivationThreshold, float contactDeactivationThreshold, 
            float pFeetOffset, bool floorLock, float floorHeight,float floorOffsetTolerance, Color visEffectorColorGreyed)
        {
            _contactThreshold = contactThreshold;
            _contactActivationThreshold = contactActivationThreshold;
            _contactDeactivationThreshold = contactDeactivationThreshold;
            _pFeetOffset = pFeetOffset;
            _floorLock = floorLock;
            _floorHeight = floorHeight;
            _floorOffsetTolerance = floorOffsetTolerance;
            _visEffectorColorGreyed = visEffectorColorGreyed;
            
            DebugSetup();
        }
        
        private void DebugSetup()
        {
            _visEffectorMat = visEffector.GetComponent<Renderer>().material;
            _visEffectorColor = _visEffectorMat.color;
            _visEffectorMat.color = _visEffectorColorGreyed;
        }

        internal void DebugDeColor()
        {
            if (visToggle)
            {
                _visEffectorMat.color = _visEffectorColorGreyed;
            }
        }
        
        internal void DebugResetColor()
        {
            if (visToggle)
            {
                _visEffectorMat.color = _visEffectorColor;
            }
        }

        internal void SetEffectorPos()
        {
            visEffector.transform.position = pos;
        }
        
        /*public void SetEffectorPos(Vector3 position)
        {
            visEffector.transform.position = position;
        }*/
    }
}
