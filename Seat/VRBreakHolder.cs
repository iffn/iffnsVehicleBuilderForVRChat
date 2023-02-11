using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class VRBreakHolder : CockpitPickup
    {
        float breakInput;
        Vector3 initialLocalPosition;

        public float BreakInput
        {
            get
            {
                return breakInput;
            }
        }

        public override void Setup()
        {
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                transform.parent.gameObject.SetActive(false);
                return;
            }

            initialLocalPosition = transform.localPosition;

            base.Setup();
        }

        protected override void AdditionalLateUpdateFunctions()
        {
            switch (currentPickupHand)
            {
                case VRC_Pickup.PickupHand.None:
                    break;
                case VRC_Pickup.PickupHand.Left:
                    transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    break;
                case VRC_Pickup.PickupHand.Right:
                    transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            switch (currentPickupHand)
            {
                case VRC_Pickup.PickupHand.None:
                    break;
                case VRC_Pickup.PickupHand.Left:
                    breakInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                    break;
                case VRC_Pickup.PickupHand.Right:
                    breakInput = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                    break;
                default:
                    break;
            }
        }

        protected override void DropOccured()
        {
            breakInput = 0;

            transform.localPosition = initialLocalPosition;
        }
    }
}