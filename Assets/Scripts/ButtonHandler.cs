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
    public GameObject interactionButton;
    public GameObject deformationButton;

    private const float activeButtonAlpha = 1f;
    private const float disabledButtonAlpha = 100f / 255f;

    private USAGE_MODE usageMode = USAGE_MODE.OTHER;

    public void SelectClicked()
    {
        Debug.Log("Select button is clicked.");

        DisableInteraction();
        meshSphere.SetActive(false);

        ButtonColorSet(saveButton, disabledButtonAlpha);
        ButtonColorSet(interactionButton, disabledButtonAlpha);
        ButtonColorSet(deformationButton, disabledButtonAlpha);

        // TODO: Add mesh selection
        Mesh selectedMesh = Resources.Load<Mesh>("armadillo_1k");
        mesh.GetComponent<MeshFilter>().mesh = selectedMesh;

        Debug.Log("Mesh is selected.");

        EnableInteraction();
        meshSphere.SetActive(true);

        ButtonColorSet(saveButton, activeButtonAlpha);
        ButtonColorSet(interactionButton, activeButtonAlpha);
    }

    public void SaveClicked()
    {
        // TODO: Add save mesh function
        Debug.Log("Save button is clicked.");
    }

    public void InteractionClicked()
    {
        Debug.Log("Interaction mode button is clicked.");

        if (usageMode == USAGE_MODE.INTERACTION)
        {
            return;
        }

        EnableInteraction();

        ButtonColorSet(deformationButton, disabledButtonAlpha);
        ButtonColorSet(interactionButton, activeButtonAlpha);
    }

    public void DeformationClicked()
    {
        Debug.Log("Deformation mode button is clicked.");

        if (usageMode == USAGE_MODE.DEFORMATION)
        {
            return;
        }

        DisableInteraction();

        // TODO: Add deformation mode
        usageMode = USAGE_MODE.DEFORMATION;

        ButtonColorSet(interactionButton, disabledButtonAlpha);
        ButtonColorSet(deformationButton, activeButtonAlpha);
    }

    private void ButtonColorSet(GameObject button, float alpha)
    {
        button.GetComponent<Image>().color = new Color(255, 255, 225, alpha);
    }

    private void EnableInteraction()
    {
        if (usageMode == USAGE_MODE.INTERACTION)
        {
            return;
        }

        inputController.SetActive(true);
        usageMode = USAGE_MODE.INTERACTION;
    }

    private void DisableInteraction()
    {
        if (usageMode == USAGE_MODE.INTERACTION)
        {
            inputController.SetActive(false);
        }

        // TODO: Set MeshSphere rotation to default and add an animation
        usageMode = USAGE_MODE.OTHER;
    }
}
