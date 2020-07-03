using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct MESH_OPTIONS
{
    public const string ARMADILLO = "armadillo_1k";
    public const string NONE = "None";
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

    private string selectedMeshOption = MESH_OPTIONS.NONE;

    public void SelectClicked()
    {
        Debug.Log("Select button is clicked.");

        DisableInteraction();
        meshSphere.SetActive(false);

        ButtonColorSet(selectButton, disabledButtonAlpha);
        ButtonColorSet(saveButton, disabledButtonAlpha);

        // TODO: Add mesh selection

        selectedMeshOption = MESH_OPTIONS.ARMADILLO;

        if (selectedMeshOption == MESH_OPTIONS.NONE)
        {
            ButtonColorSet(selectButton, activeButtonAlpha);
            mesh.GetComponent<MeshFilter>().mesh = null;

            Debug.Log("Mesh is cleared.");

            return;
        }

        Mesh selectedMesh = Resources.Load<Mesh>(selectedMeshOption);
        mesh.GetComponent<MeshFilter>().mesh = selectedMesh;

        Debug.Log("Mesh is selected: " + selectedMeshOption);

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

        Debug.Log("Interaction is enabled.");
    }

    private void DisableInteraction()
    {
        // TODO: Set MeshSphere rotation to default and add an animation

        inputController.SetActive(false);

        Debug.Log("Interaction is disabled.");
    }
}
