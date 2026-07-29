using System;
using UnityEngine;

public enum UIElement
{
    Background,
    Text1,
    Text2,
    CardBg,
    MenuSectionHeader,  // Заголовки секций (Settings, More)
    MenuButtonMainText, // Главный текст кнопок (Theme, Sound, Privacy policy)
    MenuButtonSubText
}

// Добавляем новые темы сюда
public enum ThemeType { Light, Dark }

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    public ThemeType CurrentTheme { get; private set; } = ThemeType.Light;
    public event Action OnThemeChanged;

    private void Awake()
    {
        // Настраиваем Синглтон и делаем его неуничтожаемым при загрузке новых сцен
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadTheme();
    }

    public void SetTheme(ThemeType theme)
    {
        if (CurrentTheme == theme) return;

        CurrentTheme = theme;
        SaveTheme(); // Сохраняем выбор игрока
        OnThemeChanged?.Invoke();
    }

    // Сохранение в настройки устройства
    private void SaveTheme()
    {
        Options.SetInt("Theme.currentTheme", (int)CurrentTheme);
        Options.Save();
    }

    // Загрузка из настроек устройства
    private void LoadTheme()
    {
        int savedThemeIndex = Options.GetInt("Theme.currentTheme", 0);
        CurrentTheme = (ThemeType)savedThemeIndex;
    }

    public Color GetColor(UIElement element)
    {
        switch (CurrentTheme)
        {
            case ThemeType.Light:
                return HexToColor(element switch
                {
                    UIElement.Background => "#F6F9FA",
                    UIElement.Text1 => "#000000",
                    UIElement.Text2 => "#8A90A0",
                    UIElement.CardBg => "#FFFFFF",
                    UIElement.MenuSectionHeader => "#62768E",
                    // Главный текст кнопок — светлый для хорошей читаемости на темном фоне
                    UIElement.MenuButtonMainText => "#87A1BE",
                    // Второстепенный текст — темнее основного, чтобы увести на второй план
                    UIElement.MenuButtonSubText => "#87A1BE",

                    _ => "#FF00FF"
                });

            case ThemeType.Dark:
                return HexToColor(element switch
                {
                    UIElement.Background => "#101219",
                    UIElement.Text1 => "#E5E5E5",
                    UIElement.Text2 => "#7E869D",
                    UIElement.CardBg => "#171B21",
                    UIElement.MenuSectionHeader => "#85ACF5",
                    // Главный текст кнопок — светлый для хорошей читаемости на темном фоне
                    UIElement.MenuButtonMainText => "#E5E5E5",
                    // Второстепенный текст — темнее основного, чтобы увести на второй план
                    UIElement.MenuButtonSubText => "#95989E",

                    _ => "#FF00FF"
                });

            default:
                Debug.LogWarning($"Тема {CurrentTheme} не настроена!");
                return Color.magenta;
        }
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        Debug.LogWarning($"Ошибка парсинга цвета: {hex}");
        return Color.magenta;
    }
}
