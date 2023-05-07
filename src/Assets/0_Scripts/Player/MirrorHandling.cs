using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

public enum CurrentTime { Present, Past };

public class MirrorHandling : MonoBehaviour
{
    [Header("Mirror information")]
    [SerializeField] GameObject _presentMirrorPrefab;
    [SerializeField] GameObject _pastMirrorPrefab;
    [SerializeField] Material _presentSkybox;
    [SerializeField] Material _pastSkybox;
    [SerializeField] Transform _presentWorldTransform;
    [SerializeField] Transform _pastWorldTransform;
    [SerializeField] float _distanceToMirror = 2f;
    [SerializeField] CurrentTime _startingTime;

    private bool _isMirrorCasted;
    private Transform _playerTransform;
    private GameObject _presentMirror; //Having one Mirror creates a bug because they both need a transform (could fix with position and rotation variables but having two doors is fine)
    private GameObject _pastMirror;

    private CurrentTime _currentTime;

    private Vector3 _PresentToPastWorldTranslation;


    void Awake()
    {
        _currentTime = _startingTime;

        _PresentToPastWorldTranslation = _pastWorldTransform.position - _presentWorldTransform.position;
        _playerTransform = GetComponent<Transform>();
        _presentMirror = Instantiate(_presentMirrorPrefab);
        _pastMirror = Instantiate(_pastMirrorPrefab, _presentMirror.transform.position + _PresentToPastWorldTranslation, _presentMirror.transform.rotation); //Prefab can change if needed
        _presentMirror.SetActive(false);
        _pastMirror.SetActive(false);

        if (_distanceToMirror == 0) _distanceToMirror = 2f;
    }

    void OnEnable()
    {
        Mirror.onMirrorCollided += HandleMirrorCollision;
    }

    void OnDisable()
    {
        Mirror.onMirrorCollided -= HandleMirrorCollision;
    }

    void HandleMirrorCasted()
    {
        _presentMirror.SetActive(_isMirrorCasted);
        _pastMirror.SetActive(_isMirrorCasted);

        if (!_isMirrorCasted)
        {
            return;
        }

        if (_currentTime == CurrentTime.Present)
        {
            Quaternion mirrorRotation = Quaternion.Euler(new Vector3(0f, _playerTransform.rotation.eulerAngles.y + 180f, 0f));
            Vector3 presentMirrorPosition = _playerTransform.position + _playerTransform.forward * _distanceToMirror + new Vector3(0f, _presentMirror.transform.localScale.y, 0f);
            _presentMirror.transform.SetPositionAndRotation(presentMirrorPosition, mirrorRotation);

            Vector3 pastMirrorPosition = presentMirrorPosition + _PresentToPastWorldTranslation;
            _pastMirror.transform.SetPositionAndRotation(pastMirrorPosition, mirrorRotation);
        }
        else
        {
            Quaternion mirrorRotation = Quaternion.Euler(new Vector3(0f, _playerTransform.rotation.eulerAngles.y + 180f, 0f));
            Vector3 pastMirrorPosition = _playerTransform.position + _playerTransform.forward * _distanceToMirror + new Vector3(0f, _pastMirror.transform.localScale.y, 0f);
            _pastMirror.transform.SetPositionAndRotation(pastMirrorPosition, mirrorRotation);

            Vector3 presentMirrorPosition = pastMirrorPosition - _PresentToPastWorldTranslation;
            _presentMirror.transform.SetPositionAndRotation(presentMirrorPosition, mirrorRotation);
        }
    }

    void HandleMirrorCollision()
    {
        ChangePlayerPosition();
        _currentTime = (_currentTime == CurrentTime.Present) ? CurrentTime.Past : CurrentTime.Present;
        UpdateSkybox();
    }

    void UpdateSkybox()
    {
        RenderSettings.skybox = (_currentTime == CurrentTime.Present) ? _presentSkybox : _pastSkybox;
    }

    void ChangePlayerPosition()
    {
        if (_currentTime == CurrentTime.Present)
        {
            Vector3 presentPlayerPosition = _pastMirror.transform.position + _pastMirror.transform.forward * _distanceToMirror;
            _playerTransform.SetPositionAndRotation(presentPlayerPosition, Quaternion.Euler(new Vector3(0f, _pastMirror.transform.rotation.eulerAngles.y, 0f)));
        }
        else
        {
            Vector3 pastPlayerPosition = _presentMirror.transform.position + _presentMirror.transform.forward * _distanceToMirror;
            _playerTransform.SetPositionAndRotation(pastPlayerPosition, Quaternion.Euler(new Vector3(0f, _presentMirror.transform.rotation.eulerAngles.y, 0f)));
        }
    }

    //Input System
    public void OnCastMirror(InputValue value)
    {
        _isMirrorCasted = !_isMirrorCasted;
        HandleMirrorCasted();
    }
}
