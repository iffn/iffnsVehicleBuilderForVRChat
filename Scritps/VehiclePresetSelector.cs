using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.WheeledVehicles
{
    public class VehiclePresetSelector : UdonSharpBehaviour
    {
        [Header("Settings")]
        [SerializeField] PresetVehicleTypes PresetType;
        [Header("Unity assingments")]
        [SerializeField] BuilderUIController LinkedUI;

        public void SetPreset()
        {
            LinkedUI.SetVehiclePreset(PresetType);
        }
    }
}