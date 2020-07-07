﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private Animator selectButtonAnimator;
    private bool isSelectionMenuOn = false;

    private const float activeButtonAlpha = 1f;
    private const float disabledButtonAlpha = 100f / 255f;

    private string selectedMeshOption = MESH_OPTIONS.NONE;

    private void Start()
    {
        selectButtonAnimator = selectButton.GetComponent<Animator>();
    }

    public void SelectClicked()
    {
        #if UNITY_EDITOR
            Debug.Log("Select button is clicked.");
        #endif

        ChangeButtonInteractivity(saveButton, false);
        DisableInteraction();
        meshSphere.SetActive(false);

        isSelectionMenuOn = !isSelectionMenuOn;
        selectButtonAnimator.SetBool("SelectionMenuOn", isSelectionMenuOn);
        EventSystem.current.SetSelectedGameObject(null);

        // TODO: Add mesh selection

        selectedMeshOption = MESH_OPTIONS.ARMADILLO;

        if (selectedMeshOption == MESH_OPTIONS.NONE)
        {
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

        ChangeButtonInteractivity(saveButton, true);
    }

    public void SaveClicked()
    {
        #if UNITY_EDITOR
            Debug.Log("Save button is clicked.");
        #endif

        ChangeButtonInteractivity(selectButton, false);
        ChangeButtonInteractivity(saveButton, false);

        Mesh currentMesh = mesh.GetComponent<MeshFilter>().mesh;
        Utilities.SaveMeshAsFile(currentMesh.vertices, currentMesh.triangles);

        ChangeButtonInteractivity(selectButton, true);
        ChangeButtonInteractivity(saveButton, true);
    }

    private void EnableInteraction()
    {
        // TODO: Set MeshSphere rotation to default and add an animation

        inputController.SetActive(true);

        #if UNITY_EDITOR
            Debug.Log("Interaction is enabled.");
        #endif
    }

    private void DisableInteraction()
    {
        inputController.SetActive(false);

        #if UNITY_EDITOR
            Debug.Log("Interaction is disabled.");
        #endif
    }

    private void ChangeButtonInteractivity(GameObject buttonParent, bool state)
    {
        Button button = buttonParent.GetComponent<Button>();
        button.interactable = state;

        #if UNITY_EDITOR
            Debug.Log(buttonParent.name + " interactivity set to: "+ state);
        #endif
    }
}
