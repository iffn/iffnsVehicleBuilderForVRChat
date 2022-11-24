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

        [SerializeField] InputField MassInputField;
        [SerializeField] InputField widthWithWheelsInputField;
        [SerializeField] InputField lengthInputField;
        [SerializeField] InputField CenterOfMassXInputField;
        [SerializeField] InputField CenterOfMassYInputField;
        [SerializeField] InputField CenterOfMassZInputField;
        [SerializeField] InputField SteeringPositionXInputField;
        [SerializeField] InputField SteeringPositionYInputField;
        [SerializeField] InputField SteeringPositionZInputField;

        [SerializeField] InputField NumberOfWheelsInputField;
        [SerializeField] InputField WheelRadiusInputField;
        [SerializeField] InputField MotorTorqueInputField;
        [SerializeField] InputField BreakTorqueInputField;
        [SerializeField] Toggle[] DrivenWheelInputField;
        [SerializeField] InputField[] SteeringAngleInputField;

        [SerializeField] Button ClaimOwnershipButton;
        [SerializeField] UnityEngine.UI.Text CurrentOwnerName;

        [SerializeField] Button RespawnButton;

        [SerializeField] Toggle FixVehicleToggle;
        [SerializeField] Toggle UseCustomMeshToggle;
        [SerializeField] MeshBuilderInterface CustomMeshInterface;

        bool editInProgress = false;

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

        public void ToggleUseCustomMesh()
        {
            CustomMeshInterface.gameObject.SetActive(UseCustomMeshToggle.isOn);

            linkedVehicleBuilder.UseCustomMesh = UseCustomMeshToggle.isOn;
        }

        public void UpdateInputArrays()
        {
            for (int i = 0; i < WheeledVehicleBuilder.maxWheels / 2; i++)
            {
                bool active = i < linkedVehicleBuilder.numberOfWheels / 2;

                DrivenWheelInputField[i].gameObject.SetActive(active);
                SteeringAngleInputField[i].gameObject.SetActive(active);
            }
        }

        public void UpdateVehicleFromUI()
        {
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
            if (float.TryParse(MassInputField.text, out currentFloat))
            {
                linkedVehicleBuilder.mass = currentFloat;
            }

            if (float.TryParse(widthWithWheelsInputField.text, out currentFloat))
            {
                linkedVehicleBuilder.widthWithWheels = currentFloat;
            }

            if (float.TryParse(lengthInputField.text, out currentFloat))
            {
                linkedVehicleBuilder.length = currentFloat;
            }

            if (float.TryParse(CenterOfMassXInputField.text, out x)
                && float.TryParse(CenterOfMassYInputField.text, out y)
                && float.TryParse(CenterOfMassZInputField.text, out z))

            {
                linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom = new Vector3(x, y, z);
            }

            if (float.TryParse(SteeringPositionXInputField.text, out x)
                && float.TryParse(SteeringPositionYInputField.text, out y)
                && float.TryParse(SteeringPositionZInputField.text, out z))
            {
                linkedVehicleBuilder.driverStationPositionRelativeToCenterBottom = new Vector3(x, y, z);
            }

            //Wheels
            if (int.TryParse(NumberOfWheelsInputField.text, out currentInt))
            {
                //currentInt = (currentInt % 2 != 0) ? currentInt -= 1 : currentInt;
                currentInt = currentInt / 2 * 2; //Make even
                currentInt = currentInt < WheeledVehicleBuilder.minWheels ? WheeledVehicleBuilder.minWheels : currentInt;
                currentInt = currentInt > WheeledVehicleBuilder.maxWheels ? WheeledVehicleBuilder.maxWheels : currentInt;

                NumberOfWheelsInputField.text = currentInt.ToString(); //Doesn't invoke delegates

                linkedVehicleBuilder.numberOfWheels = currentInt;
            }

            if (float.TryParse(WheelRadiusInputField.text, out currentFloat))
            {
                linkedVehicleBuilder.wheelRadius = currentFloat;
            }

            if (float.TryParse(MotorTorqueInputField.text, out currentFloat))
            {
                linkedVehicleBuilder.motorTorquePerDrivenWheel = currentFloat;
            }

            if (float.TryParse(BreakTorqueInputField.text, out currentFloat))
            {
                linkedVehicleBuilder.breakTorquePerWheel = currentFloat;
            }

            for (int i = 0; i < linkedVehicleBuilder.numberOfWheels / 2; i++)
            {
                linkedVehicleBuilder.drivenWheelPairs[i] = DrivenWheelInputField[i].isOn;

                if (float.TryParse(SteeringAngleInputField[i].text, out currentFloat))
                {
                    linkedVehicleBuilder.steeringAngleDeg[i] = currentFloat;
                }
            }

            linkedVehicleBuilder.BuildVehicleBasedOnBuildParameters();

            editInProgress = false;
        }

        public void UpdateUIFromVehicle()
        {
            MassInputField.text = linkedVehicleBuilder.mass.ToString();
            widthWithWheelsInputField.text = linkedVehicleBuilder.widthWithWheels.ToString();
            lengthInputField.text = linkedVehicleBuilder.length.ToString();
            CenterOfMassXInputField.text = linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.x.ToString();
            CenterOfMassYInputField.text = linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.y.ToString();
            CenterOfMassZInputField.text = linkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.z.ToString();
            SteeringPositionXInputField.text = linkedVehicleBuilder.driverStationPositionRelativeToCenterBottom.x.ToString();
            SteeringPositionYInputField.text = linkedVehicleBuilder.driverStationPositionRelativeToCenterBottom.y.ToString();
            SteeringPositionZInputField.text = linkedVehicleBuilder.driverStationPositionRelativeToCenterBottom.z.ToString();

            NumberOfWheelsInputField.text = linkedVehicleBuilder.numberOfWheels.ToString();
            WheelRadiusInputField.text = linkedVehicleBuilder.wheelRadius.ToString();
            MotorTorqueInputField.text = linkedVehicleBuilder.motorTorquePerDrivenWheel.ToString();
            BreakTorqueInputField.text = linkedVehicleBuilder.breakTorquePerWheel.ToString();


            if (DrivenWheelInputField == null)
            {
                Debug.LogWarning($"{nameof(DrivenWheelInputField)} not yet created");
            }
            else if (SteeringAngleInputField == null)
            {
                Debug.LogWarning($"{nameof(SteeringAngleInputField)} not yet created");
            }
            else if (linkedVehicleBuilder.drivenWheelPairs.Length != DrivenWheelInputField.Length)
            {
                Debug.LogWarning($"{nameof(linkedVehicleBuilder.drivenWheelPairs)} length not correct");
                Debug.LogWarning($"   {nameof(linkedVehicleBuilder.drivenWheelPairs)} length = {linkedVehicleBuilder.drivenWheelPairs.Length}");
                Debug.LogWarning($"   {nameof(DrivenWheelInputField)} length = {DrivenWheelInputField.Length}");
            }
            else if (linkedVehicleBuilder.steeringAngleDeg.Length != DrivenWheelInputField.Length)
            {
                Debug.LogWarning($"{nameof(linkedVehicleBuilder.steeringAngleDeg)} length 2 not correct");
            }
            else if (SteeringAngleInputField.Length != DrivenWheelInputField.Length)
            {
                Debug.LogWarning($"{nameof(SteeringAngleInputField)} length 2 not correct");
            }
            else
            {
                for (int i = 0; i < DrivenWheelInputField.Length; i++)
                {
                    //DrivenWheelInputField[i].isOn = LinkedVehicleBuilder.drivenWheelPairs[i];
                    //SteeringAngleInputField[i].text = LinkedVehicleBuilder.steeringAngleDeg[i].ToString();
                }
            }

        }

        public void SetVehicleOwnerDisplay(VRCPlayerApi owner)
        {
            bool locallyOwned = owner.isLocal;

            ClaimOwnershipButton.gameObject.SetActive(!locallyOwned);
            RespawnButton.gameObject.SetActive(locallyOwned);
            FixVehicleToggle.transform.parent.gameObject.SetActive(locallyOwned);

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

            SetVehicleOwnerDisplay(Networking.GetOwner(this.linkedVehicle.LinkedVehicleSync.gameObject));

            ToggleUseCustomMesh();
        }

        void Start()
        {
            //Use Setup started by Vehicle instead
        }
    }
}