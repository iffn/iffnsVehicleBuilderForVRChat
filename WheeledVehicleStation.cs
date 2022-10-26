
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(VRCStation))]
public class WheeledVehicleStation : UdonSharpBehaviour
{
    [HideInInspector] public WheeledVehicleController linkedVehicle;
    VRCStation linkedVRCStaion;
    bool seated = false;

    private void Start()
    {
        linkedVRCStaion = transform.GetComponent<VRCStation>();

        #if UNITY_EDITOR
        //SendCustomEventDelayedSeconds(nameof(ForceEnter), 1);
        #endif
    }

    public void ForceEnter()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }

    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            linkedVehicle.Active = true;
            seated = true;
        }
        else
        {
            linkedVehicle.Active = false;
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            linkedVehicle.Active = false;
            seated = false;
        }
    }

    private void Update()
    {
        if (seated)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                linkedVRCStaion.ExitStation(Networking.LocalPlayer);
            }
        }
    }

    public override void InputJump(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (Networking.LocalPlayer.IsUserInVR() && seated)
        {
            linkedVRCStaion.ExitStation(Networking.LocalPlayer);
        }
    }
}
