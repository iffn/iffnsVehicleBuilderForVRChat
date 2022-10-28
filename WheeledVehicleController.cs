
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
    [SerializeField] WheeledVehicleBuilder linkedVehicleBuilder;
    [SerializeField] WheeledVehicleStation linkedDriverStation;
    [SerializeField] WheeledVehicleSync linkedVehicleSync;

    public WheeledVehicleBuilder LinkedVehicleBuilder
    {
        get
        {
            return linkedVehicleBuilder;
        }
    }

    public WheeledVehicleSync LinkedVehicleSync
    {
        get
        {
            return linkedVehicleSync;
        }
    }

    //Runtime parameters:
    float driveInput = 0;
    float steeringInput = 0;
    float breakingInput = 1;

    bool vehcileIsOwned;
    public bool VehicleIsOwned
    {
        get
        {
            return vehcileIsOwned;
        }
        set
        {
            vehcileIsOwned = value;
            SetRigidbodyActiveBasedOnParameters();
        }
    }



    void SetRigidbodyActiveBasedOnParameters()
    {
        LinkedRigidbody.isKinematic = !vehcileIsOwned;
    }

    public void EnteredDriverSeat()
    {
        if (vehcileIsOwned)
        {
            beingDrivenLocally = true;
        }
    }

    public void ExitedDriverSeat()
    {
        BeingDrivenLocally = false;
    }

    bool beingDrivenLocally = false;
    public bool BeingDrivenLocally
    {
        get
        {
            return beingDrivenLocally;
        }
        set
        {
            beingDrivenLocally = value;

            if (!value)
            {
                ResetInputs();
            }
        }
    }

    Transform[] wheelMeshes;

    //public VehicleStates CurrentVehicleState { get; private set; } = VehicleStates.inactive;

    //-------------------

    public Rigidbody LinkedRigidbody { get; private set; }

    //Parts
    WheelCollider[] wheelColliders = new WheelCollider[0];

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

        linkedVehicleSync.Setup(this);

        //Setup builder
        linkedVehicleBuilder.Setup(linkedController: this);

        if (Networking.IsMaster)
        {
            linkedVehicleBuilder.SetInitialParameters();
        }

        linkedVehicleBuilder.BuildVehiclesBasedOnBuildParameters();

        VehicleIsOwned = Networking.IsOwner(linkedVehicleSync.gameObject);
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
        if (BeingDrivenLocally)
        {
            Control();
        }

        UpdateWheelMeshPosition();

        Drive();
    }

    public void ClaimOwnership()
    {
        if (Networking.IsOwner(linkedVehicleSync.gameObject)) return;
        if (linkedDriverStation.SeatedPlayer != null) return;

        Networking.SetOwner(Networking.LocalPlayer, linkedVehicleSync.gameObject);
        Networking.SetOwner(Networking.LocalPlayer, linkedVehicleBuilder.gameObject);

        if (linkedDriverStation.SeatedPlayer.isLocal)
        {
            beingDrivenLocally = true;
        }
    }
}

/*
public enum VehicleStates
{
    inactive,
    beingDrivenLocally,
    synced
}
*/
