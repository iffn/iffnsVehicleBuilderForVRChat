
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

    //Runtime parameters
    WheelCollider[] wheelColliders;
    WheeledVehicleController linkedController;
    Transform[] wheelMeshes;

    const int minWheels = 4;
    const int maxWheels = 12;

    public int MinWheels { get { return minWheels; } }
    public int MaxWheels { get { return maxWheels; } }

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
    [UdonSynced(UdonSyncMode.None)] public readonly bool[] drivenWheelPairs = new bool[maxWheels];
    [UdonSynced(UdonSyncMode.None)] public readonly float[] steeringAngleDeg = new float[maxWheels];

    public void Setup(WheeledVehicleController linkedController)
    {
        this.linkedController = linkedController;

        wheelColliders = new WheelCollider[0];

        DriverStaion.linkedVehicle = linkedController;
    }

    public void SetInitialParameters()
    {
        mass = 1000;
        widthWithWheels = 2;
        length = 2;
        centerOfMassPositionRelativeToCenterBottom = 0.5f * Vector3.up;
        driverStationPositionRelativeToCenterBottom = 0.5f * Vector3.up;

        numberOfWheels = 6;
        wheelRadius = 0.5f;
        motorTorquePerDrivenWheel = 200;
        breakTorquePerWheel = 500;

        drivenWheelPairs[0] = true;
        drivenWheelPairs[1] = true;
        drivenWheelPairs[2] = true;

        steeringAngleDeg[0] = 10;
        steeringAngleDeg[1] = 0;
        steeringAngleDeg[2] = -10;
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
        BuildVehiclesBasedOnBuildParameters();
    }

    public void BuildVehiclesBasedOnBuildParameters()
    {
        //Body:
        //-----

        BoxCollider linkedCollider = linkedController.transform.GetComponent<BoxCollider>();

        linkedCollider.size = new Vector3(widthWithWheels, 0.5f, length);
        linkedCollider.center = wheelRadius * Vector3.up;

        linkedController.LinkedRigidbody.mass = mass;
        linkedController.LinkedRigidbody.centerOfMass = new Vector3(0, centerOfMassPositionRelativeToCenterBottom.y, centerOfMassPositionRelativeToCenterBottom.x);

        DriverStaion.transform.localPosition = new Vector3(0, driverStationPositionRelativeToCenterBottom.y, driverStationPositionRelativeToCenterBottom.x); ;

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
                newWheelColliderArray[i] = GameObject.Instantiate(WheelPrefab.gameObject).transform.GetComponent<WheelCollider>();
                newWheelColliderArray[i].transform.parent = linkedController.transform;

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

        float firstWheelPosition = (numberOfWheels / 2 - 1) * (wheelRadius * 2 + 0.2f) * 0.5f;

        wheelMeshes = new Transform[numberOfWheels];

        for (int i = 0; i < numberOfWheels; i++)
        {
            wheelMeshes[i] = wheelColliders[i].transform.GetChild(0);
        }

        //Set wheel parameters
        for (int i = 0; i < numberOfWheels; i++)
        {
            wheelColliders[i].radius = wheelRadius;

            int symetricArrayIndex = i / 2;

            float sideMultiplicator = 1 - 2 * (i % 2); //1 if even, -1 if uneven

            float forwardPosition = symetricArrayIndex * (wheelRadius * 2 + 0.2f) - firstWheelPosition;

            wheelMeshes[i].localScale = new Vector3(wheelRadius * 2, wheelMeshes[i].localScale.y, wheelRadius * 2);

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
