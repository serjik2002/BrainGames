//using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.Mediation.UnityAds;
using GoogleMobileAds.Ump.Api;
using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Ad Unit IDs")]
    [SerializeField] private string _interstitialAdId = "ca-app-pub-8968740975401720/3428102550";
    [SerializeField] private string _bannerAdId = "ca-app-pub-3940256099942544/6300978111";
    [SerializeField] private string _rewardedAdId = "ca-app-pub-8968740975401720/1102977103";

    [Header("Ad Free Window")]
    [SerializeField] private float adFreeDurationMinutes = 3f;
    private float _sessionStartTime;

    private bool _isInitialized;

    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;
    private RewardedAd _rewardedAd;

    // Retry/backoff
    private float _interstitialRetryDelay = 2f;
    private float _rewardedRetryDelay = 2f;
    private float _bannerRetryDelay = 2f;

    private const float RETRY_DELAY_MAX = 60f;

    private void Awake()
    {
        InitializeSingleton();
        _sessionStartTime = Time.realtimeSinceStartup;
    }

    private void Start()
    {
        // Вместо прямой инициализации сначала проверяем согласие UMP
        RequestAndLoadConsentForm();
        HideBannerAd();
    }

    private void InitializeSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private bool IsAdFreeWindowPassed()
    {
        // Считаем, сколько секунд прошло с момента запуска игры
        float passedSeconds = Time.realtimeSinceStartup - _sessionStartTime;
        return passedSeconds >= (adFreeDurationMinutes * 60f);
    }

    #region UMP Consent Flow

    private void RequestAndLoadConsentForm()
    {
        ConsentRequestParameters request = new ConsentRequestParameters();
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    private void OnConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            Debug.LogError("UMP Error: " + consentError.Message);
            TryInitializeAds();
            return;
        }

        ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
        {
            if (formError != null)
            {
                Debug.LogError("UMP Form Error: " + formError.Message);
            }

            if (ConsentInformation.CanRequestAds())
            {
                TryInitializeAds();
            }
        });
    }

    private void TryInitializeAds()
    {
        InitializeAds(() =>
        {
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    #endregion

    private void InitializeAds(Action onInitialized)
    {
        if (_isInitialized)
        {
            onInitialized?.Invoke();
            return;
        }

        // Берем реальный ответ пользователя из UMP
        bool hasConsent = ConsentInformation.ConsentStatus == GoogleMobileAds.Ump.Api.ConsentStatus.Obtained ||
                          ConsentInformation.ConsentStatus == GoogleMobileAds.Ump.Api.ConsentStatus.NotRequired;

        // Передача согласия для Европы (GDPR)
        GoogleMobileAds.Mediation.UnityAds.Api.UnityAds.SetConsentMetaData("gdpr.consent", hasConsent);

        // Передача согласия для жителей США (CCPA)
        GoogleMobileAds.Mediation.UnityAds.Api.UnityAds.SetConsentMetaData("privacy.consent", hasConsent);

        MobileAds.Initialize(_ =>
        {
            _isInitialized = true;
            Debug.Log("Google Mobile Ads SDK initialized.");
            onInitialized?.Invoke();
        });
    }

    private void OnDestroy()
    {
        DestroyBanner();
        DestroyInterstitial();
        DestroyRewarded();
    }

    #region Interstitial

    private void LoadInterstitialAd()
    {
        if (!_isInitialized) return;
        DestroyInterstitial();

        var request = new AdRequest();

        InterstitialAd.Load(_interstitialAdId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial load failed: " + error);
                float delay = _interstitialRetryDelay;
                _interstitialRetryDelay = Mathf.Min(_interstitialRetryDelay * 2f, RETRY_DELAY_MAX);
                CancelInvoke(nameof(LoadInterstitialAd));
                Invoke(nameof(LoadInterstitialAd), delay);
                return;
            }

            _interstitialRetryDelay = 2f;
            _interstitialAd = ad;

            // === ОБРАБОТЧИК ЗАКРЫТИЯ РЕКЛАМЫ ===
            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                // 1. Загружаем следующую рекламу на будущее (твой старый код)
                LoadInterstitialAd();
            };

            _interstitialAd.OnAdFullScreenContentFailed += (adError) =>
            {
                Debug.LogError("Interstitial show failed: " + adError);
                LoadInterstitialAd();
            };
        });
    }

    public bool CanShowInterstitial()
    {
        return _interstitialAd != null && _interstitialAd.CanShowAd() && IsAdFreeWindowPassed();
    }

    public void ShowInterstitialAd()
    {
        if (options.Options.GetBool("Advertisement.noads", false))
        {
            Debug.Log("[Ads] Рекламу не показано, бо її вимкнено покупкою.");
            return;
        }
        if (!IsAdFreeWindowPassed())
        {
            Debug.Log("Interstitial blocked by ad-free window.");
            return;
        }

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial not ready.");
            LoadInterstitialAd();
        }
    }

    private void DestroyInterstitial()
    {
        if (_interstitialAd == null) return;
        _interstitialAd.Destroy();
        _interstitialAd = null;
    }

    #endregion

    #region Banner

    public void LoadBannerAd()
    {
        if (!_isInitialized) return;

        DestroyBanner();

        _bannerView = new BannerView(_bannerAdId, AdSize.Banner, AdPosition.Bottom);

        // Если у твоей версии SDK есть эти события — отлично, если нет, просто убери блок.
        _bannerView.OnBannerAdLoadFailed += (error) =>
        {
            Debug.LogError("Banner load failed: " + error);

            float delay = _bannerRetryDelay;
            _bannerRetryDelay = Mathf.Min(_bannerRetryDelay * 2f, RETRY_DELAY_MAX);

            CancelInvoke(nameof(LoadBannerAd));
            Invoke(nameof(LoadBannerAd), delay);
        };

        _bannerView.OnBannerAdLoaded += () =>
        {
            _bannerRetryDelay = 2f;
        };

        var request = new AdRequest();
        _bannerView.LoadAd(request);
    }

    public void HideBannerAd()
    {
        _bannerView?.Hide();
    }

    public void ShowBannerAd()
    {
        if (options.Options.GetBool("Advertisement.noads", false))
        {
            Debug.Log("[Ads] Рекламу не показано, бо її вимкнено покупкою.");
            return;
        }
        _bannerView?.Show();
    }

    private void DestroyBanner()
    {
        if (_bannerView == null) return;
        _bannerView.Destroy();
        _bannerView = null;
    }

    #endregion

    #region Rewarded

    public void LoadRewardedAd()
    {
        if (!_isInitialized) return;

        DestroyRewarded();

        var request = new AdRequest();

        RewardedAd.Load(_rewardedAdId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded load failed: " + error);

                float delay = _rewardedRetryDelay;
                _rewardedRetryDelay = Mathf.Min(_rewardedRetryDelay * 2f, RETRY_DELAY_MAX);

                CancelInvoke(nameof(LoadRewardedAd));
                Invoke(nameof(LoadRewardedAd), delay);
                return;
            }

            _rewardedRetryDelay = 2f;
            _rewardedAd = ad;

            RegisterRewardedHandlers(_rewardedAd);
        });
    }

    private void RegisterRewardedHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded closed. Loading next.");
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (adError) =>
        {
            Debug.LogError("Rewarded show failed: " + adError);
            LoadRewardedAd();
        };
    }

    public bool CanShowRewarded()
    {
        return _rewardedAd != null && _rewardedAd.CanShowAd();
    }

    public void ShowRewardedAd(Action<Reward> onRewardEarned)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show(reward =>
            {
                Debug.Log($"Reward earned: {reward.Type} x{reward.Amount}");
                onRewardEarned?.Invoke(reward);
            });
        }
        else
        {
            Debug.Log("Rewarded not ready.");
            LoadRewardedAd();
        }
    }

    private void DestroyRewarded()
    {
        if (_rewardedAd == null) return;
        _rewardedAd.Destroy();
        _rewardedAd = null;
    }

    #endregion
}