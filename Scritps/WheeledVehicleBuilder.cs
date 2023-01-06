using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class WheeledVehicleBuilder : UdonSharpBehaviour
    {
        //Parameters to be set in Unity
        [SerializeField] WheelCollider WheelPrefab;
        [SerializeField] SeatController[] AvailableSeats;

        //Body mesh templates
        [SerializeField] GameObject CenterFrontTemplate;
        [SerializeField] GameObject CenterMiddleTemplate;
        [SerializeField] GameObject SideFrontTemplate;
        [SerializeField] GameObject SideStraightTemplate;
        [SerializeField] GameObject SideWheelOpeningTemplate;
        [SerializeField] GameObject CenterOfGravityIndicator;
        [SerializeField] Transform BodyHolder;
        [SerializeField] PresetVehicleTypes initialVehicleType;

        //Runtime parameters
        WheelCollider[] wheelColliders = new WheelCollider[0];
        WheeledVehicleController linkedController;
        Transform[] wheelMeshes = new Transform[0];

        GameObject[] BodyMeshes = new GameObject[0];

        public const int minWheels = 4;
        public const int maxWheels = 12;
        public const int maxSeatRows = 5;

        //Bulid parameters:
        //-----------------

        //Vehicle
        [UdonSynced(UdonSyncMode.None)] public float mass;
        [UdonSynced(UdonSyncMode.None)] public float widthWithWheels;
        [UdonSynced(UdonSyncMode.None)] public float length;
        [UdonSynced(UdonSyncMode.None)] public float groundClearance;
        [UdonSynced(UdonSyncMode.None)] public Vector3 centerOfMassPositionRelativeToCenterBottom;
        [UdonSynced(UdonSyncMode.None)] public int numberOfSeatRows;
        [UdonSynced(UdonSyncMode.None)] public bool[] seatsMirrored;

        //Wheels
        [UdonSynced(UdonSyncMode.None)] public int numberOfWheels; //Divisible by 2, min = 4
        [UdonSynced(UdonSyncMode.None)] public float wheelRadius;
        [UdonSynced(UdonSyncMode.None)] public float wheelWidth;
        [UdonSynced(UdonSyncMode.None)] public readonly bool[] drivenWheelPairs = new bool[maxWheels / 2];
        [UdonSynced(UdonSyncMode.None)] public float motorTorquePerDrivenWheel;
        [UdonSynced(UdonSyncMode.None)] public float breakTorquePerWheel;
        [UdonSynced(UdonSyncMode.None)] public readonly float[] steeringAngleDeg = new float[maxWheels / 2];


        //Funcitons:
        //----------

        public bool EnableStationEntry
        {
            set
            {
                foreach(SeatController controller in AvailableSeats)
                {
                    controller.EnableStationEntry = value;
                }
            }
        }

        public void Setup(WheeledVehicleController linkedController)
        {
            this.linkedController = linkedController;

            seatsMirrored = new bool[maxSeatRows];
        }

        public void MakeLocalPlayerOwner()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void SetInitialParameters()
        {
            mass = 1000;

            switch (initialVehicleType)
            {
                case PresetVehicleTypes.ATV6Wheel:

                    widthWithWheels = 2;
                    length = 3.2f;
                    groundClearance = 0.4f;
                    centerOfMassPositionRelativeToCenterBottom = 0.5f * Vector3.up;
                    numberOfSeatRows = 3;
                    seatsMirrored[0] = false;

                    for(int i = 1; i< seatsMirrored.Length; i++)
                    {
                        seatsMirrored[i] = true;
                    }

                    numberOfWheels = 6;
                    wheelRadius = 0.5f;
                    wheelWidth = 0.4f;
                    motorTorquePerDrivenWheel = 200;
                    breakTorquePerWheel = 500;

                    drivenWheelPairs[0] = true;
                    drivenWheelPairs[1] = true;
                    drivenWheelPairs[2] = true;

                    steeringAngleDeg[0] = -10;
                    steeringAngleDeg[1] = 0;
                    steeringAngleDeg[2] = 10;

                    break;
                case PresetVehicleTypes.Car:
                    widthWithWheels = 1.8f;
                    length = 3f;
                    groundClearance = 0.3f;
                    centerOfMassPositionRelativeToCenterBottom = 0.5f * Vector3.up;
                    numberOfSeatRows = 2;
                    
                    for (int i = 0; i < seatsMirrored.Length; i++)
                    {
                        seatsMirrored[i] = true;
                    }

                    numberOfWheels = 4;
                    wheelRadius = 0.4f;
                    wheelWidth = 0.3f;
                    motorTorquePerDrivenWheel = 400;
                    breakTorquePerWheel = 500;

                    drivenWheelPairs[0] = true;
                    drivenWheelPairs[1] = true;
                    drivenWheelPairs[2] = true;

                    steeringAngleDeg[0] = -25;
                    break;
                case PresetVehicleTypes.Monstertruck:
                    widthWithWheels = 6.8f;
                    length = 8.6f;
                    groundClearance = 1.2f;
                    centerOfMassPositionRelativeToCenterBottom = 0.5f * Vector3.up;
                    numberOfSeatRows = 1;
                    
                    for (int i = 0; i < seatsMirrored.Length; i++)
                    {
                        seatsMirrored[i] = true;
                    }

                    numberOfWheels = 4;
                    wheelRadius = 1.5f;
                    wheelWidth = 0.6f;
                    motorTorquePerDrivenWheel = 1500;
                    breakTorquePerWheel = 3000;

                    drivenWheelPairs[0] = true;
                    drivenWheelPairs[1] = false;

                    steeringAngleDeg[0] = -15;
                    steeringAngleDeg[1] = 15;
                    break;
                default:
                    break;
            }
        }

        bool usesValidBuildParameters()
        {
            //Verify wheel cound
            if (numberOfWheels % 2 != 0)
            {
                return false;
                
            }

            if (numberOfWheels < minWheels)
            {
                return false;
            }

            //Check if numbers positive
            if (wheelRadius < 0)
            {
                return false;
            }

            if (breakTorquePerWheel < 0)
            {
                return false;
            }

            return true;
        }

        public override void OnDeserialization()
        {
            Debug.LogWarning("Receiving build parameters");

            BuildFromParameters();
        }

        public void BuildFromParameters()
        {
            BuildVehicleBasedOnBuildParameters();

            linkedController.LinkedUI.UpdateUIFromVehicle();
        }

        public void DelayedBuild()
        {
            if (usesValidBuildParameters())
            {
                Debug.LogWarning("Parameters useful, building vehicle");


                BuildVehicleBasedOnBuildParameters();

                Debug.LogWarning("Updating UI");

                linkedController.LinkedUI.UpdateUIFromVehicle();

                Debug.LogWarning("Deserialization complete");
            }
            else
            {
                Debug.LogWarning("Parameters not useful! Sending delayed event");
                SendCustomEventDelayedFrames(nameof(DelayedBuild), 0);
            }
        }
        
        void BuildBody()
        {
            BodyHolder.localPosition = groundClearance * Vector3.up;

            float yScale = 2 - groundClearance / wheelRadius;

            yScale = Mathf.Clamp(yScale, 0.1f, 10);

            BodyHolder.localScale = new Vector3(1, yScale, 1); // Shortened from (2*r - d)/r 

            //Cleanup body
            for (int i = 0; i < BodyMeshes.Length; i++)
            {
                Destroy(BodyMeshes[i]);
            }

            int numberOfMeshes = numberOfWheels + numberOfWheels - 2 + 1 + 3 + 3; //Wheel openings, between wheels, center, front, back

            BodyMeshes = new GameObject[numberOfMeshes];

            int currentMeshCount = 0;

            float firstWheelPosition = length * 0.5f - wheelRadius;
            float distanceBetweenWheels = (length - wheelRadius * 2) / (numberOfWheels / 2 - 1);
            float betweenDistance = distanceBetweenWheels - wheelRadius * 2;

            //Wheel openings:
            for (int i = 0; i < numberOfWheels; i++)
            {
                GameObject wheelOpening = Instantiate(SideWheelOpeningTemplate, BodyHolder);

                BodyMeshes[currentMeshCount++] = wheelOpening;

                float sideMultiplicator = 1 - 2 * (i % 2); //1 if even, -1 if uneven

                wheelOpening.transform.localScale = new Vector3(sideMultiplicator * wheelWidth, wheelRadius * 2, wheelRadius * 2);

                int symetricArrayIndex = i / 2;

                float forwardPosition = symetricArrayIndex * distanceBetweenWheels - firstWheelPosition;

                wheelOpening.transform.localPosition = new Vector3((widthWithWheels * 0.5f - wheelWidth) * sideMultiplicator, 0, forwardPosition);
            }

            //Between wheels:
            for (int i = 0; i < numberOfWheels - 2; i++)
            {
                GameObject sideStraight = Instantiate(SideStraightTemplate, BodyHolder);

                BodyMeshes[currentMeshCount++] = sideStraight;

                float sideMultiplicator = 1 - 2 * (i % 2);

                sideStraight.transform.localScale = new Vector3(sideMultiplicator * wheelWidth, betweenDistance, wheelRadius * 2);

                int symetricArrayIndex = i / 2;

                float forwardPosition = symetricArrayIndex * distanceBetweenWheels - firstWheelPosition + wheelRadius;

                sideStraight.transform.localPosition = new Vector3((widthWithWheels * 0.5f - wheelWidth) * sideMultiplicator, 0, forwardPosition);
            }

            //Floor:
            GameObject floor = Instantiate(CenterMiddleTemplate, BodyHolder);
            BodyMeshes[currentMeshCount++] = floor;
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(widthWithWheels * 0.5f - wheelWidth, length, wheelRadius * 2);

            /*
            floor = Instantiate(floor, transform);
            BodyMeshes[currentMeshCount++] = floor;
            floor.transform.localScale = new Vector3(-floor.transform.localScale.x, floor.transform.localScale.y, floor.transform.localScale.z);
            */

            //Front and back middle:
            GameObject frontMiddle = Instantiate(CenterFrontTemplate, BodyHolder);
            BodyMeshes[currentMeshCount++] = frontMiddle;
            frontMiddle.transform.localPosition = new Vector3(0, 0, length * 0.5f);
            frontMiddle.transform.localScale = new Vector3(widthWithWheels * 0.5f - wheelWidth, frontMiddle.transform.localScale.y, wheelRadius * 2);

            GameObject backMiddle = Instantiate(frontMiddle, BodyHolder);
            BodyMeshes[currentMeshCount++] = backMiddle;
            frontMiddle.transform.localPosition = new Vector3(0, 0, -length * 0.5f);
            backMiddle.transform.localScale = new Vector3(backMiddle.transform.localScale.x, -backMiddle.transform.localScale.y, backMiddle.transform.localScale.z);

            GameObject backRight = Instantiate(SideFrontTemplate, BodyHolder);
            BodyMeshes[currentMeshCount++] = backRight;
            backRight.transform.localPosition = new Vector3(widthWithWheels * 0.5f - wheelWidth, 0, -length * 0.5f);
            backRight.transform.localScale = new Vector3(wheelWidth, backRight.transform.localScale.y, wheelRadius * 2);

            GameObject backLeft = Instantiate(backRight, BodyHolder);
            BodyMeshes[currentMeshCount++] = backLeft;
            backLeft.transform.localPosition = new Vector3(-backRight.transform.localPosition.x, backRight.transform.localPosition.y, backRight.transform.localPosition.z);
            backLeft.transform.localScale = new Vector3(-wheelWidth, backRight.transform.localScale.y, backRight.transform.localScale.z);

            if(currentMeshCount <= numberOfMeshes)
            {
                GameObject frontRight = Instantiate(backRight, BodyHolder);

                BodyMeshes[currentMeshCount++] = frontRight;
                frontRight.transform.localPosition = new Vector3(backRight.transform.localPosition.x, backRight.transform.localPosition.y, -backRight.transform.localPosition.z);
                frontRight.transform.localScale = new Vector3(wheelWidth, -backRight.transform.localScale.y, backRight.transform.localScale.z);
            }
            else
            {
                Debug.LogWarning($"Out of bounds 1 while building mesh with {numberOfMeshes} meshes using {numberOfWheels} wheels");
            }

            if (currentMeshCount <= numberOfMeshes)
            {
                GameObject frontLeft = Instantiate(backLeft, BodyHolder);
                BodyMeshes[currentMeshCount++] = frontLeft;
                frontLeft.transform.localPosition = new Vector3(backLeft.transform.localPosition.x, backLeft.transform.localPosition.y, -backLeft.transform.localPosition.z);
                frontLeft.transform.localScale = new Vector3(-wheelWidth, -backLeft.transform.localScale.y, backLeft.transform.localScale.z);
            }
            else
            {
                Debug.LogWarning($"Out of bounds 2 while building mesh with {numberOfMeshes} meshes using {numberOfWheels} wheels");
            }
        }

        public void BuildVehicleBasedOnBuildParameters()
        {
            if (Networking.IsOwner(gameObject))
            {
                RequestSerialization();
            }

            //Body:
            //-----

            BuildBody();

            BoxCollider linkedCollider = linkedController.transform.GetComponent<BoxCollider>();

            linkedCollider.size = new Vector3(widthWithWheels, 0.5f, length);
            linkedCollider.center = wheelRadius * Vector3.up;

            linkedController.LinkedRigidbody.mass = mass;
            linkedController.LinkedRigidbody.centerOfMass = centerOfMassPositionRelativeToCenterBottom;
            CenterOfGravityIndicator.transform.localPosition = centerOfMassPositionRelativeToCenterBottom;

            //Seats
            float seatXPos = 0.3f;
            float seatZPosOffset = length * 0.5f / (numberOfSeatRows - 1);
            float seatHeight = wheelRadius + 0.1f; //+0.1 because of body thickness

            for(int i = 0; i < maxSeatRows; i++)
            {
                int firstSeat = i * 2;
                int secondSeat = firstSeat + 1;

                if(i < numberOfSeatRows)
                {
                    float zPos = length * 0.25f - seatZPosOffset * i;

                    AvailableSeats[firstSeat].gameObject.SetActive(true);

                    if (seatsMirrored[i])
                    {
                        AvailableSeats[firstSeat].transform.localPosition = new Vector3(-seatXPos, seatHeight, zPos);
                        AvailableSeats[secondSeat].transform.localPosition = new Vector3(seatXPos, seatHeight, zPos);
                        AvailableSeats[secondSeat].gameObject.SetActive(true);
                    }
                    else
                    {
                        AvailableSeats[firstSeat].transform.localPosition = new Vector3(0, seatHeight, zPos);
                        AvailableSeats[secondSeat].gameObject.SetActive(false);
                    }
                }
                else
                {
                    AvailableSeats[firstSeat].gameObject.SetActive(false);
                    AvailableSeats[secondSeat].gameObject.SetActive(false);
                }
            }

            //Wheels:
            //-------

            //Generate correct number of wheel colliders
            if (numberOfWheels > wheelColliders.Length)
            {
                WheelCollider[] newWheelColliderArray = new WheelCollider[numberOfWheels];

                for (int i = 0; i < wheelColliders.Length; i++)
                {
                    newWheelColliderArray[i] = wheelColliders[i];
                    wheelColliders[i].enabled = false;
                }

                for (int i = wheelColliders.Length; i < numberOfWheels; i++)
                {
                    newWheelColliderArray[i] = Instantiate(WheelPrefab.gameObject, linkedController.transform).transform.GetComponent<WheelCollider>();
                }

                wheelColliders = newWheelColliderArray;
            }
            else if (numberOfWheels < wheelColliders.Length)
            {
                WheelCollider[] newWheelColliderArray = new WheelCollider[numberOfWheels];

                for (int i = 0; i < numberOfWheels; i++)
                {
                    newWheelColliderArray[i] = wheelColliders[i];
                }

                for (int i = numberOfWheels; i < wheelColliders.Length; i++)
                {
                    Destroy(wheelColliders[i].gameObject);
                }

                wheelColliders = newWheelColliderArray;
            }

            wheelMeshes = new Transform[numberOfWheels];

            for (int i = 0; i < numberOfWheels; i++)
            {
                wheelMeshes[i] = wheelColliders[i].transform.GetChild(0);
            }

            //Set wheel parameters
            float firstWheelPosition = -length * 0.5f + wheelRadius;
            float distanceBetweenWheels = -(length - wheelRadius * 2) / (numberOfWheels / 2 - 1);

            for (int i = 0; i < numberOfWheels; i++)
            {
                wheelColliders[i].radius = wheelRadius;

                int symetricArrayIndex = i / 2;

                float sideMultiplicator = 1 - 2 * (i % 2); //1 if even, -1 if uneven

                float forwardPosition = symetricArrayIndex * distanceBetweenWheels - firstWheelPosition;

                wheelMeshes[i].localScale = new Vector3(wheelWidth, wheelRadius * 2, wheelRadius * 2);

                wheelColliders[i].transform.localPosition = new Vector3((widthWithWheels * 0.5f - wheelWidth * 0.5f) * sideMultiplicator, wheelRadius, forwardPosition);

                wheelColliders[i].enabled = true;
            }

            linkedController.SetBuildParameters(
                numberOfWheels: numberOfWheels,
                motorTorquePerDrivenWheel: motorTorquePerDrivenWheel,
                breakTorquePerWheel: breakTorquePerWheel,
                drivenWheels: drivenWheelPairs,
                steeringAngleDeg: steeringAngleDeg,
                wheelColliders: wheelColliders,
                wheelMeshes: wheelMeshes);
        }

    }

    public enum PresetVehicleTypes
    {
        ATV6Wheel,
        Car,
        Monstertruck
    }
}