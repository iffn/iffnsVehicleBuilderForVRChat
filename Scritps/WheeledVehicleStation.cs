using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [RequireComponent(typeof(VRCStation))]
    public class WheeledVehicleStation : UdonSharpBehaviour
    {
        [HideInInspector] public WheeledVehicleController linkedVehicle;
        VRCStation linkedVRCStaion;

        public bool EnableCollider
        {
            set
            {
                transform.GetComponent<Collider>().enabled = value;
            }
        }

        VRCPlayerApi seatedPlayer;
        public VRCPlayerApi SeatedPlayer
        {
            get
            {
                return seatedPlayer;
            }
        }

        public StationOccupantTypes StationOccupant
        {
            get
            {
                if (seatedPlayer == null)
                {
                    return StationOccupantTypes.noone;
                }
                else if (seatedPlayer.isLocal)
                {
                    return StationOccupantTypes.me;
                }
                else
                {
                    return StationOccupantTypes.someoneElse;
                }
            }
        }

        public void Setup()
        {
            linkedVRCStaion = transform.GetComponent<VRCStation>();

            /*
            #if UNITY_EDITOR
            SendCustomEventDelayedSeconds(nameof(ForceEnter), 1);
            #endif
            */
        }

        private void Start()
        {
            //Run in setup started by the controller
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
            seatedPlayer = player;

            if (player.isLocal)
            {
                linkedVehicle.EnteredDriverSeat();
            }
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            seatedPlayer = null;

            linkedVehicle.ExitedDriverSeat();
        }

        private void Update()
        {
            if (seatedPlayer != null && seatedPlayer.isLocal)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    linkedVRCStaion.ExitStation(Networking.LocalPlayer);
                }
            }
        }

        public override void InputJump(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (seatedPlayer != null && Networking.LocalPlayer.IsUserInVR() && seatedPlayer.isLocal)
            {
                linkedVRCStaion.ExitStation(Networking.LocalPlayer);
            }
        }
    }

    public enum StationOccupantTypes
    {
        noone,
        me,
        someoneElse
    }
}
