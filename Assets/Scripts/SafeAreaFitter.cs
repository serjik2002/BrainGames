using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform _rect;
    Rect _lastSafeArea;
    ScreenOrientation _lastOrientation;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        // Перерахунок при зміні safeArea / орієнтації (поворот, жестова навігація, тощо)
        if (Screen.safeArea != _lastSafeArea || Screen.orientation != _lastOrientation)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect sa = Screen.safeArea;

        Vector2 min = sa.position;
        Vector2 max = sa.position + sa.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        _rect.anchorMin = min;
        _rect.anchorMax = max;

        _lastSafeArea = sa;
        _lastOrientation = Screen.orientation;
    }
}