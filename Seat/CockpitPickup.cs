using UdonSharp;
using UnityEngine;
using UnityEngine.XR;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class CockpitPickup : CockpitBaseTrigger //This class should be abstract but isn't because of U# being U#
    {
        /*
            Custom pickup script for VRChat:

            Advantages:
            + Uses shape as interaction distance without any additional distance
            + Should work at hight speed due to pickup detection in LateUpdate
            + Custom interaction or pickup behavior 
            + Pickup behavior for Index and other controllers the same way as they are in VRChat
        */

        public bool UsuallyHeldForLongerThanAFewSeconds;
        public bool ShowInteractionFeedback;

        bool isHeld = false;
        bool indexUser = false;
        HandType currentHandNeverNone;
        bool tryPickupNextLateUpdate;

        bool setupComplete = false;

        public bool IsHeld
        {
            get
            {
                return isHeld;
            }
        }

        public VRC_Pickup.PickupHand currentPickupHand
        {
            get
            {
                if (!isHeld) return VRC_Pickup.PickupHand.None;

                return GetPickupHandFromControllerHand(currentHandNeverNone);
            }
        }

        public VRCPlayerApi.TrackingDataType GetCurrentTrackingDataHand()
        {
            return GetTrackingHandFromControllerHand(currentHandNeverNone);
        }

        protected override void Setup()
        {
            base.Setup();

            setupComplete = true;

            //Index user detection
            string[] controllers = Input.GetJoystickNames();

            indexUser = false;

            foreach (string controller in controllers)
            {
                if (!controller.ToLower().Contains("index")) continue;

                indexUser = true;
                break;
            }
        }

        void CheckPickup()
        {
            if (!tryPickupNextLateUpdate) return;

            TryPickup();

            tryPickupNextLateUpdate = false;
        }



        private void LateUpdate()
        {
            if (!setupComplete) return;

            CheckPickup();

            //Indicators
            if (!isHeld && ShowInteractionFeedback)
            {
                InteractionTriggerForLateUpdate();
            }

            AdditionalLateUpdateFunctions();
        }

        void TryPickup()
        {
            if (HandIsInRange(currentHandNeverNone)) pickup();
        }

        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            //Pickup
            if (!isHeld && value)
            {
                //Try pickup
                currentHandNeverNone = args.handType;
                tryPickupNextLateUpdate = true;
            }

            //Drop when letting go of grab
            if (indexUser || !UsuallyHeldForLongerThanAFewSeconds)
            {
                if (args.handType != currentHandNeverNone) return;

                if (IsHeld && !value)
                {
                    drop();
                }
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            //Never called on index

            if (!UsuallyHeldForLongerThanAFewSeconds) return;

            if (isHeld && value && args.handType == currentHandNeverNone)
            {
                drop();
            }
        }

        protected virtual void DropOccured()
        {
            //For override in derived classes
        }

        protected virtual void PickupOccured()
        {
            //For override in derived classes
        }

        protected virtual void AdditionalLateUpdateFunctions()
        {

        }

        public void ForcePickup(HandType hand)
        {
            pickup();
        }

        public void ForceDropIfHeld()
        {
            drop();
        }

        void pickup()
        {
            isHeld = true;

            if(LeftLineRenderer!= null) LeftLineRenderer.gameObject.SetActive(false);
            if(RightLineRenderer!= null) RightLineRenderer.gameObject.SetActive(false);

            Networking.LocalPlayer.PlayHapticEventInHand(GetPickupHandFromControllerHand(currentHandNeverNone), 0.2f, 0.5f, 0.3f);

            PickupOccured();
        }

        void drop()
        {
            isHeld = false;

            DropOccured();
        }
    }

}
