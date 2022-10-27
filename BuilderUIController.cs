
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Mozilla;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class BuilderUIController : UdonSharpBehaviour
{
    [SerializeField] WheeledVehicleBuilder LinkedVehicleBuilder;

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
    }

    void UpdateUIFromVehicle()
    {

    }

    void Start()
    {
        
    }
}
