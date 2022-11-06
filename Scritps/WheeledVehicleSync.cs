using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class WheeledVehicleSync : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.Smooth)] Vector3 Position;
        [UdonSynced(UdonSyncMode.Smooth)] Quaternion Rotation;
        [UdonSynced(UdonSyncMode.Smooth)] public float[] verticalWheelPositions = new float[WheeledVehicleBuilder.maxWheels];

        WheeledVehicleController linkedVehicle;
        Transform linkedVehicleTransform;

        float heading = 0;
        float previousHeading = 0;

        public bool VehicleIsOwned
        {
            get
            {
                return Networking.IsOwner(gameObject);
            }
        }

        public void Setup(WheeledVehicleController linkedVehicle)
        {
            enabled = true;
            this.linkedVehicle = linkedVehicle;
            linkedVehicleTransform = this.linkedVehicle.transform;
        }


        public float GetCaluclatedTurnRateIfSynced
        {
            get
            {
                previousHeading = heading;

                heading = Mathf.Atan2(transform.forward.z, transform.forward.x);

                return (heading - previousHeading) / Time.deltaTime;

            }
        }

        void calculateHeading()
        {
            heading = Mathf.Atan2(transform.forward.z, transform.forward.x);
        }

        public void SyncLocationFromMe()
        {
            Position = linkedVehicleTransform.position;
            Rotation = linkedVehicleTransform.rotation;

            verticalWheelPositions = linkedVehicle.GetWheelColliderHeight();
        }

        public void SyncLocationPositionToMe()
        {
            linkedVehicleTransform.SetPositionAndRotation(Position, Rotation);
        }

        private void Update()
        {
            //Update controlled by vehicle controller
        }

        public void MakeLocalPlayerOwner()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Debug.LogWarning("Ownership transfered to " + player.playerId + ":" + player.displayName);

            linkedVehicle.UpdateParametersBasedOnOwnership();

            linkedVehicle.LinkedUI.SetVehicleOwnerDisplay(player);

            if (!player.isLocal)
            {
                //Reset heading values
                calculateHeading();
                previousHeading = heading;
            }
        }
    }
}