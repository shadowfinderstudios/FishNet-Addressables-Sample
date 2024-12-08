using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shadowfinder.Scene
{
    public class LoadInitialScenes : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
