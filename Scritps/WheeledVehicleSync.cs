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
        /*
            Tasks of this component:
            - Manage ownership
            - Ensure position and rotation sync
            - Provide derivatives for component sync
        */

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

        VRCPlayerApi localPlayer;

        bool locallyOwned = false;
        float heading = 0;
        float previousHeading = 0;
        float lastUpdate;

        int counter;

        public string DebugString()
        {
            string returnString = "";

            string newLine = "\n";

            returnString += $"Debug of {nameof(WheeledVehicleSync)}:" + newLine;
            returnString += $"• {nameof(lastUpdate)} = {lastUpdate}" + newLine;

            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            returnString += $"• Owner = {owner.playerId}: {owner.displayName} {(owner.isLocal ? "(You)" : "")}" + newLine;

            returnString += $"• {nameof(locallyOwned)} = {locallyOwned}" + newLine;
            returnString += $"• {nameof(heading)} = {heading}" + newLine;
            returnString += $"• {nameof(previousHeading)} = {previousHeading}" + newLine;

            returnString += newLine;

            return returnString;
        }

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

            localPlayer = Networking.LocalPlayer;

            locallyOwned = localPlayer.IsOwner(gameObject);
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

            lastUpdate = Time.time;

            if (Input.GetKeyDown(KeyCode.Home))
            {
                Debug.Log($"Sync owner = {Networking.GetOwner(gameObject).playerId}, {nameof(locallyOwned)} value = {locallyOwned}");
            }

            counter++;

            if(counter % 100 == 0)
            {
                counter -= 100;

                EnsureCorrectOwnership();
            }
        }

        void EnsureCorrectOwnership()
        {
            if (!Networking.IsOwner(gameObject)) return;

            if (!locallyOwned)
            {
                Debug.LogWarning("Locally owned somehow didn't work");
                locallyOwned = true;
            }

            linkedVehicle.LinkedVehicleBuilder.MakeLocalPlayerOwner();
        }

        public override void OnDeserialization()
        {
            //updateSyncFromArray();
        }

        public void MakeLocalPlayerOwner()
        {
            if (Networking.IsOwner(gameObject)) return;

            Networking.SetOwner(localPlayer, gameObject);

            linkedVehicle.LinkedVehicleBuilder.MakeLocalPlayerOwner();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (localPlayer.IsOwner(gameObject) && !locallyOwned)
            {
                Debug.Log("If you see this message, VRChat has not fixed OnOwnershipTransferred on owner leave yet ");
                OnOwnershipTransferred(localPlayer);
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            locallyOwned = player.isLocal;

            //Inform vehicle controller
            linkedVehicle.UpdateParametersBasedOnOwnership();

            //Inform UI
            linkedVehicle.LinkedUICanBeNull.SetVehicleOwnerDisplay(player);

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