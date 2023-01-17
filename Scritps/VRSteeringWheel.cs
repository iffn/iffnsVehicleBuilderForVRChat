using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(VRCPickup))]
    public class VRSteeringWheel : UdonSharpBehaviour
    {
        VRCPickup attachedPickup;
        float initialAngle;
        float steeringAngle;
        float maxSteeringAngleRad;

        Collider attachedCollider;

        Vector3 initialLocalPosition;
        Quaternion initialLocalRotation;

        public float SteeringInput
        {
            get
            {
                return Mathf.Clamp(steeringAngle / maxSteeringAngleRad, -1, 1);
            } 
        }

        public VRC_Pickup.PickupHand currentHand
        {
            get
            {
                return attachedPickup.currentHand;
            }
        }

        public void Setup(float maxSteeringAngleDeg)
        {
            attachedPickup = transform.GetComponent<VRCPickup>();
            attachedCollider = transform.GetComponent<Collider>();
            this.maxSteeringAngleRad = maxSteeringAngleDeg * Mathf.Deg2Rad;

            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
        }

        private void Start()
        {
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                gameObject.SetActive(false);
            }
        }

        /*
        public void SetSteeringWheelPositionRelativeToPlayer()
        {
            float playerHeight = (Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head) - Networking.LocalPlayer.GetPosition()).magnitude;

            Vector2 positionRelativeToHead; //x = away, y = up

            positionRelativeToHead.x = playerHeight * 0.1f;
            positionRelativeToHead.y = -playerHeight * 0.6f;

            transform.parent.localScale = playerHeight * 0.5f * Vector3.one;

            transform.parent.position =
                Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position
                + transform.parent.forward * positionRelativeToHead.x
                + transform.parent.up * positionRelativeToHead.y;
        }
        */

        public void UpdateControlls()
        {
            if (!attachedPickup.IsHeld)
            {
                return;
            }

            steeringAngle = GetHandAngle() - initialAngle;
        }

        public void ResetWheelPosition()
        {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }

        float GetHandAngle()
        {
            Vector3 handPosition;

            switch (attachedPickup.currentHand)
            {
                case VRC_Pickup.PickupHand.None:
                    Debug.LogWarning("Get hand angle of VRWheel could not be run because the pickup hand is None");
                    return 0;
                case VRC_Pickup.PickupHand.Left:
                    handPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    break;
                case VRC_Pickup.PickupHand.Right:
                    handPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    break;
                default:
                    Debug.LogWarning("Get hand angle of VRWheel could not be run because the pickup hand is not defined: " + nameof(attachedPickup.currentHand));
                    return 0;
            }

            Vector3 localHandPosition = transform.parent.InverseTransformPoint(handPosition);

            return Mathf.Atan2(localHandPosition.y, localHandPosition.x);
        }

        public void DropIfHeld()
        {
            if (!attachedPickup.IsHeld) return;

            attachedPickup.Drop();
        }

        public override void OnPickup()
        {
            initialAngle = GetHandAngle();
            attachedCollider.enabled = false;
        }

        public override void OnDrop()
        {
            attachedCollider.enabled = true;
            steeringAngle = 0;
            initialAngle = 0;
            ResetWheelPosition();
        }
    }
}