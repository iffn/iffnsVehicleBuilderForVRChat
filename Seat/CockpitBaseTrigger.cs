using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class CockpitBaseTrigger : UdonSharpBehaviour //This class should be abstract but isn't because of U# being U#
    {
        public PickupColliderShapes pickupShape = PickupColliderShapes.CylinderAlongY;

        HandType handNeverNone;

        protected Vector3 lastRightPositionDebug;
        protected Vector3 lastLeftPositionDebug;

        [SerializeField] protected LineRenderer LeftLineRenderer;
        [SerializeField] protected LineRenderer RightLineRenderer;

        bool validInteractionFeedback;

        bool leftHandWasInRange;
        bool rightHandWasInRange;
        
        protected bool ValidInteractionFeedback
        {
            get
            {
                return validInteractionFeedback;
            }
        }

        protected virtual void Setup()
        {
            validInteractionFeedback = LeftLineRenderer != null && RightLineRenderer != null;
        }

        public bool HandIsInRange(HandType hand)
        {
            VRCPlayerApi.TrackingDataType trackingHand = GetTrackingHandFromControllerHand(hand);

            switch (pickupShape)
            {
                case PickupColliderShapes.CylinderAlongY:
                    return HandIsInRangeOfCylinder(trackingHand);
                case PickupColliderShapes.Sphere:
                    return HandIsInRangeOfSphere(trackingHand);
                default:
                    Debug.LogWarning($"Use of unknown enum state of {nameof(PickupColliderShapes)} called {pickupShape}.");
                    break;
            }

            return false;
        }

        bool HandIsInRangeOfCylinder(VRCPlayerApi.TrackingDataType hand)
        {
            //Get hand position
            Vector3 localHandPosition = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(hand).position);

            switch (hand)
            {
                case VRCPlayerApi.TrackingDataType.Head:
                    break;
                case VRCPlayerApi.TrackingDataType.LeftHand:
                    lastLeftPositionDebug = localHandPosition;
                    break;
                case VRCPlayerApi.TrackingDataType.RightHand:
                    lastRightPositionDebug = localHandPosition;
                    break;
                case VRCPlayerApi.TrackingDataType.Origin:
                    break;
                default:
                    break;
            }

            //Check main axis offset
            if (Mathf.Abs(localHandPosition.y) > 1) return false;

            localHandPosition.y = 0;

            return localHandPosition.magnitude < 0.5f;
        }

        bool HandIsInRangeOfSphere(VRCPlayerApi.TrackingDataType hand)
        {
            Vector3 localHandPosition = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(hand).position);

            switch (hand)
            {
                case VRCPlayerApi.TrackingDataType.Head:
                    break;
                case VRCPlayerApi.TrackingDataType.LeftHand:
                    lastLeftPositionDebug = localHandPosition;
                    break;
                case VRCPlayerApi.TrackingDataType.RightHand:
                    lastRightPositionDebug = localHandPosition;
                    break;
                case VRCPlayerApi.TrackingDataType.Origin:
                    break;
                default:
                    break;
            }

            return localHandPosition.magnitude < 0.5f;
        }

        bool HandIsInRangeOfCube(VRCPlayerApi.TrackingDataType hand)
        {
            Vector3 localHandPosition = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(hand).position);

            switch (hand)
            {
                case VRCPlayerApi.TrackingDataType.Head:
                    break;
                case VRCPlayerApi.TrackingDataType.LeftHand:
                    lastLeftPositionDebug = localHandPosition;
                    break;
                case VRCPlayerApi.TrackingDataType.RightHand:
                    lastRightPositionDebug = localHandPosition;
                    break;
                case VRCPlayerApi.TrackingDataType.Origin:
                    break;
                default:
                    break;
            }

            return (Mathf.Abs(localHandPosition.x) > 0.25f && Mathf.Abs(localHandPosition.y) > 0.25f && Mathf.Abs(localHandPosition.z) > 0.25f);
        }

        protected void InteractionTriggerForLateUpdate()
        {
            bool leftHandInRange = HandIsInRange(HandType.LEFT);
            bool rightHandInRange = HandIsInRange(HandType.RIGHT);

            if (leftHandInRange)
            {
                if (!leftHandWasInRange)
                {
                    LeftLineRenderer.gameObject.SetActive(true);
                    Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.1f, 0.3f, 0.5f);
                }

                LeftLineRenderer.SetPosition(0, transform.position);
                LeftLineRenderer.SetPosition(1, Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position);
            }
            else
            {
                LeftLineRenderer.gameObject.SetActive(false);
            }

            if (rightHandInRange)
            {
                if (!rightHandWasInRange)
                {
                    RightLineRenderer.gameObject.SetActive(true);
                    Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.1f, 0.3f, 0.5f);
                }

                RightLineRenderer.SetPosition(0, transform.position);
                RightLineRenderer.SetPosition(1, Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
            }
            else
            {
                RightLineRenderer.gameObject.SetActive(false);
            }

            leftHandWasInRange = leftHandInRange;
            rightHandWasInRange = rightHandInRange;
        }

        

        public VRCPlayerApi.TrackingDataType GetTrackingHandFromControllerHand(HandType hand)
        {
            switch (hand)
            {
                case HandType.RIGHT:
                    return VRCPlayerApi.TrackingDataType.RightHand;
                case HandType.LEFT:
                    return VRCPlayerApi.TrackingDataType.LeftHand;
                default:
                    Debug.LogWarning($"Use of unknown enum state of {nameof(HandType)} called {hand}. Using RightHand.");
                    return VRCPlayerApi.TrackingDataType.RightHand;
            }
        }

        public VRCPickup.PickupHand GetPickupHandFromControllerHand(HandType hand)
        {
            switch (hand)
            {
                case HandType.RIGHT:
                    return VRC_Pickup.PickupHand.Right;
                case HandType.LEFT:
                    return VRC_Pickup.PickupHand.Left;
                default:
                    Debug.LogWarning($"Use of unknown enum state of {nameof(HandType)} called {hand}. Using RightHand.");
                    return VRC_Pickup.PickupHand.Right;
            }
        }
    }

    public enum PickupColliderShapes
    {
        CylinderAlongY,
        Sphere,
        Cube
    }
}