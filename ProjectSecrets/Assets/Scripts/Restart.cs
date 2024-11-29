using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using UnityEngine.XR;

public class Restart : MonoBehaviour
{

    void Update()
    {
        Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player != null)
        {
            Time.timeScale = 1;
            Destroy(player.playerDependencies);
            SceneManager.LoadScene("StartScene");
        }
    }
}
