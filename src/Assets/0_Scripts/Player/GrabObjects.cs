using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GrabObjects : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGrabbable;
    [SerializeField] private string _levelLinkedTo;

    bool _dialogueOpened;
    bool _changeLevelSelected;
    bool _isCollidingGrabbable;
    GameObject world;
    [SerializeField] private PopupSystem _popupSystem;


    private void Awake()
    {
        _dialogueOpened = false;
        _changeLevelSelected = false;
        if (_popupSystem == null)
        {
            print("This was null");
        }
    }

    public void OnInteractDialogue(InputValue value)
    {
        if (_isCollidingGrabbable)
        {
            if (_changeLevelSelected)
            {
                ChangeLevel();
                return;
            }

            if (!_dialogueOpened)
            {
                //_popupSystem.PopUp("This needed text");
            }
            else
            {
                //_popupSystem.ClosePop();
            }
            _dialogueOpened = !_dialogueOpened;
        }
        else
        {
            if (_dialogueOpened)
            {
                //_popupSystem.ClosePop();
                _dialogueOpened = !_dialogueOpened;
            }
        }
        

    }

    private void OnTriggerStay(Collider collidedObject)
    {
        if (collidedObject.gameObject.layer == 7)
        {
            if (collidedObject.gameObject.name == "ChangeLevel")
            {

                _changeLevelSelected = true;
            }
            _isCollidingGrabbable = true;
        }
        
    }

    void ChangeLevel()
    {
        SceneManager.LoadScene(_levelLinkedTo);
    }

    private void OnTriggerExit(Collider other)
    {
        _isCollidingGrabbable = false;
    }
}
