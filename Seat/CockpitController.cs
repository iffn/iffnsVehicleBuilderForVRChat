using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class CockpitController : UdonSharpBehaviour
    {
        /*
            Tasks of this component:
            - Get inputs from controls
            - Apply visualizations
        */

        [Header("Settings")]
        [SerializeField] float maxSteeringAnlgeDeg = 45;

        [Header("Unity assingments")]
        [SerializeField] MeshRenderer FlagRenderer;
        [SerializeField] WheeledVehicleSeatController linkedDriverStation;
        [SerializeField] DriveDirectionInteractor LinkedDriveDirectionInteractor;
        [SerializeField] VRSteeringWheel LinkedVRSteeringWheel;
        [SerializeField] VRBreakHolder LinkedVRBreakHolder;
        [SerializeField] MapDisplay LinkedMapDisplay;
        [SerializeField] Transform LinkedSteeringWheelVisualizer;
        [SerializeField] TMPro.TextMeshProUGUI speedIndicator;

        public bool CheckAssignments()
        {
            bool failed = false;

            if (linkedDriverStation == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(linkedDriverStation)} not assigned");
                failed = true;
            }

            if (LinkedDriveDirectionInteractor == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(LinkedDriveDirectionInteractor)} not assigned");
                failed = true;
            }

            if (LinkedVRSteeringWheel == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(LinkedVRSteeringWheel)} not assigned");
                failed = true;
            }

            if (LinkedVRBreakHolder == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(LinkedVRBreakHolder)} not assigned");
                failed = true;
            }

            if (LinkedMapDisplay == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(LinkedMapDisplay)} not assigned");
                failed = true;
            }

            if (LinkedSteeringWheelVisualizer == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(LinkedSteeringWheelVisualizer)} not assigned");
                failed = true;
            }

            if (speedIndicator == null)
            {
                Debug.LogWarning($"Error during setup of {gameObject.name}: {nameof(speedIndicator)} not assigned");
                failed = true;
            }

            return failed;
        }

        public void SetFlagMaterial(Material material)
        {
            FlagRenderer.material = material;
        }

        bool active = false;
        bool IsUserInVR;

        float driveInput;
        float breakingInput;
        float steeringInput;

        public float DriveInptut
        {
            get
            {
                return driveInput;
            }
        }

        public float BreakingInput
        {
            get
            {
                return breakingInput;
            }
        }

        public float SteeringInptut
        {
            get
            {
                return steeringInput;
            }
        }

        Vector2 lookInput;
        Vector2 moveInput;

        public void ResetControls()
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;

            driveInput = 0;
            breakingInput = 1;
            steeringInput = 0;
        }

        public bool Active
        {
            set
            {
                enabled = value;

                LinkedDriveDirectionInteractor.ColliderState = value;

                LinkedMapDisplay.gameObject.SetActive(value);
            }
        }

        public void Setup(bool active)
        {

            IsUserInVR = Networking.LocalPlayer.IsUserInVR();

            LinkedDriveDirectionInteractor.Setup();

            LinkedVRBreakHolder.Setup();

            Active = active;

            LinkedVRSteeringWheel.Setup(maxSteeringAnlgeDeg);
            LinkedMapDisplay.gameObject.SetActive(false);

            LinkedVRBreakHolder.gameObject.SetActive(Networking.LocalPlayer.IsUserInVR());
        }

        public void UpdateComponent(float speed)
        {
            GetControlInputs();

            //Visualize control inputs
            if (!IsUserInVR)
            {
                LinkedDriveDirectionInteractor.ForwardDrive = driveInput > 0;
            }

            LinkedSteeringWheelVisualizer.localRotation = Quaternion.Euler(0, 0, steeringInput * maxSteeringAnlgeDeg);

            speedIndicator.text = speed.ToString("F2") + " m/s";
        }

        void GetControlInputs()
        {
            driveInput = 0;
            breakingInput = 0;
            steeringInput = 0;

            if (IsUserInVR)
            {
                ApplyVRControls();
            }
            else
            {
                ApplyDesktopControls();
            }
        }

        void ApplyDesktopControls()
        {
            driveInput += Input.GetAxis("Vertical");
            steeringInput -= Input.GetAxis("Horizontal");
            
            if (Input.GetKey(KeyCode.Space))
            {
                breakingInput = 1;
            }
        }

        void ApplyVRControls()
        {
            steeringInput = LinkedVRSteeringWheel.SteeringInput;

            //Check hand: Return if not held, otherwise get drive and brake inputs
            switch (LinkedVRSteeringWheel.currentPickupHand)
            {
                case VRC_Pickup.PickupHand.None:
                    if (moveInput.magnitude > 0.3f)
                    {
                        Debug.Log("Exiting vehicle by walking");

                        linkedDriverStation.ForceExit();
                    }
                    return;
                case VRC_Pickup.PickupHand.Left:
                    driveInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                    if (moveInput.y > 0)
                    {
                        driveInput = Mathf.Clamp01(driveInput + moveInput.y);
                    }
                    else
                    {
                        breakingInput = Mathf.Clamp01(breakingInput - moveInput.y);
                    }
                    break;
                case VRC_Pickup.PickupHand.Right:
                    driveInput = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                    if (lookInput.y > 0)
                    {
                        driveInput = Mathf.Clamp01(driveInput + lookInput.y);
                    }
                    else
                    {
                        breakingInput = Mathf.Clamp01(breakingInput - lookInput.y);
                    }
                    break;
                default:
                    break;
            }

            breakingInput = Mathf.Clamp01(breakingInput + LinkedVRBreakHolder.BreakInput);

            if (!LinkedDriveDirectionInteractor.ForwardDrive)
            {
                driveInput *= -1;
            }
        }

        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            lookInput.y = value;
        }

        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            lookInput.x = value;
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            moveInput.y = value;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            moveInput.x = value;
        }
    }
}