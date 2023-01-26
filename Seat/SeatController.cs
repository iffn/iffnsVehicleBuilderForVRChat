
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [RequireComponent(typeof(VRCStation))]
    public class SeatController : UdonSharpBehaviour
    {
        [SerializeField] protected PlayerTrackingTypes playerTrackingType;
        [SerializeField] protected Transform targetHeadPosition;
        [SerializeField] protected Transform targetHipPosition;
        [SerializeField] Transform playerMover;
        [SerializeField] float positioningTime = 0.3f;

        float entryTime = Mathf.NegativeInfinity;

        protected const float minAvatarDistance = 0.1f;
        protected const float maxAvatarDistance = 5f;
        Collider attacheCollider;

        VRCStation attachedStation;

        VRCPlayerApi seatedPlayer;
        public VRCPlayerApi SeatedPlayer
        {
            get
            {
                return seatedPlayer;
            }
        }

        public bool EnableStationEntry
        {
            set
            {
                transform.GetComponent<Collider>().enabled = value;
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

        //Unity functions
        void Start()
        {
            attacheCollider = transform.GetComponent<Collider>();
            attachedStation = transform.GetComponent<VRCStation>();
        }

        private void Update()
        {
            UpdateFunction();
        }

        virtual protected void UpdateFunction() //Use separate update function for overriding
        {
            if (entryTime + positioningTime > Time.time)
            {
                PositionStation(Networking.LocalPlayer);
            }
        }


        //Custom functions
        public void ForceEnter()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        public void ForceExit()
        {
            if (seatedPlayer == null || !seatedPlayer.isLocal) return;

            attachedStation.ExitStation(Networking.LocalPlayer);
        }

        void PositionStation(VRCPlayerApi player)
        {
            Vector3 trackingPosition;
            Vector3 headTrackingPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Vector3 playerPosition = Networking.LocalPlayer.GetPosition();


            float checkDistance;

            Vector3 targetPosition;

            switch (playerTrackingType)
            {
                case PlayerTrackingTypes.Feet:
                    //No need to move
                    return;
                case PlayerTrackingTypes.Hip:
                    trackingPosition = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Hips);

                    checkDistance = (trackingPosition - playerPosition).magnitude;
                    if (checkDistance < minAvatarDistance || checkDistance > maxAvatarDistance)
                    {
                        trackingPosition = 0.5f * (headTrackingPosition + playerPosition);
                    }

                    targetPosition = targetHipPosition.position;

                    break;
                case PlayerTrackingTypes.Head:
                    trackingPosition = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);

                    checkDistance = (trackingPosition - playerPosition).magnitude;
                    if (checkDistance < minAvatarDistance || checkDistance > maxAvatarDistance)
                    {
                        trackingPosition = headTrackingPosition;
                    }

                    targetPosition = targetHeadPosition.position;

                    break;
                default:
                    Debug.LogWarning($" Enum state {playerTrackingType} of Enum {nameof(PlayerTrackingTypes)} not defined in function {nameof(PositionStation)}");
                    return;
            }

            Vector3 offset = transform.InverseTransformVector(targetPosition - trackingPosition);

            offset.x = 0;

            playerMover.transform.localPosition += offset;
        }

        //VRChat functions:
        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            seatedPlayer = player;
            attacheCollider.enabled = false;

            if (!player.isLocal) return;

            PositionStation(player);

            entryTime = Time.time;

        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            seatedPlayer = null;

            attacheCollider.enabled = true;

            playerMover.localPosition = Vector3.zero;

            entryTime = Mathf.NegativeInfinity;
        }
    }

    public enum PlayerTrackingTypes
    {
        Feet,
        Hip,
        Head
    }

    public enum StationOccupantTypes
    {
        noone,
        me,
        someoneElse
    }
}

