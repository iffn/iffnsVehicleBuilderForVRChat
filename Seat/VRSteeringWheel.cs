using UdonSharp;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.XR;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class VRSteeringWheel : CockpitPickup
    {
        //[SerializeField] Material highlightMaterial;
        //[SerializeField] MeshRenderer highlightObject;

        Material defaultMaterial;

        float initialAngle;
        float steeringAngle;
        float maxSteeringAngleRad;
        bool leftSelecetd = false;
        bool rightSelected = false;

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

        public void Setup(float maxSteeringAngleDeg)
        {
            base.Setup();

            this.maxSteeringAngleRad = maxSteeringAngleDeg * Mathf.Deg2Rad;

            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;

            gameObject.SetActive(false);

            //if (highlightObject != null && highlightMaterial != null) defaultMaterial = highlightObject.material;
        }

        protected override void AdditionalLateUpdateFunctions()
        {
            if (!IsHeld) return;

            steeringAngle = GetHandAngle() - initialAngle;
        }

        float GetHandAngle()
        {
            Vector3 handPosition = Networking.LocalPlayer.GetTrackingData(GetCurrentTrackingDataHand()).position;

            Vector3 localHandPosition = transform.parent.InverseTransformPoint(handPosition);

            return Mathf.Atan2(localHandPosition.y, localHandPosition.x);
        }

        void ResetWheelPosition()
        {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }

        bool debugActive = false;

        void DebugFunction()
        {
            if (Input.GetKey(KeyCode.Home) || Input.GetAxis("Oculus_GearVR_DpadX") > 0.8f)
            {
                if (debugActive) return;

                //                                      V---Manually adjust this for each class ffs
                Debug.Log($"Class {nameof(VRSteeringWheel)} of GameObject {gameObject.name} worked at {Time.time}");
                
                Debug.Log($"Steering wheel is held = {IsHeld}");
                Debug.Log($"Current hand = {currentPickupHand}");
                Debug.Log($"Last left hand position = {lastLeftPositionDebug}");
                Debug.Log($"Last right hand position = {lastRightPositionDebug}");
                Debug.Log($"");

                debugActive = true;
            }
            else
            {
                debugActive = false;
            }
        }

        private void Update()
        {
            DebugFunction();
        }

        /*
        void SetDefaultMaterial()
        {
            if (highlightObject.material == defaultMaterial) return;

            highlightObject.material = defaultMaterial;
        }

        void SetHighlightMaterial()
        {
            if (highlightObject.material == highlightMaterial) return;

            highlightObject.material = highlightMaterial;
        }
        */

        protected override void PickupOccured()
        {
            Debug.Log("Wheel pickup");

            initialAngle = GetHandAngle();

            //if (defaultMaterial != null) SetDefaultMaterial();
        }

        protected override void DropOccured()
        {
            Debug.Log("Wheel drop");

            steeringAngle = 0;
            initialAngle = 0;
            ResetWheelPosition();
        }
    }
}