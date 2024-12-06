using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadInitialScenes : MonoBehaviour
{
    public void Load(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
