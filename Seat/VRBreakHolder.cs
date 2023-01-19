using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [RequireComponent(typeof(VRCPickup))]
    public class VRBreakHolder : UdonSharpBehaviour
    {
        VRCPickup attachedPickup;

        float breakInput;
        Vector3 initialLocalPosition;

        public float BreakInput
        {
            get
            {
                return breakInput;
            }
        }

        void Start()
        {
            attachedPickup = GetComponent<VRCPickup>();

            if (!Networking.LocalPlayer.IsUserInVR())
            {
                transform.parent.gameObject.SetActive(false);
                return;
            }

            initialLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            switch (attachedPickup.currentHand)
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

        public void DropIfHeld()
        {
            if (attachedPickup.IsHeld)
            {
                attachedPickup.Drop();
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            breakInput = 0;
            transform.localPosition = initialLocalPosition;
        }
    }
}