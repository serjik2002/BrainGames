using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string _gameSceneName = "Game";

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);

        var op = SceneManager.LoadSceneAsync(_gameSceneName, LoadSceneMode.Single);
        yield return new WaitForSeconds(2f);
        while (!op.isDone) yield return null;

    }
}
