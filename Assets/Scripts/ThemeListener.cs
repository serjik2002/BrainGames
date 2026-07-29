using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ThemeListener : MonoBehaviour
{
    // Выбираем в инспекторе, чем является этот объект
    public UIElement elementType;

    private Graphic _graphicElement;

    private void Awake()
    {
        _graphicElement = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        ThemeManager.Instance.OnThemeChanged += UpdateColor;
        UpdateColor(); // Красим при включении
    }

    private void OnDisable()
    {
        ThemeManager.Instance.OnThemeChanged -= UpdateColor;
    }

    private void UpdateColor()
    {
        if (_graphicElement != null)
        {
            // Запрашиваем цвет у статического класса, передавая свой тип
            _graphicElement.color = ThemeManager.Instance.GetColor(elementType);
        }
    }
}