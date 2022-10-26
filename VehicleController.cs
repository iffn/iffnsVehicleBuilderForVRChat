
using System.Linq.Expressions;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class VehicleController : UdonSharpBehaviour
{
    //Unity assignments:
    
    [SerializeField] VehicleBuilder LinkedBuilder;

    //Runtime parameters:

    float driveInput = 0;
    float steeringInput = 0;
    float breakingInput = 0;

    public bool active = false;
    Transform[] wheelMeshes;

    public VehicleStates currentVehicleState { get; private set; } = VehicleStates.inactive;

    //-------------------

    public Rigidbody LinkedRigidbody { get; private set; }

    //Parts
    WheelCollider[] wheelColliders;

    //Vehicle parameters

    int numberOfWheels;
    float motorTorquePerDrivenWheel;
    float breakTorquePerWheel;
    bool[] drivenWheels;
    float[] steeringAngleDeg;

    void Setup()
    {
        LinkedRigidbody = transform.GetComponent<Rigidbody>();
    }

    public void SetBuildParameters(
        int numberOfWheels,
        float motorTorquePerDrivenWheel,
        float breakTorquePerWheel,
        bool[] drivenWheels,
        float[] steeringAngleDeg,
        WheelCollider[] wheelColliders,
        Transform[] wheelMeshes
        )
        
    { 
        this.numberOfWheels = numberOfWheels;
        this.motorTorquePerDrivenWheel = motorTorquePerDrivenWheel;
        this.breakTorquePerWheel = breakTorquePerWheel;
        this.drivenWheels = drivenWheels;
        this.steeringAngleDeg = steeringAngleDeg;
        this.wheelColliders = wheelColliders;
        this.wheelMeshes = wheelMeshes;
    }

    void Drive()
    {
        for (int i = 0; i < numberOfWheels; i++)
        {
            int symetricArrayIndex = i / 2;

            if (drivenWheels[symetricArrayIndex])
            {
                wheelColliders[i].motorTorque = motorTorquePerDrivenWheel * driveInput;
            }

            wheelColliders[i].steerAngle = steeringAngleDeg[symetricArrayIndex] * steeringInput;

            wheelColliders[i].brakeTorque = breakTorquePerWheel * breakingInput;
        }
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LinkedRigidbody.constraints = RigidbodyConstraints.None;
        }

        driveInput = 0;

        if (Input.GetKey(KeyCode.W))
        {
            driveInput++;
        }
        if (Input.GetKey(KeyCode.S))
        {
            driveInput--;
        }

        steeringInput = 0;

        if (Input.GetKey(KeyCode.A))
        {
            steeringInput++;
        }
        if (Input.GetKey(KeyCode.D))
        {
            steeringInput--;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            breakingInput = 1;
        }
        else
        {
            breakingInput = 0;
        }
    }

    void Start()
    {
        Setup();

        //Setup builder
        LinkedBuilder.Setup(linkedController: this);

        if (Networking.IsMaster)
        {
            LinkedBuilder.SetInitialParameters();
        }

        LinkedBuilder.BuildVehiclesBasedOnBuildParameters();
    }

    void UpdateWheelMeshPosition()
    {
        for(int i = 0; i < wheelColliders.Length; i++)
        {
            Vector3 position;
            Quaternion rotation;

            wheelColliders[i].GetWorldPose(out position, out rotation);

            wheelMeshes[i].SetPositionAndRotation(position, rotation);
        }
    }

    private void Update()
    {
        if (active)
        {
            Control();
        }

        UpdateWheelMeshPosition();

        Drive();
    }
}

public enum VehicleStates
{
    inactive,
    ownedLocally,
    synced
}
