using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class PopupSystem : MonoBehaviour
{
    [SerializeField] private GameObject _popupUI;
    private Animator _popupUIAnimator;
    [SerializeField] private TMP_Text popupText; // TODO: Add find reference to the popupText (avoid nulls)

    private void Awake()
    {
        _popupUIAnimator = _popupUI.GetComponent<Animator>();
        Popup.EnteredPopup += SetPopup; // I use the same event that lets the character controller something is being touched, set the message of the collider that was touched
        CharacterController.GrabbedObject += OpenPopup; // I use the character saying something was grabbed to open it
        Popup.LeftPopup += ClosePopup; // And then the popup leaving the area to close it...Or maybe the player closes it?
    }

    private void OnDisable()
    {
        Popup.EnteredPopup -= SetPopup;
        CharacterController.GrabbedObject -= OpenPopup;
        Popup.LeftPopup -= ClosePopup;
    }

    private void SetPopup(string text)
    {
        popupText.text = text;
    }
    private void OpenPopup()
    {
        _popupUI.SetActive(true);
        // _popupUIAnimator.SetTrigger("Pop"); // TODO: Implement opening animation
    }

    private void ClosePopup()
    {
        _popupUI.SetActive(false);
        // _popupUIAnimator.SetTrigger("Close"); // TODO: Implement closing animation
    }
}
