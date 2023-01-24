using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class BuilderUIController : UdonSharpBehaviour
    {
        
        [Header("Unity assingments")]
        WheeledVehicleBuilder linkedVehicleBuilder;
        WheeledVehicleController linkedVehicle;

        //Vehicle
        [SerializeField] FloatInputLineController MassInput;
        [SerializeField] FloatInputLineController WidthWithWheelsInput;
        [SerializeField] FloatInputLineController LengthInput;
        [SerializeField] FloatInputLineController GroundClearanceInput;
        [SerializeField] InputField[] CenterOfMassXYZInputFields;
        [SerializeField] InputField NumberOfSeatRowsInputField;
        [SerializeField] FloatInputLineController SeatLengthRatioInput;
        [SerializeField] FloatInputLineController SeatWidthRatioInput;
        [SerializeField] Toggle[] SeatsMirroredToggle;

        //Wheels
        [SerializeField] InputField NumberOfWheelsInputField;
        [SerializeField] FloatInputLineController WheelRadiusInput;
        [SerializeField] FloatInputLineController WheelWidthInput;
        [SerializeField] Toggle[] DrivenWheelToggle;
        [SerializeField] FloatInputLineController MotorTorqueInput;
        [SerializeField] FloatInputLineController BreakTorqueInput;
        [SerializeField] InputField[] SteeringAngleInputField;

        //[SerializeField] Button ClaimOwnershipButton;
        [SerializeField] UnityEngine.UI.Text CurrentOwnerName;

        [SerializeField] GameObject[] OwnerObjects;
        [SerializeField] GameObject[] NonOwnerObjects;

        //[SerializeField] Button RespawnButton;

        [SerializeField] Toggle FixVehicleToggle;
        [SerializeField] Toggle SeriousModeToggle;

        FloatInputLineController[] floatInputs;

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

            linkedVehicleBuilder.seatLenghtRatio = SeatLengthRatioInput.Value;
            linkedVehicleBuilder.seatWidthRatio = SeatWidthRatioInput.Value;

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

            SeatLengthRatioInput.Value = linkedVehicleBuilder.seatLenghtRatio;
            SeatWidthRatioInput.Value = linkedVehicleBuilder.seatWidthRatio;

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
                bool isActive = i < linkedVehicleBuilder.numberOfWheels / 2;

                DrivenWheelToggle[i].gameObject.SetActive(isActive);
                SteeringAngleInputField[i].gameObject.SetActive(isActive);
            }
        }

        public void SetVehicleOwnerDisplay(VRCPlayerApi owner)
        {
            bool locallyOwned = owner.isLocal;

            foreach(GameObject o in OwnerObjects)
            {
                o.SetActive(locallyOwned);
            }

            foreach (GameObject o in NonOwnerObjects)
            {
                o.SetActive(!locallyOwned);
            }

            //ClaimOwnershipButton.gameObject.SetActive(!locallyOwned);
            //RespawnButton.gameObject.SetActive(locallyOwned);

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

            seriousMode = SeriousModeToggle.isOn;

            MassInput.Setup(this, 800, 6000, 1000, seriousMode);
            WidthWithWheelsInput.Setup(this, 1, 4, 3, seriousMode);
            LengthInput.Setup(this, 1, 6, 4, seriousMode);
            GroundClearanceInput.Setup(this, 0.05f, 1f, 0.2f, seriousMode);
            SeatLengthRatioInput.Setup(this, 0.05f, 1f, 0.8f, seriousMode);
            SeatWidthRatioInput.Setup(this, 0.05f, 1f, 0.8f, seriousMode);
            WheelRadiusInput.Setup(this, 0.1f, 1f, 0.5f, seriousMode);
            WheelWidthInput.Setup(this, 0.1f, 1, 0.2f, seriousMode);
            MotorTorqueInput.Setup(this, 100, 3000, 400, seriousMode);
            BreakTorqueInput.Setup(this, 100, 3000, 500, seriousMode);

            floatInputs = new FloatInputLineController[] {
                MassInput,
                WidthWithWheelsInput,
                LengthInput,
                GroundClearanceInput,
                SeatLengthRatioInput,
                SeatWidthRatioInput,
                WheelRadiusInput,
                WheelWidthInput,
                MotorTorqueInput,
                BreakTorqueInput
            };

            SetVehicleOwnerDisplay(Networking.GetOwner(this.linkedVehicle.LinkedVehicleSync.gameObject));
        }

        void Start()
        {
            //Use Setup started by Vehicle instead
        }

        public void SeriousModeUpdate()
        {
            seriousMode = SeriousModeToggle.isOn;

            foreach(FloatInputLineController input in floatInputs)
            {
                input.ApplyLimits = seriousMode;
            }

            if(seriousMode) UpdateVehicleFromUI();
        }

        public void SetVehiclePreset(PresetVehicleTypes types)
        {
            linkedVehicleBuilder.SetBuildParameters(types);
        }

        public void AddSeatRow()
        {
            if (linkedVehicleBuilder.numberOfSeatRows > 4) return;

            linkedVehicleBuilder.numberOfSeatRows += 1;

            linkedVehicleBuilder.BuildFromParameters();
            UpdateUIFromVehicle();
        }

        public void SubtractSeatRow()
        {
            if (linkedVehicleBuilder.numberOfSeatRows < 2) return;

            linkedVehicleBuilder.numberOfSeatRows -= 1;

            linkedVehicleBuilder.BuildFromParameters();
            UpdateUIFromVehicle();
        }

        public void AddWheelPair()
        {
            if (linkedVehicleBuilder.numberOfWheels > 10) return;

            linkedVehicleBuilder.numberOfWheels += 2;

            linkedVehicleBuilder.BuildFromParameters();
            UpdateUIFromVehicle();
        }

        public void RemoveWheelPair()
        {
            if (linkedVehicleBuilder.numberOfWheels < 6) return;

            linkedVehicleBuilder.numberOfWheels -= 2;

            linkedVehicleBuilder.BuildFromParameters();
            UpdateUIFromVehicle();
        }
    }
}