
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Mozilla;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WheeledVehicleBuilder : UdonSharpBehaviour
{
    //Parameters to be set in Unity
    [SerializeField] WheeledVehicleStation DriverStaion;
    [SerializeField] WheelCollider WheelPrefab;

    //Meshes
    [SerializeField] GameObject CenterFrontTemplate;
    [SerializeField] GameObject CenterMiddleTemplate;
    [SerializeField] GameObject SideFrontTemplate;
    [SerializeField] GameObject SideStraightTemplate;
    [SerializeField] GameObject SideWheelOpeningTemplate;
    public BuilderUIController LinkedUI; //For updating vehicle during sync;

    //Runtime parameters
    WheelCollider[] wheelColliders;
    WheeledVehicleController linkedController;
    Transform[] wheelMeshes;

    const int minWheels = 4;
    const int maxWheels = 12;

    public int MinWheels { get { return minWheels; } }
    public int MaxWheels { get { return maxWheels; } }

    float wheelWidth = 0.4f;

    //Bulid parameters:
    //-----------------

    //Vehicle
    [UdonSynced(UdonSyncMode.None)] public float mass;
    [UdonSynced(UdonSyncMode.None)] public float widthWithWheels;
    [UdonSynced(UdonSyncMode.None)] public float length;
    [UdonSynced(UdonSyncMode.None)] public Vector3 centerOfMassPositionRelativeToCenterBottom;
    [UdonSynced(UdonSyncMode.None)] public Vector3 driverStationPositionRelativeToCenterBottom;

    //Wheels
    [UdonSynced(UdonSyncMode.None)] public int numberOfWheels; //Divisible by 2, min = 4
    [UdonSynced(UdonSyncMode.None)] public float wheelRadius;
    [UdonSynced(UdonSyncMode.None)] public float motorTorquePerDrivenWheel;
    [UdonSynced(UdonSyncMode.None)] public float breakTorquePerWheel;
    [UdonSynced(UdonSyncMode.None)] public readonly bool[] drivenWheelPairs = new bool[maxWheels / 2];
    [UdonSynced(UdonSyncMode.None)] public readonly float[] steeringAngleDeg = new float[maxWheels / 2];

    public void Setup(WheeledVehicleController linkedController)
    {
        this.linkedController = linkedController;

        wheelColliders = new WheelCollider[0];

        DriverStaion.linkedVehicle = linkedController;
    }
    public void MakeLocalPlayerOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void SetInitialParameters()
    {
        mass = 1000;
        widthWithWheels = 2;
        length = 4;
        centerOfMassPositionRelativeToCenterBottom = 0.5f * Vector3.up;
        driverStationPositionRelativeToCenterBottom = new Vector3(0, 0.5f, 1);

        numberOfWheels = 6;
        wheelRadius = 0.5f;
        motorTorquePerDrivenWheel = 200;
        breakTorquePerWheel = 500;

        drivenWheelPairs[0] = true;
        drivenWheelPairs[1] = true;
        drivenWheelPairs[2] = true;

        steeringAngleDeg[0] = -10;
        steeringAngleDeg[1] = 0;
        steeringAngleDeg[2] = 10;
    }

    void ValidateBuildParameters()
    {
        //Verify wheel cound
        if (numberOfWheels % 2 != 0)
        {
            numberOfWheels--;
        }

        if (numberOfWheels < 4)
        {
            numberOfWheels = 4;
        }

        //Check if numbers positive
        if (wheelRadius < 0)
        {
            wheelRadius = -wheelRadius;
        }

        if (breakTorquePerWheel < 0)
        {
            breakTorquePerWheel = -breakTorquePerWheel;
        }
    }

    public override void OnDeserialization()
    {
        Debug.LogWarning("Receiving build parameters");

        BuildVehiclesBasedOnBuildParameters();

        Debug.LogWarning("Updating UI");

        LinkedUI.UpdateUIFromVehicle();

        Debug.LogWarning("Deserialization complete");
    }

    GameObject[] BodyMeshes = new GameObject[0];

    void BuildBody()
    {
        //Cleanup body
        for(int i = 0; i<BodyMeshes.Length; i++)
        {
            Destroy(BodyMeshes[i]);
        }

        int numberOfMeshes = numberOfWheels + numberOfWheels - 2 + 6 + 2 + 20; //Wheel openings, between wheels, front, center, add 20 for safety

        BodyMeshes = new GameObject[numberOfMeshes];

        int currentMeshCount = 0;


        float firstWheelPosition = length * 0.5f - wheelRadius;
        float distanceBetweenWheels = (length - wheelRadius * 2) / (numberOfWheels / 2 - 1);
        float betweenDistance = distanceBetweenWheels - wheelRadius * 2;

        //Wheel openings:
        for (int i = 0; i < numberOfWheels; i++)
        {
            GameObject wheelOpening = Instantiate(SideWheelOpeningTemplate, transform);

            BodyMeshes[currentMeshCount++] = wheelOpening;

            float sideMultiplicator = 1 - 2 * (i % 2); //1 if even, -1 if uneven

            wheelOpening.transform.localScale = new Vector3(sideMultiplicator, wheelRadius * 2, wheelRadius * 2);

            int symetricArrayIndex = i / 2;

            float forwardPosition = symetricArrayIndex * distanceBetweenWheels - firstWheelPosition;

            wheelOpening.transform.localPosition =new Vector3((widthWithWheels * 0.5f - wheelWidth) * sideMultiplicator, wheelRadius, forwardPosition);
        }

        //Between wheels:
        for (int i = 0; i < numberOfWheels - 2; i++)
        {
            GameObject sideStraight = Instantiate(SideStraightTemplate, transform);

            BodyMeshes[currentMeshCount++] = sideStraight;

            float sideMultiplicator = 1 - 2 * (i % 2);

            sideStraight.transform.localScale = new Vector3(sideMultiplicator, betweenDistance, wheelRadius * 2);

            int symetricArrayIndex = i / 2;

            float forwardPosition = symetricArrayIndex * distanceBetweenWheels - firstWheelPosition + wheelRadius;

            sideStraight.transform.localPosition = new Vector3((widthWithWheels * 0.5f - wheelWidth) * sideMultiplicator, wheelRadius, forwardPosition);
        }

        //Floor:
        GameObject floor = Instantiate(CenterMiddleTemplate, transform);
        BodyMeshes[currentMeshCount++] = floor;
        floor.transform.localPosition = new Vector3(0, wheelRadius, 0);
        floor.transform.localScale = new Vector3(widthWithWheels * 0.5f - wheelWidth, length, wheelRadius * 2);

        /*
        floor = Instantiate(floor, transform);
        BodyMeshes[currentMeshCount++] = floor;
        floor.transform.localScale = new Vector3(-floor.transform.localScale.x, floor.transform.localScale.y, floor.transform.localScale.z);
        */

        //Front and back middle:
        GameObject frontMiddle = Instantiate(CenterFrontTemplate, transform);
        BodyMeshes[currentMeshCount++] = frontMiddle;
        frontMiddle.transform.localPosition = new Vector3(0, wheelRadius, length * 0.5f);
        frontMiddle.transform.localScale = new Vector3(widthWithWheels * 0.5f - wheelWidth, frontMiddle.transform.localScale.y, wheelRadius * 2);

        GameObject backMiddle = Instantiate(frontMiddle, transform);
        BodyMeshes[currentMeshCount++] = backMiddle;
        frontMiddle.transform.localPosition = new Vector3(0, wheelRadius, -length * 0.5f);
        backMiddle.transform.localScale = new Vector3(backMiddle.transform.localScale.x, -backMiddle.transform.localScale.y, backMiddle.transform.localScale.z);

        GameObject backRight = Instantiate(SideFrontTemplate, transform);
        BodyMeshes[currentMeshCount++] = backRight;
        backRight.transform.localPosition = new Vector3(widthWithWheels * 0.5f - wheelWidth, wheelRadius, -length * 0.5f);
        backRight.transform.localScale = new Vector3(1, backRight.transform.localScale.y, wheelRadius * 2);

        GameObject backLeft = Instantiate(backRight, transform);
        BodyMeshes[currentMeshCount++] = backLeft;
        backLeft.transform.localPosition = new Vector3(-backRight.transform.localPosition.x, backRight.transform.localPosition.y, backRight.transform.localPosition.z);
        backLeft.transform.localScale = new Vector3(-backRight.transform.localScale.x, backRight.transform.localScale.y, backRight.transform.localScale.z);

        GameObject frontRight = Instantiate(backRight, transform);
        BodyMeshes[currentMeshCount++] = frontRight;
        frontRight.transform.localPosition = new Vector3(backRight.transform.localPosition.x, backRight.transform.localPosition.y, -backRight.transform.localPosition.z);
        frontRight.transform.localScale = new Vector3(backRight.transform.localScale.x, -backRight.transform.localScale.y, backRight.transform.localScale.z);

        GameObject frontLeft = Instantiate(backLeft, transform);
        BodyMeshes[currentMeshCount++] = frontLeft;
        frontLeft.transform.localPosition = new Vector3(backLeft.transform.localPosition.x, backLeft.transform.localPosition.y, -backLeft.transform.localPosition.z);
        frontLeft.transform.localScale = new Vector3(backLeft.transform.localScale.x, -backLeft.transform.localScale.y, backLeft.transform.localScale.z);
    }

    public void BuildVehiclesBasedOnBuildParameters()
    {
        Debug.LogWarning("Building vehicle according to parameters");

        if (Networking.IsOwner(gameObject))
        {
            Debug.LogWarning("Sending build parameters");
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

        DriverStaion.transform.localPosition = driverStationPositionRelativeToCenterBottom;

        Debug.Log(driverStationPositionRelativeToCenterBottom.z);

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
                newWheelColliderArray[i] = GameObject.Instantiate(WheelPrefab.gameObject, linkedController.transform).transform.GetComponent<WheelCollider>();
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
                GameObject.Destroy(wheelColliders[i].gameObject);
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

            wheelMeshes[i].localScale = new Vector3(wheelMeshes[i].localScale.x, wheelRadius * 2, wheelRadius * 2);

            wheelColliders[i].transform.localPosition = new Vector3(widthWithWheels * 0.5f * sideMultiplicator, wheelRadius, forwardPosition);

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
