using System;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [System.Serializable]
    public struct NotificationConfig
    {
        [Tooltip("Через сколько часов отправить (можно использовать дроби, например 0.5 для получаса)")]
        public float DelayHours;
        public string Title;
        public string Text;
    }

    [Header("Настройки каскадных уведомлений")]
    [SerializeField]
    private NotificationConfig[] _notifications = new NotificationConfig[]
    {
        // 2 часа
        new NotificationConfig { DelayHours = 2f, Title = "Brain Workout \ud83e\udde0", Text = "The arrows are waiting! Ready to beat a few levels?" },
        // 24 часа
        new NotificationConfig { DelayHours = 24f, Title = "The arrows miss you", Text = "Come back to the game, these new levels won't solve themselves!" },
        // 3 дня (72 часа)
        new NotificationConfig { DelayHours = 72f, Title = "Long time no see!", Text = "It's been 3 days. Jump back in and solve a few more levels!" },
        // 7 дней (168 часов)
        new NotificationConfig { DelayHours = 168f, Title = "We're still waiting!", Text = "A whole week has passed! Jump back in to untangle new puzzles." }
    };

    private const string CHANNEL_ID = "game_reminders_channel";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNotifications();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeNotifications()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = CHANNEL_ID,
            Name = "Напоминания об игре",
            Importance = Importance.Default,
            Description = "Напоминает вернуться в игру через разные промежутки времени"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        if (AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed)
        {
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#endif

#if UNITY_IOS
        StartCoroutine(RequestAuthorizationIOS());
#endif

        // При старте игры сразу отменяем все запланированные пуши
        CancelAllNotifications();
    }

#if UNITY_IOS
    private System.Collections.IEnumerator RequestAuthorizationIOS()
    {
        using (var req = new AuthorizationOption[] { AuthorizationOption.Alert, AuthorizationOption.Badge, AuthorizationOption.Sound })
        {
            var request = new iOSNotificationAuthorizationRequest(req);
            while (!request.IsFinished)
            {
                yield return null;
            }
        }
    }
#endif

    /// <summary>
    /// Планирует всю цепочку уведомлений
    /// </summary>
    private void ScheduleNotifications()
    {
        CancelAllNotifications(); // Очищаем старые перед планированием новых

        for (int i = 0; i < _notifications.Length; i++)
        {
            var config = _notifications[i];
            DateTime fireTime = DateTime.Now.AddHours(config.DelayHours);

#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = config.Title,
                Text = config.Text,
                FireTime = fireTime,
                SmallIcon = "icon_0",
                LargeIcon = "icon_1"
            };
            AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
#endif

#if UNITY_IOS
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                // Конвертируем часы во флоат, чтобы поддерживать дробные значения (например, 1.5 часа)
                TimeInterval = TimeSpan.FromHours(config.DelayHours),
                Repeats = false
            };

            var notificationiOS = new iOSNotification()
            {
                // ВАЖНО: У каждого пуша на iOS должен быть уникальный ID, иначе они перезапишут друг друга
                Identifier = $"reminder_notification_{i}",
                Title = config.Title,
                Body = config.Text,
                ShowInForeground = false, // Не показываем пуш, если игра открыта
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "retention_category",
                ThreadIdentifier = "retention_thread",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notificationiOS);
#endif
            Debug.Log($"[Notifications] Запланировано уведомление '{config.Title}' на {fireTime}");
        }
    }

    private void CancelAllNotifications()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#endif

#if UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
#endif
        Debug.Log("[Notifications] Все уведомления отменены.");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ScheduleNotifications(); // Запускаем весь каскад
        }
        else
        {
            CancelAllNotifications(); // Игрок вернулся — отменяем весь каскад
        }
    }

    private void OnApplicationQuit()
    {
        ScheduleNotifications();
    }
}