
using iffnsStuff.iffnsVRCStuff.WheeledVehicles;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DummyPickup : CockpitPickup
{
    bool debugActive = false;

    void DebugFunction()
    {
        if (Input.GetKey(KeyCode.Home) || Input.GetAxis("Oculus_GearVR_DpadX") > 0.8f)
        {
            if (debugActive) return;

            //                                      V---Manually adjust this for each class ffs
            Debug.Log($"Class {nameof(VRSteeringWheel)} of GameObject {gameObject.name} worked at {Time.time}");

            Debug.Log($"Steering wheel is held = {IsHeld}");
            Debug.Log($"Current hand = {currentPickupHand}");
            Debug.Log($"Last left hand position = {lastLeftPositionDebug}");
            Debug.Log($"Last right hand position = {lastRightPositionDebug}");
            Debug.Log($"");

            debugActive = true;
        }
        else
        {
            debugActive = false;
        }
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        DebugFunction();
    }
}
