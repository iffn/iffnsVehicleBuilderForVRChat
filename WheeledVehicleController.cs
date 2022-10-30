
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

    bool vehcileIsOwned = true;
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
        if (!vehcileIsOwned)
        {
            MakeLocalPlayerOwner();
        }

        beingDrivenLocally = true;
    }

    public void MakeLocalPlayerOwner()
    {
        linkedVehicleSync.MakeLocalPlayerOwner();
        linkedVehicleBuilder.MakeLocalPlayerOwner();
        
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

    public float[] GetWheelColliderHeight()
    {
        float[] returnValue = new float[12];

        for(int i = 0; i<wheelColliders.Length; i++)
        {
            wheelColliders[i].GetWorldPose(out Vector3 pos, out Quaternion quat);

            returnValue[i] = wheelColliders[i].transform.InverseTransformPoint(pos).y;
        }

        return returnValue;
    }

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

        UpdateWheelMeshPositionWhenOwner();

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

    void UpdateWheelMeshPositionWhenOwner()
    {
        for(int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);

            wheelMeshes[i].SetPositionAndRotation(position, rotation);
        }
    }

    //Wheel sync
    public float wheelSyncAdjuster = 0.08f;
    float assumedWheelRotation = 0;

    public float forwardVelocityDebug;
    public float turnRateDebug;
    public float assumedSteeringInputDebug;

    void UpdateWheelMeshPositionWhenNotOwned()
    {
        //float turnRate = transform.InverseTransformDirection(LinkedRigidbody.angularVelocity).y;
        float turnRate = linkedVehicleSync.GetCaluclatedTurnRateIfSynced;

        float forwardVelocity = transform.InverseTransformDirection(LinkedRigidbody.velocity).z;

        float velocityDirection = (forwardVelocity > 0) ? 1 : -1;

        float assumedSteeringInput = Mathf.Clamp(turnRate * Mathf.Rad2Deg * wheelSyncAdjuster * velocityDirection, -1, 1);
        //Debug.Log(numberOfWheels);

        assumedWheelRotation += forwardVelocity / linkedVehicleBuilder.wheelRadius * Time.deltaTime;

        turnRateDebug = turnRate;
        forwardVelocityDebug = forwardVelocity;
        assumedSteeringInputDebug = assumedSteeringInput;

        for (int i = 0; i < numberOfWheels; i++)
        {
            int symetricArrayIndex = i / 2;

            float steerAngle = -steeringAngleDeg[symetricArrayIndex] * assumedSteeringInput;

            //wheelMeshes[i].rotation = transform.rotation * Quaternion.Euler(new Vector3(assumedWheelRotation, steerAngle, 0));
            wheelMeshes[i].rotation = transform.rotation * Quaternion.Euler(new Vector3(0, steerAngle, 0));
            wheelMeshes[i].localPosition = Vector3.up * LinkedVehicleSync.verticalWheelPosition[i];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            Debug.Log($"{nameof(vehcileIsOwned)} = {vehcileIsOwned}");
            Debug.Log($"{nameof(BeingDrivenLocally)} = {BeingDrivenLocally}");
        }

        if (VehicleIsOwned)
        {
            if (BeingDrivenLocally)
            {
                Control();
            }

            UpdateWheelMeshPositionWhenOwner();

            Drive();
        }
        else
        {
            UpdateWheelMeshPositionWhenNotOwned();
        }
    }

    public void ClaimOwnership()
    {
        Debug.LogWarning("Trying to claim  ownership");

        if (Networking.IsOwner(linkedVehicleSync.gameObject))
        {
            Debug.LogWarning("   Already the vehicle owner");
            return;
        }
        if (linkedDriverStation.StationOccupant == StationOccupantTypes.someoneElse)
        {
            Debug.LogWarning("   Someone else is sitting in the vehicle");
            return;
        }

        Debug.LogWarning("   Making the local player the owner");

        MakeLocalPlayerOwner();

        if (linkedDriverStation.StationOccupant == StationOccupantTypes.me)
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
