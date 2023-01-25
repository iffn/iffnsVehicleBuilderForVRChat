
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TrackingDataFollower : UdonSharpBehaviour
{
    [SerializeField] VRCPlayerApi.TrackingDataType trackingDataType;

    void Start()
    {
        
    }

    private void LateUpdate()
    {
        transform.SetPositionAndRotation(
            Networking.LocalPlayer.GetTrackingData(trackingDataType).position,
            Networking.LocalPlayer.GetTrackingData(trackingDataType).rotation);
    }
}
