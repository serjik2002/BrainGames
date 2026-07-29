using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SideMenuController : MonoBehaviour
{
    [Header("Menu Animation Settings")]
    [SerializeField] private RectTransform _sidePanel;
    [SerializeField] private CanvasGroup _backgroundDim;
    [SerializeField] private float _animationDuration = 0.3f;

    [Header("Top Buttons")]
    [SerializeField] private Button _closeMenuButton;
    [SerializeField] private Button _backgroundCloseButton;

    [Header("Settings Buttons")]
    [SerializeField] private Button _themeButton;
    [SerializeField] private TMP_Text _themeStatusText;
    [Tooltip("Компонент Image самой иконки на кнопке Theme")]
    [SerializeField] private Image _themeIconImage;
    [Tooltip("Спрайт луны (Темная тема)")]
    [SerializeField] private Sprite _darkThemeSprite;
    [Tooltip("Спрайт солнца (Светлая тема)")]
    [SerializeField] private Sprite _lightThemeSprite;

    [Space(10)]
    [SerializeField] private Button _soundButton;
    [SerializeField] private TMP_Text _soundStatusText;

    [Header("More Buttons")]
    [SerializeField] private Button _privacyPolicyButton;
    [SerializeField] private Button _removeAdsButton;
    [SerializeField] private Button _contactUsButton;

    [Header("Footer")]
    [SerializeField] private TMP_Text _versionText;

    private float _hiddenPosX;
    private bool _isOpen = false;
    private Coroutine _animationCoroutine;

    private bool _isDarkTheme = true;
    private bool _isSoundOn = true;

    private void Start()
    {
        InitializeMenuState();
        LoadSettings();
        UpdateSettingsUI();
        SetupButtonListeners();
        SetupVersionText();
    }

    private void InitializeMenuState()
    {
        _hiddenPosX = -_sidePanel.rect.width;
        _sidePanel.anchoredPosition = new Vector2(_hiddenPosX, _sidePanel.anchoredPosition.y);
        _backgroundDim.alpha = 0f;
        _backgroundDim.blocksRaycasts = false;
    }

    private void SetupVersionText()
    {
        if (_versionText != null)
        {
            _versionText.text = $"(c) UnitedITForce\nv {Application.version}";
        }
    }

    private void LoadSettings()
    {
        _isDarkTheme = PlayerPrefs.GetInt("IsDarkTheme", 1) == 1;
        _isSoundOn = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;

        ApplySoundState();
        ApplyThemeState();
    }

    private void SetupButtonListeners()
    {
        if (_closeMenuButton != null) _closeMenuButton.onClick.AddListener(CloseMenu);
        if (_backgroundCloseButton != null) _backgroundCloseButton.onClick.AddListener(CloseMenu);

        if (_themeButton != null) _themeButton.onClick.AddListener(ToggleTheme);
        if (_soundButton != null) _soundButton.onClick.AddListener(ToggleSound);

        if (_privacyPolicyButton != null) _privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
        if (_removeAdsButton != null) _removeAdsButton.onClick.AddListener(BuyRemoveAds);
        if (_contactUsButton != null) _contactUsButton.onClick.AddListener(ContactUs);
    }

    private void OnDestroy()
    {
        if (_closeMenuButton != null) _closeMenuButton.onClick.RemoveAllListeners();
        if (_backgroundCloseButton != null) _backgroundCloseButton.onClick.RemoveAllListeners();
        if (_themeButton != null) _themeButton.onClick.RemoveAllListeners();
        if (_soundButton != null) _soundButton.onClick.RemoveAllListeners();
        if (_privacyPolicyButton != null) _privacyPolicyButton.onClick.RemoveAllListeners();
        if (_removeAdsButton != null) _removeAdsButton.onClick.RemoveAllListeners();
        if (_contactUsButton != null) _contactUsButton.onClick.RemoveAllListeners();
    }

    public void OpenMenu()
    {
        if (_isOpen) return;
        _isOpen = true;
        _backgroundDim.blocksRaycasts = true;
        StartMenuAnimation(0f, 1f);
    }

    public void CloseMenu()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _backgroundDim.blocksRaycasts = false;
        StartMenuAnimation(_hiddenPosX, 0f);
    }

    private void StartMenuAnimation(float targetX, float targetAlpha)
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(AnimateMenu(targetX, targetAlpha));
    }

    private IEnumerator AnimateMenu(float targetX, float targetAlpha)
    {
        float elapsedTime = 0f;
        Vector2 startPos = _sidePanel.anchoredPosition;
        Vector2 targetPos = new Vector2(targetX, startPos.y);
        float startAlpha = _backgroundDim.alpha;

        while (elapsedTime < _animationDuration)
        {
            float t = elapsedTime / _animationDuration;
            t = t * t * (3f - 2f * t);

            _sidePanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            _backgroundDim.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _sidePanel.anchoredPosition = targetPos;
        _backgroundDim.alpha = targetAlpha;
    }

    private void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        PlayerPrefs.SetInt("IsDarkTheme", _isDarkTheme ? 1 : 0);
        PlayerPrefs.Save();

        ApplyThemeState();
        UpdateSettingsUI();
    }

    private void ToggleSound()
    {
        _isSoundOn = !_isSoundOn;
        PlayerPrefs.SetInt("IsSoundOn", _isSoundOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplySoundState();
        UpdateSettingsUI();
    }

    private void ApplyThemeState()
    {
        Debug.Log("Theme switched to: " + (_isDarkTheme ? "Dark" : "Light"));
    }

    private void ApplySoundState()
    {
        AudioListener.volume = _isSoundOn ? 1f : 0f;
    }

    private void UpdateSettingsUI()
    {
        // Обновляем текст темы
        if (_themeStatusText != null)
            _themeStatusText.text = _isDarkTheme ? "Dark" : "Light";

        // Обновляем картинку темы
        if (_themeIconImage != null)
        {
            _themeIconImage.sprite = _isDarkTheme ? _darkThemeSprite : _lightThemeSprite;
            _themeIconImage.color = _isDarkTheme ? new Color(0.58f, 0.61f, 0.95f) : new Color(0.91f, 0.78f, 0.45f);

        }

        // Обновляем текст звука
        if (_soundStatusText != null)
            _soundStatusText.text = _isSoundOn ? "On" : "Off";
    }

    private void OpenPrivacyPolicy()
    {
        Application.OpenURL("https://docs.google.com/document/d/1zeeJ2kKRbaKkHCSyXlcAdQ6rp8651tCChpjEkYAQgHU/edit?usp=sharing");
    }

    private void BuyRemoveAds()
    {
        Debug.Log("Initiating IAP: Remove Ads...");
    }

    private void ContactUs()
    {
        string email = "support@uniteditforce.com";
        string subject = EscapeURL($"Support: Brain Games v{Application.version}");
        string body = EscapeURL("Please describe your problem here:\n\n");

        Application.OpenURL($"mailto:{email}?subject={subject}&body={body}");
    }

    private string EscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }
}