
using System.Linq.Expressions;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class WheeledVehicleController : UdonSharpBehaviour
{
    //Unity assignments:
    
    [SerializeField] WheeledVehicleBuilder LinkedBuilder;

    //Runtime parameters:

    float driveInput = 0;
    float steeringInput = 0;
    float breakingInput = 1;

    bool active = false;

    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            active = value;

            if (!value)
            {
                ResetInputs();
            }
        }
    }

    Transform[] wheelMeshes;

    public VehicleStates CurrentVehicleState { get; private set; } = VehicleStates.inactive;

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

    void ResetInputs()
    {
        driveInput = 0;
        steeringInput = 0;
        breakingInput = 1;
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LinkedRigidbody.constraints = RigidbodyConstraints.None;
        }

        driveInput = 0;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            driveInput++;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            driveInput--;
        }

        steeringInput = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            steeringInput++;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
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

            wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);

            wheelMeshes[i].SetPositionAndRotation(position, rotation);
        }
    }

    private void Update()
    {
        if (Active)
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
