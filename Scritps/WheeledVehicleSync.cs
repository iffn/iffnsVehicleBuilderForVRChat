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

        
        ///*[UdonSynced(UdonSyncMode.Smooth)]*/ public float[] verticalWheelPositions = new float[WheeledVehicleBuilder.maxWheels];

        /*
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition0;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition1;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition2;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition3;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition4;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition5;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition6;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition7;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition8;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition9;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition10;
        [UdonSynced(UdonSyncMode.Smooth)] float wheelPosition11;

        void updateSyncFromArray()
        {
            wheelPosition0 = verticalWheelPositions[0];
            wheelPosition1 = verticalWheelPositions[1];
            wheelPosition2 = verticalWheelPositions[2];
            wheelPosition3 = verticalWheelPositions[3];
            wheelPosition4 = verticalWheelPositions[4];
            wheelPosition5 = verticalWheelPositions[5];
            wheelPosition6 = verticalWheelPositions[6];
            wheelPosition7 = verticalWheelPositions[7];
            wheelPosition8 = verticalWheelPositions[8];
            wheelPosition9 = verticalWheelPositions[9];
            wheelPosition10 = verticalWheelPositions[10];
            wheelPosition11 = verticalWheelPositions[11];
        }

        public void updateArrayFromSync()
        {
            verticalWheelPositions[0] = wheelPosition0;
            verticalWheelPositions[1] = wheelPosition1;
            verticalWheelPositions[2] = wheelPosition2;
            verticalWheelPositions[3] = wheelPosition3;
            verticalWheelPositions[4] = wheelPosition4;
            verticalWheelPositions[5] = wheelPosition5;
            verticalWheelPositions[6] = wheelPosition6;
            verticalWheelPositions[7] = wheelPosition7;
            verticalWheelPositions[8] = wheelPosition8;
            verticalWheelPositions[9] = wheelPosition9;
            verticalWheelPositions[10] = wheelPosition10;
            verticalWheelPositions[11] = wheelPosition11;
        }
        */


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
            Debug.Log("Syncing position from me: " + linkedVehicleTransform.position);

            Position = linkedVehicleTransform.position;
            Rotation = linkedVehicleTransform.rotation;

            //verticalWheelPositions = linkedVehicle.GetWheelColliderHeight();
        }

        public void SyncLocationPositionToMe()
        {
            Debug.Log("Syncing position to me: " + Position);
            linkedVehicleTransform.SetPositionAndRotation(Position, Rotation);
        }

        private void Update()
        {
            //Update controlled by vehicle controller
        }

        public override void OnDeserialization()
        {
            //updateSyncFromArray();
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