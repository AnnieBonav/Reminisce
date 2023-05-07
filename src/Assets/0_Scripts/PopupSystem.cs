using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupSystem : MonoBehaviour
{
    [SerializeField] private GameObject popupBox;
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_Text popupText;

    private void Awake()
    {
        Popup.EnteredPopup += OpenPopup;
        Popup.LeftPopup += ClosePopup;
    }

    private void OnDisable()
    {
        Popup.EnteredPopup -= OpenPopup;
        Popup.LeftPopup -= ClosePopup;
    }

    private void OpenPopup(string text)
    {
        print("Popping");
        popupText.text = text;
        popupBox.SetActive(true);
        animator.SetTrigger("Pop");
    }

    private void ClosePopup()
    {
        print("Unpopping");
        animator.SetTrigger("Close");
    }
}
