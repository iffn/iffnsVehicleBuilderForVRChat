
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WheelScalerInteractions : UdonSharpBehaviour
{
    [SerializeField] Transform WheelScaler;

    public void IncreaseWheelScale()
    {
        WheelScaler.localScale *= 1.25f;
    }

    public void DecreaseWheelScale()
    {
        WheelScaler.localScale *= 0.8f;
    }
}
