using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class FloatInputLineController : InputLineController
    {
        float min;
        float max;
        float currentValue;
        bool applyLimits;
        bool ignoreCallback = false;

        public void Setup(BuilderUIController linkedUIController, float min, float max, float defaultValue, bool applyLimits)
        {
            base.Setup(linkedUIController);

            this.min = min;
            this.max = max;
            this.applyLimits = applyLimits;


            ignoreCallback = true;
            LinkedSlider.wholeNumbers = false;
            ignoreCallback = true;
            LinkedSlider.minValue = min;
            ignoreCallback = true;
            LinkedSlider.maxValue = max;

            ApplyLimits = applyLimits;
            Value = defaultValue;
            ignoreCallback = false;
        }

        public bool ApplyLimits
        {
            set
            {
                bool update = !applyLimits && value; //Only update if switched on

                applyLimits = value;

                if (update)
                {
                    Value = currentValue;
                }

                LinkedSlider.gameObject.SetActive(value);
            }
        }

        public float Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                ignoreCallback = true;

                if (applyLimits) currentValue = Mathf.Clamp(value, min, max);
                else currentValue = value;

                LinkedSlider.value = currentValue;

                LinkedInputField.text = "" + currentValue;

                ignoreCallback = false;
            }
        }

        void Start()
        {

        }

        //Calls for Unity UI
        public void UpdateFromInputField()
        {
            if (ignoreCallback) return;

            string valueText = LinkedInputField.text;

            if (float.TryParse(valueText, out float value))
            {
                Value = value;
            }
            else
            {
                Value = currentValue;
            }

            linkedUIController.UpdateVehicleFromUI();
        }

        public void UpdateFromSlider()
        {
            if (ignoreCallback) return;

            Value = LinkedSlider.value;

            linkedUIController.UpdateVehicleFromUI();
        }
    }
}