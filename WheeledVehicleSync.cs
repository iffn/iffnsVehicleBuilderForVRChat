
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
        linkedVehicle.VehicleIsOwned = Networking.IsOwner(gameObject);
    }

    float heading = 0;
    float previousHeading = 0;

    public float GetCaluclatedTurnRateIfSynced
    {
        get
        {
            return (heading - previousHeading) / Time.deltaTime;
        }
    }

    void calculateHeading()
    {
        heading = Mathf.Atan2(transform.forward.z, transform.forward.x);
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

            previousHeading = heading;
            calculateHeading();
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        linkedVehicle.VehicleIsOwned = player.isLocal;

        linkedVehicle.LinkedVehicleBuilder.LinkedUI.SetVehicleOwnerDisplay(player);

        if (!player.isLocal)
        {
            //Reset heading values
            calculateHeading();
            previousHeading = heading;
        }
    }
}
