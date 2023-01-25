
using iffnsStuff.iffnsVRCStuff.WheeledVehicles;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WheeledVehicleSeatController : SeatController
{
    [SerializeField] Transform Scaler;

    WheeledVehicleController linkedVehicle;
    VRCStation linkedVRCStaion;

    public void Setup(WheeledVehicleController linkedVehicle)
    {
        this.linkedVehicle = linkedVehicle;

        linkedVRCStaion = transform.GetComponent<VRCStation>();
        linkedVRCStaion.disableStationExit = true;

        linkedVRCStaion.canUseStationFromStation = false; //Set in script since prefab values don't get saved
    }

    protected override void UpdateFunction()
    {
        base.UpdateFunction();

        if (SeatedPlayer != null && SeatedPlayer.isLocal)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                linkedVRCStaion.ExitStation(Networking.LocalPlayer);
            }
        }
    }

    public void ScaleWheel(VRCPlayerApi player)
    {
        /*

        Vector3 leftShoulderPosition = player.GetBonePosition(HumanBodyBones.LeftShoulder);
        Vector3 headPosition = player.GetBonePosition(HumanBodyBones.Head);
        float shoulderDistance = (leftShoulderPosition - rightShoulderPosition).magnitude;
        float middleShoulderToHeadDistance = (0.5f * (rightShoulderPosition + leftShoulderPosition) - headPosition).magnitude;

        if (shoulderDistance < minAvatarDistance || shoulderDistance > maxAvatarDistance || rightArmLength < minAvatarDistance || rightArmLength > maxAvatarDistance)
        */

        Vector3 rightShoulderPosition = player.GetBonePosition(HumanBodyBones.RightShoulder);
        Vector3 rightEllbowPosition = player.GetBonePosition(HumanBodyBones.RightLowerArm);
        Vector3 rightHandPosition = player.GetBonePosition(HumanBodyBones.RightHand);
        


        float lowerArmLength = (rightEllbowPosition - rightHandPosition).magnitude;
        float upperArmLength = (rightShoulderPosition - rightEllbowPosition).magnitude;

        float rightArmLength = lowerArmLength + upperArmLength;

        Debug.Log("Right arm lenght = " + rightArmLength);

        if (rightArmLength < minAvatarDistance || rightArmLength > maxAvatarDistance)
        {
            //Invalid avatar size
            Debug.Log("Invalid player size detected. Scaling with assumption height");

            float referenceSize;

            Vector3 playerPosition = player.GetPosition();

            if (player.isLocal)
            {
                Vector3 headPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                referenceSize = (headPosition - playerPosition).magnitude;
            }
            else
            {
                referenceSize = 1.6f; //Eye to feet distance
            }

            rightArmLength = referenceSize * 0.375f; //Arm length = 0.6, Eye to feet distance = 1.6
        }

        Scaler.transform.localScale = rightArmLength * Vector3.one;

        /*
        Vector3 localWheelPosition = Vector3.zero;

        localWheelPosition.z = targetHeadPosition.localPosition.x + lowerArmLength;
        localWheelPosition.y = targetHeadPosition.localPosition.y - upperArmLength - middleShoulderToHeadDistance;

        float wheelDiameter = shoulderDistance;

        wheel.transform.localPosition = localWheelPosition;
        wheel.transform.localScale = wheelDiameter * Vector3.one;
        */
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        base.OnStationEntered(player);

        if (player.isLocal)
        {
            linkedVehicle.EnteredDriverSeat();
        }

        ScaleWheel(player);
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        base.OnStationExited(player);

        linkedVehicle.ExitedDriverSeat();
    }
}
