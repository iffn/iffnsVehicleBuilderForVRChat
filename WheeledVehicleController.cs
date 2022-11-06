using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class WheeledVehicleController : UdonSharpBehaviour
    {
        //Unity assignments:
        [SerializeField] WheeledVehicleBuilder linkedVehicleBuilder;
        [SerializeField] WheeledVehicleStation linkedDriverStation;
        [SerializeField] WheeledVehicleSync linkedVehicleSync;
        public BuilderUIController LinkedUI; //For updating vehicle during sync;

        public Rigidbody LinkedRigidbody { get; private set; }

        //Build parameters and parts for faster access. Set and maintained by vehicle builder.
        int numberOfWheels;
        float motorTorquePerDrivenWheel;
        float breakTorquePerWheel;
        bool[] drivenWheels;
        float[] steeringAngleDeg;
        WheelCollider[] wheelColliders = new WheelCollider[0];
        Transform[] wheelMeshes = new Transform[0];

        //Runtime parameters:
        float driveInput = 0;
        float steeringInput = 0;
        float breakingInput = 1;

        public float wheelSyncAdjuster = 0.08f;
        float assumedWheelRotation = 0;

        public float forwardVelocityDebug;
        public float turnRateDebug;
        public float assumedSteeringInputDebug;

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

        public void UpdateParametersBasedOnOwnership()
        {
            //SetRigidbodyActiveBasedOnParameters
            LinkedRigidbody.isKinematic = !linkedVehicleSync.VehicleIsOwned;
        }

        public void EnteredDriverSeat()
        {
            if (!linkedVehicleSync.VehicleIsOwned)
            {
                MakeLocalPlayerOwner();
            }

            beingDrivenLocally = true;
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

        public float[] GetWheelColliderHeight()
        {
            float[] returnValue = new float[WheeledVehicleBuilder.maxWheels];

            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].GetWorldPose(out Vector3 pos, out Quaternion quat);

                returnValue[i] = wheelColliders[i].transform.InverseTransformPoint(pos).y;
            }

            return returnValue;
        }

        //Vehicle parameters
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
            //Error checks:
            bool failed = false;

            if (linkedVehicleBuilder == null)
            {
                Debug.LogWarning($"Error during setup of {{gameObject.name}}: {nameof(linkedVehicleBuilder)} not assigned");
                failed = true;
            }
            if (linkedDriverStation == null) {
                Debug.LogWarning($"Error during setup of {{gameObject.name}}: {nameof(linkedDriverStation)} not assigned"); 
                failed = true;
            }
            if (linkedVehicleSync == null) {
                Debug.LogWarning($"Error during setup of {{gameObject.name}}: {nameof(linkedVehicleSync)} not assigned"); 
                failed = true;
            }
            if (LinkedUI == null) {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(LinkedUI)} not assigned"); 
                failed = true;
            }

            if (failed)
            {
                gameObject.SetActive(false);
                return;
            }

            //Setup

            LinkedRigidbody = transform.GetComponent<Rigidbody>();


            UpdateWheelMeshPositionWhenOwner();

            //Setup scripts
            linkedDriverStation.Setup();
            linkedVehicleSync.Setup(linkedVehicle: this);
            linkedVehicleBuilder.Setup(linkedController: this);
            LinkedUI.Setup(linkedVehicle: this);

            //Setup builder

            if (Networking.IsMaster)
            {
                linkedVehicleBuilder.SetInitialParameters();
            }

            linkedVehicleBuilder.BuildVehicleBasedOnBuildParameters();

            UpdateParametersBasedOnOwnership();
        }

        void UpdateWheelMeshPositionWhenOwner()
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);

                wheelMeshes[i].SetPositionAndRotation(position, rotation);
            }
        }

        void UpdateWheelMeshPositionWhenNotOwned()
        {
            //float turnRate = transform.InverseTransformDirection(LinkedRigidbody.angularVelocity).y;
            float turnRate = linkedVehicleSync.GetCaluclatedTurnRateIfSynced;

            float forwardVelocity = transform.InverseTransformDirection(LinkedRigidbody.velocity).z;

            float velocityDirection = forwardVelocity > 0 ? 1 : -1;

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
                wheelMeshes[i].localPosition = Vector3.up * LinkedVehicleSync.verticalWheelPositions[i];
            }
        }

        private void Update()
        {
            if (linkedVehicleSync.VehicleIsOwned)
            {
                if (BeingDrivenLocally)
                {
                    Control();
                }

                UpdateWheelMeshPositionWhenOwner();

                Drive();

                linkedVehicleSync.SyncLocationFromMe();
            }
            else
            {
                linkedVehicleSync.SyncLocationPositionToMe();

                UpdateWheelMeshPositionWhenNotOwned();
            }
        }

        public void ClaimOwnership()
        {
            Debug.LogWarning("Trying to claim  ownership");

            if (linkedVehicleSync.VehicleIsOwned)
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

        void MakeLocalPlayerOwner()
        {
            linkedVehicleSync.MakeLocalPlayerOwner();
            linkedVehicleBuilder.MakeLocalPlayerOwner();

        }
    }
}