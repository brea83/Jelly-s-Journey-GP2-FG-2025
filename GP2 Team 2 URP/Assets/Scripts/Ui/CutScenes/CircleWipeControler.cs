using NGAME;
using RoomSystem;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
[ExecuteAlways]
public class CircleWipeControler : MonoBehaviour
{
    private Image _canvasImage;
    private int _imgSizeId = Shader.PropertyToID("_Size");
    private int _circleSizeId = Shader.PropertyToID("_FadeProgress");
    //private int _shaderFadeIn = Shader.PropertyToID("_FadeIn");
    //private int _shaderFadeOut = Shader.PropertyToID("_FadeOut");
    //private int _shaderPrevFade = Shader.PropertyToID("_PrevFadeIn");
    //private bool _previousFadeWasIn = false;
    public bool DoingFadeOut = false;
    public bool DoingFadeIn = false;
    private bool _firstFadeFrame = true;
    private float _fadeTimer = 0.0f;
    [SerializeField] private float _fadeSeconds = 1.0f;
    public float FadeSeconds { get { return _fadeSeconds; } }
    private void Start()
    {
        _canvasImage = GetComponent<Image>();

        //RoomNavigator navigator = RoomNavigator.Instance;
        //navigator.RoomLoadStart.AddListener(OnRoomLoadStart);
        //navigator.RoomLoadComplete.AddListener(OnRoomLoadComplete);
    }

    public void OnRoomLoadComplete(IEncounterRegionConnector connector)
    {
        DoingFadeIn = true;
        Debug.Log($"DOING FADE IN. timescale is {Time.timeScale} and time is {Time.time}");
    }

    public void OnRoomLoadStart()
    {
        DoingFadeOut = true;
        Debug.Log($"DOING FADE OUT. timescale is {Time.timeScale} and time is {Time.time}");
    }

#if UNITY_EDITOR
    void OnValidate() { UpdateMaterial(); }
#endif 

    private void Update()
    {
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (_canvasImage != null && _canvasImage.material != null)
        {
            Rect imageRect = _canvasImage.rectTransform.rect;
            Vector2 widthHeight = new (x: imageRect.width, y: imageRect.height);
            _canvasImage.material.SetVector(_imgSizeId, widthHeight);

            if (DoingFadeOut)
            {
                CheckFirstFrame();
                FadeOut();

                CountUpFadeTimer();
            }
            else if(DoingFadeIn)
            {
                CheckFirstFrame();
                FadeIn();
                
                CountDownFadeTimer();
            }
        }
    }

    private void CountUpFadeTimer()
    {
        _fadeTimer += Time.fixedDeltaTime;
        if (_fadeTimer >= _fadeSeconds)
        {
            if (DoingFadeIn) ToggleOffFadeIn();
            if (DoingFadeOut) ToggleOffFadeOut();
            _fadeTimer = _fadeSeconds;
            _firstFadeFrame = true;
        }
    }
    private void CountDownFadeTimer()
    {
        _fadeTimer -= Time.fixedDeltaTime;
        if (_fadeTimer <= 0.0f)
        {
            if (DoingFadeIn) ToggleOffFadeIn();
            if (DoingFadeOut) ToggleOffFadeOut();
            _fadeTimer = 0.0f;
            _firstFadeFrame = true;
        }
    }
    private void ToggleOffFadeOut()
    {
        DoingFadeOut = false;
        //_canvasImage.material.SetInt(_shaderFadeOut, 0);
    }
    private void ToggleOffFadeIn()
    {
        DoingFadeIn = false;
        //_canvasImage.material.SetInt(_shaderFadeIn, 0);
    }
    public void FadeOut()
    {
        if (_firstFadeFrame)
        {
            DoingFadeIn = false;
            //_previousFadeWasIn = false;
            _fadeTimer = 0.0f;
            //_canvasImage.material.SetInt(_shaderPrevFade, _previousFadeWasIn ? 1 : 0);
            //_canvasImage.material.SetInt(_shaderFadeOut, 1);
        }
        _canvasImage.material.SetFloat(_circleSizeId, _fadeTimer);
        //_firstFadeFrame = false;
    }

    public void FadeIn()
    {
        if (_firstFadeFrame)
        {
            DoingFadeOut = false;
            //_previousFadeWasIn = true;
            _fadeTimer = _fadeSeconds;
            //_canvasImage.material.SetInt(_shaderPrevFade, _previousFadeWasIn ? 1 : 0); 
            //_canvasImage.material.SetInt(_shaderFadeIn, 1);
        }
        _canvasImage.material.SetFloat(_circleSizeId, _fadeTimer);
        //_firstFadeFrame = false;
    }

    private void CheckFirstFrame()
    {
        if (_firstFadeFrame)
        {
            _firstFadeFrame = false;
        }
    }
}