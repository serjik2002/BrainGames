using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDebugger : MonoBehaviour
{
    void Update()
    {
        // Проверяем клик левой кнопкой мыши или тап по экрану
        if (Input.GetMouseButtonDown(0))
        {
            // Создаем событие указателя (мыши/пальца)
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            // Создаем список для результатов
            List<RaycastResult> results = new List<RaycastResult>();

            // Пускаем луч через весь UI
            EventSystem.current.RaycastAll(pointerData, results);

            // Если луч во что-то попал
            if (results.Count > 0)
            {
                // results[0] — это самый верхний элемент, который перехватил клик
                Debug.Log("<color=green>Клик пойман элементом:</color> " + results[0].gameObject.name, results[0].gameObject);
            }
            else
            {
                Debug.Log("<color=yellow>Клик в пустоту (UI не задет)</color>");
            }
        }
    }
}