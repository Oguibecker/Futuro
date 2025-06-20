using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management functions
using System.Collections;

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
                StartCoroutine(goToMainMenu());
            }
        }
    }

    private IEnumerator goToMainMenu()
    {
        yield return new WaitForSeconds(9.5f);
        SceneManager.LoadScene(targetSceneName);
    }
}