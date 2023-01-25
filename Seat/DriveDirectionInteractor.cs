using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class DriveDirectionInteractor : CockpitBaseTrigger
    {
        [Header("Settings")]
        [SerializeField] float directionalOffset;

        [Header("Unity assingments")]
        [SerializeField] Transform visualMover;

        public bool ShowInteractionFeedback;

        bool forwardDrive = true;

        Collider attachedCollider;

        public bool ForwardDrive
        {
            get
            {
                return forwardDrive;
            }
            set
            {
                forwardDrive = value;
                SetPosition();
            }
        }

        public bool ColliderState
        {
            set
            {
                attachedCollider.enabled = value;
            }
            get
            {
                return attachedCollider.enabled;
            }
        }

        private void Start()
        {
            SetPosition();

            attachedCollider= GetComponent<Collider>();

            attachedCollider.enabled = false;
        }

        private void LateUpdate()
        {
            if (!ShowInteractionFeedback)
            {
                InteractionTriggerForLateUpdate();
            }
        }

        void SetPosition()
        {
            float offset = forwardDrive ? directionalOffset : -directionalOffset;

            visualMover.localPosition = offset * Vector3.forward;
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (HandIsInRange(args.handType))
            {
                forwardDrive = !forwardDrive;

                SetPosition();
            }
        }
    }
}