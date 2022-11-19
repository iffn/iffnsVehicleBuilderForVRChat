using Newtonsoft.Json.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

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
        [SerializeField] VRSteeringWheel LinkedVRSteeringWheelControlls;
        [SerializeField] Transform LinkedSteeringWheelVisualizer;

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

        Vector2 rightJoystickInput;
        Vector2 leftJoystickInput;

        //Runtime parameters:
        float driveInput = 0;
        float steeringInput = 0;
        float brakingInput = 1;

        public float wheelSyncAdjuster = 0.08f;
        float assumedWheelRotation = 0;

        public float forwardVelocityDebug;
        public float turnRateDebug;
        public float assumedSteeringInputDebug;

        bool vehicleFixed = false;
        public bool VehicleFixed
        {
            get
            {
                return vehicleFixed;
            }
            set
            {
                foreach(WheelCollider wheel in wheelColliders)
                {
                    wheel.enabled = !value;
                }

                /*
                foreach(Transform wheelMesh in wheelMeshes)
                {
                    wheelMesh.localPosition = Vector3.zero;
                    wheelMesh.localRotation = Quaternion.identity;
                }
                */

                transform.GetComponent<BoxCollider>().enabled = !value;

                LinkedRigidbody.isKinematic = value;
                LinkedRigidbody.velocity = Vector3.zero;
                LinkedRigidbody.angularVelocity = Vector3.zero;

                linkedDriverStation.EnableCollider = !value;

                vehicleFixed = value;
            }
        }

        Vector3 originalLocalPosition;
        Quaternion originalLocalRotation;

        public void RespawnVehicle()
        {
            LinkedRigidbody.velocity = Vector3.zero;
            LinkedRigidbody.angularVelocity = Vector3.zero;

            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
        }

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

            if (Networking.LocalPlayer.IsUserInVR())
            {
                LinkedVRSteeringWheelControlls.gameObject.SetActive(true);
            }

            LinkedVRSteeringWheelControlls.SetSteeringWheelPositionRelativeToPlayer();
        }

        public void ExitedDriverSeat()
        {
            BeingDrivenLocally = false;

            if (Networking.LocalPlayer.IsUserInVR())
            {
                LinkedVRSteeringWheelControlls.gameObject.SetActive(false);

                LinkedVRSteeringWheelControlls.DropIfHeld();
            }
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

                wheelColliders[i].brakeTorque = breakTorquePerWheel * brakingInput;
            }
        }

        void ResetInputs()
        {
            driveInput = 0;
            steeringInput = 0;
            brakingInput = 1;
        }

        void Control()
        {
            driveInput = 0;
            brakingInput = 0;
            steeringInput = 0;

            if (Networking.LocalPlayer.IsUserInVR())
            {
                applyVRControls();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                LinkedRigidbody.constraints = RigidbodyConstraints.None;
            }


            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                driveInput++;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                driveInput--;
            }

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
                brakingInput = 1;
            }

            LinkedSteeringWheelVisualizer.localRotation = Quaternion.Euler(0, 0, steeringInput * Mathf.Rad2Deg);
        }

        void applyVRControls()
        {
            //Check hand: Return if not held, otherwise get drive and brake inputs
            switch (LinkedVRSteeringWheelControlls.currentHand)
            {
                case VRC_Pickup.PickupHand.None:
                    return;
                    break;
                case VRC_Pickup.PickupHand.Left:
                    driveInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                    brakingInput = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                    if(rightJoystickInput.y > 0)
                    {
                        driveInput = Mathf.Clamp01(driveInput + rightJoystickInput.y);
                    }
                    else
                    {
                        brakingInput = Mathf.Clamp01(brakingInput - rightJoystickInput.y);
                    }
                    break;
                case VRC_Pickup.PickupHand.Right:
                    driveInput = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                    brakingInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                    if (leftJoystickInput.y > 0)
                    {
                        driveInput = Mathf.Clamp01(driveInput + leftJoystickInput.y);
                    }
                    else
                    {
                        brakingInput = Mathf.Clamp01(brakingInput - leftJoystickInput.y);
                    }
                    break;
                default:
                    break;
            }

            LinkedVRSteeringWheelControlls.UpdateControlls();
            steeringInput = LinkedVRSteeringWheelControlls.SteeringAngle;
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
            LinkedVRSteeringWheelControlls.gameObject.SetActive(Networking.LocalPlayer.IsUserInVR());

            if (Networking.LocalPlayer.IsUserInVR())
            {
                LinkedVRSteeringWheelControlls.Setup();
            }

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

                LinkedUI.UpdateUIFromVehicle();
            }

            linkedVehicleBuilder.BuildVehicleBasedOnBuildParameters();

            UpdateParametersBasedOnOwnership();

            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
        }

        void UpdateWheelMeshPositionWhenOwner()
        {
            if (!vehicleFixed)
            {
                for (int i = 0; i < wheelColliders.Length; i++)
                {
                    wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);

                    wheelMeshes[i].SetPositionAndRotation(position, rotation);
                }
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

        //index right hand joystck
        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            rightJoystickInput.y = value;
        }

        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            rightJoystickInput.x = value;
        }

        //index left hand joystck
        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            leftJoystickInput.y = value;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            leftJoystickInput.x = value;
        }
    }
}