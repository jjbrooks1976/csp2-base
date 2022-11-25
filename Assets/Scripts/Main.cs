using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Simulation simulation;
    public GameObject canvas;

    public Toggle errorCorrectionToggle;
    public Toggle correctionSmoothingToggle;
    public Toggle redundantInputToggle;

    public void Start()
    {
        errorCorrectionToggle.isOn = true;
        correctionSmoothingToggle.isOn = true;
        redundantInputToggle.isOn = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            canvas.SetActive(!canvas.activeSelf);
        }
    }

    public void OnErrorCorrectionToggled(bool value)
    {
        correctionSmoothingToggle.interactable = value;
        simulation.errorCorrection = value;
    }

    public void OnCorrectionSmoothingToggled(bool value)
    {
        simulation.correctionSmoothing = value;
    }

    public void OnRedundantInputToggled(bool value)
    {
        simulation.redundantInput = value;
    }
}
