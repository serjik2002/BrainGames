using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class IconThemeListener : MonoBehaviour
{
    [Header("Цвета для конкретной мини-игры")]
    public Color lightColor = Color.white;
    public Color darkColor = Color.gray;

    private Graphic _graphicElement;

    private void Awake()
    {
        _graphicElement = GetComponent<Graphic>();
    }

    private void Start()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged += UpdateColor;
            UpdateColor(); // Красим при появлении префаба на сцене
        }
    }

    private void OnDestroy()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged -= UpdateColor;
        }
    }

    private void UpdateColor()
    {
        if (_graphicElement == null || ThemeManager.Instance == null) return;

        // Берем текущую тему из глобального менеджера, но цвета используем свои, локальные!
        _graphicElement.color = ThemeManager.Instance.CurrentTheme switch
        {
            ThemeType.Light => lightColor,
            ThemeType.Dark => darkColor,
            _ => lightColor
        };
    }
}