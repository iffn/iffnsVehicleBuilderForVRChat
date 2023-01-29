using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    [RequireComponent(typeof(VRCObjectSync))]
    public class WheeledVehicleSync : UdonSharpBehaviour
    {
        
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

        bool locallyOwned = false;
        float heading = 0;
        float previousHeading = 0;

        public bool LocallyOwned
        {
            get
            {
                return locallyOwned;

                //return Networking.IsOwner(gameObject);
            }
        }

        public void Setup(WheeledVehicleController linkedVehicle)
        {
            enabled = true;
            this.linkedVehicle = linkedVehicle;

            locallyOwned = Networking.LocalPlayer.IsOwner(gameObject);
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

        void CalculateHeading()
        {
            heading = Mathf.Atan2(transform.forward.z, transform.forward.x);
        }

        private void Update()
        {
            //Update controlled by vehicle controller

            if(Input.GetKeyDown(KeyCode.Home))
            {
                Debug.Log($"Sync owner = {Networking.GetOwner(gameObject).playerId}, {nameof(locallyOwned)} value = {locallyOwned}");
            }
        }

        public override void OnDeserialization()
        {
            //updateSyncFromArray();
        }

        public void MakeLocalPlayerOwner()
        {
            if (Networking.IsOwner(gameObject)) return;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject) && !locallyOwned)
            {
                Debug.Log("If you see this message, VRChat has not fixed OnOwnershipTransferred on owner leave yet ");
                OnOwnershipTransferred(Networking.LocalPlayer);
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            locallyOwned = player.isLocal;

            //Inform vehicle controller
            linkedVehicle.UpdateParametersBasedOnOwnership();

            //Inform UI
            linkedVehicle.LinkedUI.SetVehicleOwnerDisplay(player);

            //Ensure ownership of builder
            linkedVehicle.LinkedVehicleBuilder.MakeLocalPlayerOwner();

            if (!player.isLocal)
            {
                //Reset heading values
                CalculateHeading();
                previousHeading = heading;
            }
        }
    }
}