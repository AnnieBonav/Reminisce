using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class PopupSystem : MonoBehaviour
{
    [SerializeField] private GameObject popupBox;
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_Text popupText;

    private void Awake()
    {
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
        print("Setting popup: " + text);
        popupText.text = text;
    }
    private void OpenPopup()
    {
        print("Openning popup");
        popupBox.SetActive(true);
        animator.SetTrigger("Pop");
    }

    private void ClosePopup()
    {
        print("Unpopping");
        animator.SetTrigger("Close");
        popupBox.SetActive(false);
    }
}
