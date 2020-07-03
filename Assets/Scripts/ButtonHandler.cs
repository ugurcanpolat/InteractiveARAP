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
        #if UNITY_EDITOR
            Debug.Log("Select button is clicked.");
        #endif

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

            #if UNITY_EDITOR
                Debug.Log("Mesh is cleared.");
            #endif

            return;
        }

        Mesh selectedMesh = Resources.Load<Mesh>(selectedMeshOption);
        mesh.GetComponent<MeshFilter>().mesh = selectedMesh;

        #if UNITY_EDITOR
            Debug.Log("Mesh is selected: " + selectedMeshOption);
        #endif

        EnableInteraction();
        meshSphere.SetActive(true);

        ButtonColorSet(selectButton, activeButtonAlpha);
        ButtonColorSet(saveButton, activeButtonAlpha);
    }

    public void SaveClicked()
    {
        #if UNITY_EDITOR
            Debug.Log("Save button is clicked.");
        #endif

        Utilities.SaveMeshAsFile(mesh.GetComponent<MeshFilter>().mesh.vertices);
    }

    private void ButtonColorSet(GameObject button, float alpha)
    {
        button.GetComponent<Image>().color = new Color(255, 255, 225, alpha);
    }

    private void EnableInteraction()
    {
        inputController.SetActive(true);

        #if UNITY_EDITOR
            Debug.Log("Interaction is enabled.");
        #endif
    }

    private void DisableInteraction()
    {
        // TODO: Set MeshSphere rotation to default and add an animation

        inputController.SetActive(false);

        #if UNITY_EDITOR
            Debug.Log("Interaction is disabled.");
        #endif
    }
}
