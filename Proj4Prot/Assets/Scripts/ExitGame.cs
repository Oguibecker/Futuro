using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management functions

public class SceneChangerTrigger : MonoBehaviour
{
    public string targetSceneName = "YourNextSceneName";

    public string playerTag = "Player";

    void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}