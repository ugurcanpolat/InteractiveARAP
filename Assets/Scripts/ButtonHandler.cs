using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum USAGE_MODE
{
    INTERACTION, DEFORMATION, OTHER
};

public class ButtonHandler : MonoBehaviour
{
    public GameObject inputController;
    public GameObject meshSphere;
    public GameObject mesh;

    public GameObject selectButton;
    public GameObject saveButton;

    private const float activeButtonAlpha = 1f;
    private const float disabledButtonAlpha = 100f / 255f;

    private USAGE_MODE usageMode = USAGE_MODE.OTHER;

    public void SelectClicked()
    {
        Debug.Log("Select button is clicked.");

        DisableInteraction();
        meshSphere.SetActive(false);

        ButtonColorSet(selectButton, disabledButtonAlpha);
        ButtonColorSet(saveButton, disabledButtonAlpha);

        // TODO: Add mesh selection
        Mesh selectedMesh = Resources.Load<Mesh>("armadillo_1k");
        mesh.GetComponent<MeshFilter>().mesh = selectedMesh;

        Debug.Log("Mesh is selected.");

        EnableInteraction();
        meshSphere.SetActive(true);

        ButtonColorSet(selectButton, activeButtonAlpha);
        ButtonColorSet(saveButton, activeButtonAlpha);
    }

    public void SaveClicked()
    {
        // TODO: Add save mesh function
        Debug.Log("Save button is clicked.");
    }

    private void ButtonColorSet(GameObject button, float alpha)
    {
        button.GetComponent<Image>().color = new Color(255, 255, 225, alpha);
    }

    private void EnableInteraction()
    {
        inputController.SetActive(true);
    }

    private void DisableInteraction()
    {
        // TODO: Set MeshSphere rotation to default and add an animation
        inputController.SetActive(false);
    }
}
