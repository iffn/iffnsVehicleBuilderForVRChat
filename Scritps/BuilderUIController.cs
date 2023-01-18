using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class BuilderUIController : UdonSharpBehaviour
    {
        WheeledVehicleController linkedVehicle;
        WheeledVehicleBuilder linkedVehicleBuilder;

        //Vehicle
        [SerializeField] FloatInputLineController MassInput;
        [SerializeField] FloatInputLineController WidthWithWheelsInput;
        [SerializeField] FloatInputLineController LengthInput;
        [SerializeField] FloatInputLineController GroundClearanceInput;
        [SerializeField] InputField[] CenterOfMassXYZInputFields;
        [SerializeField] InputField NumberOfSeatRowsInputField;
        [SerializeField] Toggle[] SeatsMirroredToggle;

        //Wheels
        [SerializeField] InputField NumberOfWheelsInputField;
        [SerializeField] FloatInputLineController WheelRadiusInput;
        [SerializeField] FloatInputLineController WheelWidthInput;
        [SerializeField] Toggle[] DrivenWheelToggle;
        [SerializeField] FloatInputLineController MotorTorqueInput;
        [SerializeField] FloatInputLineController BreakTorqueInput;
        [SerializeField] InputField[] SteeringAngleInputField;

        [SerializeField] Button ClaimOwnershipButton;
        [SerializeField] UnityEngine.UI.Text CurrentOwnerName;

        [SerializeField] Button RespawnButton;

        [SerializeField] Toggle FixVehicleToggle;

        bool editInProgress = false;
        bool skipUICalls = false;
        bool seriousMode = true;

        private void Update()
        {

        }

        public void RespawnVehicle()
        {
            linkedVehicle.RespawnVehicle();
        }

        public void ToggleFixVehicle()
        {
            linkedVehicle.VehicleFixed = FixVehicleToggle.isOn;
        }

        public void UpdateInputArrays()
        {
            for (int i = 0; i < WheeledVehicleBuilder.maxWheels / 2; i++)
            {
                bool active = i < linkedVehicleBuilder.numberOfWheels / 2;

                DrivenWheelToggle[i].gameObject.SetActive(active);
                SteeringAngleInputField[i].gameObject.SetActive(active);
            }
        }

        public void UpdateVehicleFromUI()
        {
            if (skipUICalls) return;

            if (editInProgress)
            {
                Debug.LogWarning("Edit in progress");
                return;
            }

            editInProgress = true;

            if (!Networking.IsOwner(linkedVehicleBuilder.gameObject))
            {
                Debug.LogWarning("UpdateVehicleFromUI for non owner");

                UpdateUIFromVehicle();
                editInProgress = false;
                return;
            }

            float currentFloat;
            int currentInt;
            float x, y, z;

            //Vehicle
            linkedVehicleBuilder.mass = MassInput.Value;

            linkedVehicleBuilder.widthWithWheels = WidthWithWheelsInput.Value;

            linkedVehicleBuilder.length = LengthInput.Value;

            linkedVehicleBuilder.groundClearance = GroundClearanceInput.Value;

            if (float.TryParse(CenterOfMassXYZInputFields[0].text, out x)
                && float.TryParse(CenterOfMassXYZInputFields[1].text, out y)
                && float.TryParse(CenterOfMassXYZInputFields[2].text, out z))

            {
                linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom = new Vector3(x, y, z);
            }

            if (int.TryParse(NumberOfSeatRowsInputField.text, out currentInt))
            {
                currentInt = Mathf.Clamp(currentInt, 1, WheeledVehicleBuilder.maxSeatRows);

                linkedVehicleBuilder.numberOfSeatRows = currentInt;
            }

            for (int i = 0; i < SeatsMirroredToggle.Length; i++)
            {
                linkedVehicleBuilder.seatsMirrored[i] = SeatsMirroredToggle[i].isOn;
            }

            //Wheels
            if (int.TryParse(NumberOfWheelsInputField.text, out currentInt))
            {
                //currentInt = (currentInt % 2 != 0) ? currentInt -= 1 : currentInt;

                currentInt = currentInt / 2 * 2; //Make even
                currentInt = Mathf.Clamp(currentInt, WheeledVehicleBuilder.minWheels, WheeledVehicleBuilder.maxWheels);

                NumberOfWheelsInputField.text = currentInt.ToString(); //Doesn't invoke delegates

                linkedVehicleBuilder.numberOfWheels = currentInt;
            }

            linkedVehicleBuilder.wheelRadius = WheelRadiusInput.Value;

            linkedVehicleBuilder.wheelWidth = WheelWidthInput.Value;

            for (int i = 0; i < linkedVehicleBuilder.drivenWheelPairs.Length; i++)
            {
                linkedVehicleBuilder.drivenWheelPairs[i] = DrivenWheelToggle[i].isOn;
            }

            linkedVehicleBuilder.motorTorquePerDrivenWheel = MotorTorqueInput.Value;
            linkedVehicleBuilder.breakTorquePerWheel = BreakTorqueInput.Value;

            for (int i = 0; i < linkedVehicleBuilder.steeringAngleDeg.Length; i++)
            {
                if (float.TryParse(SteeringAngleInputField[i].text, out currentFloat))
                {
                    linkedVehicleBuilder.steeringAngleDeg[i] = currentFloat;
                }
            }

            linkedVehicleBuilder.BuildVehicleBasedOnBuildParameters();

            ToggleArrayElementsDependingOnInputs();

            editInProgress = false;
        }

        public void UpdateUIFromVehicle()
        {
            skipUICalls = true;

            //Vehicle
            MassInput.Value = linkedVehicleBuilder.mass;
            WidthWithWheelsInput.Value = linkedVehicleBuilder.widthWithWheels;
            //widthWithWheelsInputField.text = linkedVehicleBuilder.widthWithWheels.ToString();
            LengthInput.Value = linkedVehicleBuilder.length;
            GroundClearanceInput.Value = linkedVehicleBuilder.groundClearance;
            CenterOfMassXYZInputFields[0].text = linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.x.ToString();
            CenterOfMassXYZInputFields[1].text = linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.y.ToString();
            CenterOfMassXYZInputFields[2].text = linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.z.ToString();
            NumberOfSeatRowsInputField.text = linkedVehicleBuilder.numberOfSeatRows.ToString();
            
            for(int i = 0; i<WheeledVehicleBuilder.maxSeatRows; i++)
            {
                SeatsMirroredToggle[i].isOn = linkedVehicleBuilder.seatsMirrored[i];
            }

            //Wheels
            NumberOfWheelsInputField.text = linkedVehicleBuilder.numberOfWheels.ToString();
            WheelRadiusInput.Value = linkedVehicleBuilder.wheelRadius;
            WheelWidthInput.Value = linkedVehicleBuilder.wheelWidth;

            for (int i = 0; i < WheeledVehicleBuilder.maxWheels / 2; i++)
            {
                DrivenWheelToggle[i].isOn = linkedVehicleBuilder.drivenWheelPairs[i];
            }

            MotorTorqueInput.Value = linkedVehicleBuilder.motorTorquePerDrivenWheel;
            BreakTorqueInput.Value = linkedVehicleBuilder.breakTorquePerWheel;

            for (int i = 0; i < WheeledVehicleBuilder.maxWheels / 2; i++)
            {
                SteeringAngleInputField[i].text = linkedVehicleBuilder.steeringAngleDeg[i].ToString();
            }

            ToggleArrayElementsDependingOnInputs();

            skipUICalls = false;
        }

        void ToggleArrayElementsDependingOnInputs()
        {
            for(int i = 0; i<WheeledVehicleBuilder.maxSeatRows; i++)
            {
                SeatsMirroredToggle[i].gameObject.SetActive(i < linkedVehicleBuilder.numberOfSeatRows);
            }

            for (int i = 0; i < WheeledVehicleBuilder.maxWheels / 2; i++)
            {
                bool isActive = i < linkedVehicleBuilder.numberOfWheels;

                DrivenWheelToggle[i].gameObject.SetActive(isActive);
                SteeringAngleInputField[i].gameObject.SetActive(isActive);
            }
        }

        public void SetVehicleOwnerDisplay(VRCPlayerApi owner)
        {
            bool locallyOwned = owner.isLocal;

            ClaimOwnershipButton.gameObject.SetActive(!locallyOwned);
            RespawnButton.gameObject.SetActive(locallyOwned);

            CurrentOwnerName.text = owner.playerId + ": " + owner.displayName;
        }

        public void ClaimOwnership()
        {
            linkedVehicle.ClaimOwnership();
        }

        public void Setup(WheeledVehicleController linkedVehicle)
        {
            this.linkedVehicle = linkedVehicle;
            linkedVehicleBuilder = linkedVehicle.LinkedVehicleBuilder;

            MassInput.Setup(this, 800, 6000, 1000, seriousMode);
            WidthWithWheelsInput.Setup(this, 1, 4, 3, seriousMode);
            LengthInput.Setup(this, 1, 6, 4, seriousMode);
            GroundClearanceInput.Setup(this, 0.05f, 2f, 0.2f, seriousMode);
            WheelRadiusInput.Setup(this, 0.1f, 2f, 1, seriousMode);
            WheelWidthInput.Setup(this, 0.1f, 1, 0.2f, seriousMode);
            MotorTorqueInput.Setup(this, 100, 3000, 400, seriousMode);
            BreakTorqueInput.Setup(this, 100, 3000, 500, seriousMode);

            SetVehicleOwnerDisplay(Networking.GetOwner(this.linkedVehicle.LinkedVehicleSync.gameObject));
        }

        void Start()
        {
            //Use Setup started by Vehicle instead
        }
    }
}