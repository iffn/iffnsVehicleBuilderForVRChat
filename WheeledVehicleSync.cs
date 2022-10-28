
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class WheeledVehicleSync : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.Smooth)] Vector3 Position;
    [UdonSynced (UdonSyncMode.Smooth)] Quaternion Rotation;

    WheeledVehicleController linkedVehicle;
    Transform linkedVehicleTransform;

    public void Setup(WheeledVehicleController linkedVehicle)
    {
        enabled = true;
        this.linkedVehicle = linkedVehicle;
        linkedVehicleTransform = linkedVehicle.transform;
    }

    private void Update()
    {
        if (Networking.IsOwner(gameObject))
        {
            Position = linkedVehicleTransform.position;
            Rotation = linkedVehicleTransform.localRotation;
        }
        else
        {
            linkedVehicleTransform.SetPositionAndRotation(Position, Rotation);
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        linkedVehicle.VehicleIsOwned = player.isLocal;

        linkedVehicle.LinkedVehicleBuilder.LinkedUI.SetVehicleOwnerDisplay(player);
    }
}
