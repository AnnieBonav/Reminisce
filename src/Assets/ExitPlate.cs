using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPlate : MonoBehaviour
{
    [SerializeField] private string _levelToGo;

    private void Awake()
    {
        if(_levelToGo == null)
        {
            _levelToGo = "MainMenu";
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        print("Collider: " + collider.name + " Layer: " + collider.gameObject.layer);
        if(collider.gameObject.layer == 7 )
        {
            print("Go to scene: " + _levelToGo);
            SceneManager.LoadScene(_levelToGo);
        }
    }
}
