
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Mozilla;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using static System.Net.Mime.MediaTypeNames;

public class BuilderUIController : UdonSharpBehaviour
{
    [SerializeField] WheeledVehicleController LinkedVehicle;
    WheeledVehicleBuilder LinkedVehicleBuilder;

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

    bool editInProgress = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            UpdateUIFromVehicle();
        }
    }

    public void UpdateInputArrays()
    {
        for (int i = 0; i < LinkedVehicleBuilder.MaxWheels / 2; i++)
        {
            bool active = i < LinkedVehicleBuilder.numberOfWheels / 2;

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

        if (!Networking.IsOwner(LinkedVehicleBuilder.gameObject))
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
            LinkedVehicleBuilder.mass = currentFloat;
        }

        if (float.TryParse(widthWithWheelsInputField.text, out currentFloat))
        {
            LinkedVehicleBuilder.widthWithWheels = currentFloat;
        }

        if (float.TryParse(lengthInputField.text, out currentFloat))
        {
            LinkedVehicleBuilder.length = currentFloat;
        }

        if (float.TryParse(CenterOfMassZInputField.text, out x)
            && float.TryParse(CenterOfMassYInputField.text, out y)
            && float.TryParse(CenterOfMassYInputField.text, out z))

        {
            LinkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom = new Vector3(x, y, z);
        }

        if (float.TryParse(SteeringPositionXInputField.text, out x)
            && float.TryParse(SteeringPositionYInputField.text, out y)
            && float.TryParse(SteeringPositionZInputField.text, out z))
        {
            LinkedVehicleBuilder.driverStationPositionRelativeToCenterBottom = new Vector3(x, y, z);
        }

        //Wheels
        if (int.TryParse(NumberOfWheelsInputField.text, out currentInt))
        {
            //currentInt = (currentInt % 2 != 0) ? currentInt -= 1 : currentInt;
            currentInt = currentInt / 2 * 2; //Make even
            currentInt = (currentInt < LinkedVehicleBuilder.MinWheels) ? LinkedVehicleBuilder.MinWheels : currentInt;
            currentInt = (currentInt > LinkedVehicleBuilder.MaxWheels) ? LinkedVehicleBuilder.MaxWheels : currentInt;

            NumberOfWheelsInputField.text = currentInt.ToString(); //Doesn't invoke delegates

            LinkedVehicleBuilder.numberOfWheels = currentInt;
        }

        if (float.TryParse(WheelRadiusInputField.text, out currentFloat))
        {
            LinkedVehicleBuilder.wheelRadius = currentFloat;
        }

        if (float.TryParse(MotorTorqueInputField.text, out currentFloat))
        {
            LinkedVehicleBuilder.motorTorquePerDrivenWheel = currentFloat;
        }

        if (float.TryParse(BreakTorqueInputField.text, out currentFloat))
        {
            LinkedVehicleBuilder.breakTorquePerWheel = currentFloat;
        }

        for(int i = 0; i<LinkedVehicleBuilder.numberOfWheels  / 2; i++)
        {
            LinkedVehicleBuilder.drivenWheelPairs[i] = DrivenWheelInputField[i].isOn;

            if (float.TryParse(SteeringAngleInputField[i].text, out currentFloat))
            {
                LinkedVehicleBuilder.steeringAngleDeg[i] = currentFloat;
            }
        }

        LinkedVehicleBuilder.BuildVehiclesBasedOnBuildParameters();

        editInProgress = false;
    }

    public void UpdateUIFromVehicle()
    {
        Debug.LogWarning("UpdateUIFromVehicle");

        MassInputField.text = LinkedVehicleBuilder.mass.ToString();
        widthWithWheelsInputField.text = LinkedVehicleBuilder.widthWithWheels.ToString();
        lengthInputField.text = LinkedVehicleBuilder.length.ToString();
        CenterOfMassXInputField.text = LinkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.x.ToString();
        CenterOfMassYInputField.text = LinkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.y.ToString();
        CenterOfMassZInputField.text = LinkedVehicleBuilder.centerOfMassPositionRelativeToCenterBottom.z.ToString();
        SteeringPositionXInputField.text = LinkedVehicleBuilder.driverStationPositionRelativeToCenterBottom.x.ToString();
        SteeringPositionYInputField.text = LinkedVehicleBuilder.driverStationPositionRelativeToCenterBottom.y.ToString();
        SteeringPositionZInputField.text = LinkedVehicleBuilder.driverStationPositionRelativeToCenterBottom.z.ToString();

        NumberOfWheelsInputField.text = LinkedVehicleBuilder.numberOfWheels.ToString();
        WheelRadiusInputField.text = LinkedVehicleBuilder.wheelRadius.ToString();
        MotorTorqueInputField.text = LinkedVehicleBuilder.motorTorquePerDrivenWheel.ToString();
        BreakTorqueInputField.text = LinkedVehicleBuilder.breakTorquePerWheel.ToString();


        if (DrivenWheelInputField == null)
        {
            Debug.LogWarning($"{nameof(DrivenWheelInputField)} not yet created");
        }
        else if (SteeringAngleInputField == null)
        {
            Debug.LogWarning($"{nameof(SteeringAngleInputField)} not yet created");
        }
        else if (LinkedVehicleBuilder.drivenWheelPairs.Length != DrivenWheelInputField.Length)
        {
            Debug.LogWarning($"{nameof(LinkedVehicleBuilder.drivenWheelPairs)} length not correct");
            Debug.LogWarning($"   {nameof(LinkedVehicleBuilder.drivenWheelPairs)} length = {LinkedVehicleBuilder.drivenWheelPairs.Length}");
            Debug.LogWarning($"   {nameof(DrivenWheelInputField)} length = {DrivenWheelInputField.Length}");
        }
        else if (LinkedVehicleBuilder.steeringAngleDeg.Length != DrivenWheelInputField.Length)
        {
            Debug.LogWarning($"{nameof(LinkedVehicleBuilder.steeringAngleDeg)} length 2 not correct");
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
        ClaimOwnershipButton.gameObject.SetActive(!owner.isLocal);

        CurrentOwnerName.text = owner.playerId + ": " + owner.displayName;
    }

    public void ClaimOwnership()
    {
        LinkedVehicle.ClaimOwnership();
    }

    void Start()
    {
        Debug.LogWarning("Starting UI controller");

        SetVehicleOwnerDisplay(Networking.GetOwner(LinkedVehicle.LinkedVehicleSync.gameObject));
        LinkedVehicleBuilder = LinkedVehicle.LinkedVehicleBuilder;
        LinkedVehicleBuilder.LinkedUI = this;

        Debug.LogWarning("UpdateVehicleFromUI in 1 second");
        SendCustomEventDelayedSeconds(nameof(UpdateVehicleFromUI), 1);
    }
}
