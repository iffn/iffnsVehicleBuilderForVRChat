
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class InputLineController : UdonSharpBehaviour //This class should be abstract but isn't because of U# being U#
    {
        [Header("Unity assingments")]
        [SerializeField] TMPro.TextMeshProUGUI LinkedTitle;
        [SerializeField] protected InputField LinkedInputField;
        [SerializeField] protected Slider LinkedSlider;

        protected BuilderUIController linkedUIController;

        protected virtual void Setup(BuilderUIController linkedUIController)
        {
            this.linkedUIController = linkedUIController;
        }
    }
}