using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [RequireComponent(typeof(Collider))]
    public class DriveDirectionInteractor : UdonSharpBehaviour
    {
        [Header("Settings")]
        [SerializeField] float directionalOffset;

        [Header("Unity assingments")]
        [SerializeField] Transform visualMover;

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
        }

        private void Start()
        {
            SetPosition();

            attachedCollider= GetComponent<Collider>();

            attachedCollider.enabled = false;
        }

        void SetPosition()
        {
            float offset = forwardDrive ? directionalOffset : -directionalOffset;

            visualMover.localPosition = offset * Vector3.forward;
        }

        public override void Interact()
        {
            forwardDrive = !forwardDrive;

            SetPosition();
        }
    }
}