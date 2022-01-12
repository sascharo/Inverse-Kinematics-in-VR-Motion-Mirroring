using UnityEngine;

namespace IKVR
{
    public struct CP
    {
        public static readonly int ScaleFactor = Animator.StringToHash("scaleFactor");
        public static readonly int FPS = Animator.StringToHash("fps");
        public static readonly int Idle = Animator.StringToHash("idle");
        public static readonly int Forward = Animator.StringToHash("forward");
        public static readonly int Backward = Animator.StringToHash("backward");
        public static readonly int Locomotion = Animator.StringToHash("locomotion");
        public static readonly int Magnitude = Animator.StringToHash("magnitude");
        public static readonly int MagnitudeSmooth = Animator.StringToHash("magnitudeSmooth");
        public static readonly int MagnitudeMean = Animator.StringToHash("magnitudeMean");
        public static readonly int FeetOffset = Animator.StringToHash("feetOffset");
        public static readonly int FootContactL = Animator.StringToHash("footContactL");
        public static readonly int FootContactR = Animator.StringToHash("footContactR");

        public static readonly int Advance = Animator.StringToHash("advance");
        public static readonly int Strafe = Animator.StringToHash("strafe");
        public static readonly int Turn = Animator.StringToHash("turn");
        public static readonly int Speed = Animator.StringToHash("speed");
        public static readonly int LocoSpeed = Animator.StringToHash("locoSpeed");
        
        public static readonly int HandGripLeft = Animator.StringToHash("handGripLeft");
        public static readonly int HandGripRight = Animator.StringToHash("handGripRight");
        public static readonly int HandTriggerLeft = Animator.StringToHash("handTriggerLeft");
        public static readonly int HandTriggerRight = Animator.StringToHash("handTriggerRight");
    }
}
