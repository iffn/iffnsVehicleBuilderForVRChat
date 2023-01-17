
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MapDisplay : UdonSharpBehaviour
{
    [Header("Unity assingments")]
    [SerializeField] Transform MapReference;
    [SerializeField] Material LinkedMaterial;
    [SerializeField] Transform ArrowHolder;

    private void Start()
    {
        if(MapReference == null)
        {
            enabled = false;
            return;
        }

        float aspectRatio = MapReference.localScale.x / MapReference.localScale.y;

        LinkedMaterial.SetFloat("_AspectRatio", aspectRatio);
    }

    private void Update()
    {
        if (MapReference == null)
        {
            return;
        }

        Vector3 localPosition = MapReference.InverseTransformPoint(transform.position);

        localPosition += 0.5f * Vector3.one;

        LinkedMaterial.SetVector("_WindowPosition", localPosition);

        float heading = transform.rotation.eulerAngles.y;

        ArrowHolder.localRotation = Quaternion.Euler(0, 0, -heading);
    }

    public void ZoomIn()
    {
        float currentLevel = LinkedMaterial.GetFloat("_WindowSize");

        LinkedMaterial.SetFloat("_WindowSize", currentLevel * 0.8f);
    }

    public void ZoomOut()
    {
        float currentLevel = LinkedMaterial.GetFloat("_WindowSize");

        LinkedMaterial.SetFloat("_WindowSize", currentLevel * 1.25f);
    }
}
