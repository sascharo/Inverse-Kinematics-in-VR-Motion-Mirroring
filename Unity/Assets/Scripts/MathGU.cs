using UnityEngine;

public class MathGen
{
    public static float Map(float val, float aIn1, float aIn2, float aOut1 = 0f, float aOut2 = 1f)
    {
        var t = (val - aIn1) / (aIn2 - aIn1);
        return aOut1 + (aOut2 - aOut1) * t;
    }
    
    public static float MapClamp(float val, float aIn1, float aIn2, float aOut1 = 0f, float aOut2 = 1f)
    {
        var t = (val - aIn1) / (aIn2 - aIn1);
        t = Mathf.Clamp(t, 0f, 1f);
        return aOut1 + (aOut2 - aOut1) * t;
    }
}
