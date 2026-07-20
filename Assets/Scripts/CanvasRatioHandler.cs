using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasRatioHandler : MonoBehaviour
{
    private CanvasScaler _scaler;

    private float _referenceWidth;
    private float _referenceHeight;

    void Awake()
    {
        _scaler = GetComponent<CanvasScaler>();

        _referenceWidth = _scaler.referenceResolution.x;
        _referenceHeight = _scaler.referenceResolution.y;

        UpdateCanvasMatch();
    }

    
    public void UpdateCanvasMatch()
    {
        if (_scaler == null) return;

       
        float screenRatio = (float)Screen.width / (float)Screen.height;
        
        float targetRatio = _referenceWidth / _referenceHeight;

        if (screenRatio >= targetRatio)
        {
            _scaler.matchWidthOrHeight = 1f;
        }
        else
        {
            _scaler.matchWidthOrHeight = 0f;
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        UpdateCanvasMatch();
    }
#endif
}