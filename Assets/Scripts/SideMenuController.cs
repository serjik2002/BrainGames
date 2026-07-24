using UnityEngine;
using UnityEngine.UI;
using TMPro; // Обязательно подключаем, так как скорее всего используется TextMeshPro
using System.Collections;

public class SideMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Сама выезжающая панель")]
    [SerializeField] private RectTransform _sidePanel;

    [Tooltip("Объект с затемнением фона (нужен компонент CanvasGroup)")]
    [SerializeField] private CanvasGroup _backgroundDim;

    [Tooltip("Текст копирайта внизу панели")]
    [SerializeField] private TMP_Text _versionText; // Если используешь обычный Text, замени TMP_Text на Text

    [Header("Buttons")]
    [SerializeField] private Button _privacyPolicyButton;
    [SerializeField] private Button _removeAdsButton;
    [Tooltip("Кнопка на фоне затемнения для закрытия меню")]
    [SerializeField] private Button _backgroundCloseButton;

    [Header("Settings")]
    [SerializeField] private float _animationDuration = 0.3f;

    private float _hiddenPosX;
    private bool _isOpen = false;
    private Coroutine _animationCoroutine;

    private void Start()
    {
        // 1. Инициализация текста копирайта и версии
        if (_versionText != null)
        {
            // Беремо версію з Project Settings і форматуємо текст
            _versionText.text = $"(c) UnitedITForce\nv {Application.version}";
        }

        // 2. Настройка начальных позиций и состояний
        _hiddenPosX = -_sidePanel.rect.width;
        _sidePanel.anchoredPosition = new Vector2(_hiddenPosX, _sidePanel.anchoredPosition.y);

        _backgroundDim.alpha = 0f;
        _backgroundDim.blocksRaycasts = false; // Отключаем клики по фону, пока меню закрыто

        // 3. Подписка на события кнопок
        if (_privacyPolicyButton != null)
            _privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);

        if (_removeAdsButton != null)
            _removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);

        if (_backgroundCloseButton != null)
            _backgroundCloseButton.onClick.AddListener(CloseMenu);
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта во избежание утечек памяти
        if (_privacyPolicyButton != null) _privacyPolicyButton.onClick.RemoveAllListeners();
        if (_removeAdsButton != null) _removeAdsButton.onClick.RemoveAllListeners();
        if (_backgroundCloseButton != null) _backgroundCloseButton.onClick.RemoveAllListeners();
    }

    public void ToggleMenu()
    {
        if (_isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (_isOpen) return;
        _isOpen = true;

        _backgroundDim.blocksRaycasts = true; // Включаем перехват кликов
        StartMenuAnimation(0f, 1f); // Едем на X = 0, альфа фона = 1
    }

    public void CloseMenu()
    {
        if (!_isOpen) return;
        _isOpen = false;

        _backgroundDim.blocksRaycasts = false; // Отключаем перехват кликов
        StartMenuAnimation(_hiddenPosX, 0f); // Едем на спрятанную позицию, альфа фона = 0
    }

    private void StartMenuAnimation(float targetX, float targetAlpha)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
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
            // Плавное ускорение и замедление (SmoothStep)
            t = t * t * (3f - 2f * t);

            _sidePanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            _backgroundDim.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Фиксируем конечные значения
        _sidePanel.anchoredPosition = targetPos;
        _backgroundDim.alpha = targetAlpha;
    }

    // --- Методы для кнопок меню ---

    private void OnPrivacyPolicyClicked()
    {
        Debug.Log("Privacy Policy button clicked");
        // Здесь код для открытия ссылки или окна политики
        // Application.OpenURL("https://твоя-ссылка.com/privacy");
    }

    private void OnRemoveAdsClicked()
    {
        Debug.Log("Remove Ads button clicked");
        // Здесь код вызова покупки (IAP)
    }
}