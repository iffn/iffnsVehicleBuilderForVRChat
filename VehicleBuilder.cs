
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Mozilla;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class VehicleBuilder : UdonSharpBehaviour
{
    //Parameters to be set in Unity
    [SerializeField] VehicleStation DriverStaion;
    [SerializeField] WheelCollider WheelPrefab;

    //Runtime parameters
    WheelCollider[] wheelColliders;
    VehicleController linkedController;
    Transform[] wheelMeshes;

    //Bulid parameters:
    //-----------------

    //Vehicle
    [UdonSynced(UdonSyncMode.None)] float mass;
    [UdonSynced(UdonSyncMode.None)] float widthWithWheels;
    [UdonSynced(UdonSyncMode.None)] float length;
    [UdonSynced(UdonSyncMode.None)] Vector2 centerOfMassPositionRelativeToCenterBottom;
    [UdonSynced(UdonSyncMode.None)] Vector2 driverStationPositionRelativeToCenterBottom;

    //Wheels
    [UdonSynced(UdonSyncMode.None)] int numberOfWheels; //Divisible by 2, min = 4
    [UdonSynced(UdonSyncMode.None)] float wheelRadius;
    [UdonSynced(UdonSyncMode.None)] float motorTorquePerDrivenWheel;
    [UdonSynced(UdonSyncMode.None)] float breakTorquePerWheel;
    [UdonSynced(UdonSyncMode.None)] bool[] drivenWheels; //Lenght = numberOfWheels / 2
    [UdonSynced(UdonSyncMode.None)] float[] steeringAngleDeg; //Lenght = numberOfWheels / 2

    public void Setup(VehicleController linkedController)
    {
        this.linkedController = linkedController;

        drivenWheels = new bool[0];
        steeringAngleDeg = new float[0];

        wheelColliders = new WheelCollider[0];

        DriverStaion.linkedVehicle = linkedController;
    }

    public void SetInitialParameters()
    {
        mass = 1000;
        widthWithWheels = 2;
        length = 2;
        centerOfMassPositionRelativeToCenterBottom = 0.5f * Vector2.up;
        driverStationPositionRelativeToCenterBottom = 0.5f * Vector2.up;

        numberOfWheels = 6;
        wheelRadius = 0.5f;
        motorTorquePerDrivenWheel = 200;
        breakTorquePerWheel = 500;
        drivenWheels = new bool[] { true, true, true };
        steeringAngleDeg = new float[] { 10, 0, -10 };
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

        //Verify driven wheel array
        if (drivenWheels.Length != numberOfWheels / 2)
        {
            bool[] newDrivenWheelsArray = new bool[numberOfWheels / 2];

            for (int i = 0; i < newDrivenWheelsArray.Length; i++)
            {
                newDrivenWheelsArray[i] = drivenWheels.Length > i ? drivenWheels[i] : false;
            }

            drivenWheels = newDrivenWheelsArray;
        }

        //Verify steering angle array
        if (steeringAngleDeg.Length != numberOfWheels / 2)
        {
            float[] newsteeringAngleArray = new float[numberOfWheels / 2];

            for (int i = 0; i < newsteeringAngleArray.Length; i++)
            {
                newsteeringAngleArray[i] = steeringAngleDeg.Length > i ? steeringAngleDeg[i] : 0;
            }

            steeringAngleDeg = newsteeringAngleArray;
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
            drivenWheels: drivenWheels,
            steeringAngleDeg: steeringAngleDeg,
            wheelColliders: wheelColliders,
            wheelMeshes: wheelMeshes);
    }
    
}
