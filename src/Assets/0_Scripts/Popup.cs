using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A popup is the element that I will place on the scene that handles which message it has
public class Popup : MonoBehaviour
{
    public static event Action<string> EnteredPopup;
    public static event Action LeftPopup;

    [SerializeField] private GameObject _identifier; // Light that shows so that player knows they can interact
    [SerializeField] private string _message;

    private void Awake()
    {
        if(_identifier == null)
        {
            // TODO: Get child component
        }
    }
    private void OnTriggerEnter(Collider collider)
    {
        print("Collided on trigger");
        if (collider.gameObject.layer == 7)
        {
            _identifier.SetActive(true); // Shows identifier so player knows they can interact with it
            EnteredPopup?.Invoke(_message);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // _showIcon = true;
        // HandleIconShowing();
    }
    
    void HandleIconShowing()
    {
        //alert.SetActive(_showIcon);
    }

    private void OnTriggerExit(Collider other)
    {
        print("On trigger exit");
        _identifier.SetActive(false);
        LeftPopup?.Invoke();
    }
}
