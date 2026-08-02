using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#pragma warning disable 0618

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;

    // Сюда будем передавать результат в ShopController
    private Action<bool, string> _onPurchaseCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Менеджер должен жить всегда
            InitializePurchasing();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // ВАЖНО: Добавляем ID всех твоих скинов. 
        // ProductType.NonConsumable означает, что товар покупается навсегда (не монетки)
        builder.AddProduct("com.uniteditforce.braingames.noads", ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        return _storeController != null && _extensionProvider != null;
    }

    /// <summary>
    /// Вызывается из ShopController при клике на кнопку цены
    /// </summary>
    public void BuyProduct(string productId, Action<bool, string> onPurchaseCompleted)
    {
        _onPurchaseCompleted = onPurchaseCompleted;

        if (IsInitialized())
        {
            Product product = _storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[IAP] Инициируем покупку: {product.definition.id}");
                _storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError("[IAP] Товар не найден или недоступен для покупки.");
                _onPurchaseCompleted?.Invoke(false, productId);
            }
        }
        else
        {
            Debug.LogError("[IAP] Магазин еще не инициализирован.");
            _onPurchaseCompleted?.Invoke(false, productId);
        }
    }

    // --- ДОДАЙ ЦІ МЕТОДИ ---

    /// <summary>
    /// Перевіряє, чи є у гравця підтверджений чек на цей товар від магазину
    /// </summary>
    public bool HasProduct(string productId)
    {
        if (!IsInitialized()) return false;

        Product product = _storeController.products.WithID(productId);
        // Якщо товар існує і має чек (receipt), значить він вже був куплений раніше
        return product != null && product.hasReceipt;
    }

    /// <summary>
    /// Ручне відновлення покупок (Обов'язкова вимога для App Store / iOS)
    /// </summary>
    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.LogError("[IAP] Магазин ще не ініціалізовано.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("[IAP] Починаємо відновлення покупок для Apple...");
            var apple = _extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, message) =>
            {
                Debug.Log($"[IAP] Відновлення завершено: success={result}, message={message}");
                // Якщо потрібно, тут можна додати Action callback для оновлення UI
            });
        }
        else
        {
            Debug.Log("[IAP] На Android покупки відновлюються автоматично при ініціалізації (через чеки).");
        }
    }


    /// <summary>
    /// Повертає локалізовану ціну зі стору (наприклад, "24,99 ?" або "$0.99").
    /// </summary>
    public string GetProductPrice(string productId)
    {
        if (!IsInitialized()) return ""; // Якщо магазин ще не ініціалізовано

        Product product = _storeController.products.WithID(productId);

        // Перевіряємо, чи існує такий товар і чи є у нього метадані
        if (product != null && product.metadata != null)
        {
            return product.metadata.localizedPriceString;
        }

        return "";
    }
    // =========================================================
    // МЕТОДЫ ИНТЕРФЕЙСА IStoreListener (Обработка ответов от магазина)
    // =========================================================

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAP] Успешно инициализировано!");
        _storeController = controller;
        _extensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAP] Ошибка инициализации: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAP] Ошибка инициализации: {error} - {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // Покупка прошла успешно!
        Debug.Log($"[IAP] Успешная покупка: {args.purchasedProduct.definition.id}");

        _onPurchaseCompleted?.Invoke(true, args.purchasedProduct.definition.id);

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"[IAP] Ошибка покупки {product.definition.id}: {failureReason}");
        _onPurchaseCompleted?.Invoke(false, product.definition.id);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"[IAP] Ошибка покупки {product.definition.id}: {failureDescription.message}");
        _onPurchaseCompleted?.Invoke(false, product.definition.id);
    }
}