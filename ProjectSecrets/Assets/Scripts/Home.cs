using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour
{
    bool didCheck;
    public PlayerInput playerInput;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI teleportText;
    public TextMeshProUGUI itemText;

    void Update()
    {
        if (!didCheck)
        {
            Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            if (player != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                timeText.text = TimeString(player.gameplayTime);
                damageText.text = DamageString(player.damageTaken);
                teleportText.text = player.teleports.ToString();
                itemText.text = ItemString(player);

                didCheck = true;
                Destroy(player.playerDependencies);

                playerInput.enabled = true;
            }
        }
    }

    string TimeString(float time)
    {
        int seconds = (int)time % 60;
        int minutes = (int)time / 60;
        return $"{minutes}:{seconds.ToString("D2")}";
    }

    string DamageString(float damage)
    {
        return $"{(int)damage} HP";
    }

    string ItemString(Player player)
    {
        int total = 0;

        if (player.hasJetpack)
            total++;
        if (player.hasGrappleHook)
            total++;
        if (player.hasMagShoes)
            total++;
        if (player.hasRedBattery)
            total++;
        if (player.hasGreenBattery)
            total++;
        if (player.hasBlueBattery)
            total++;

        return $"{total}/6";
    }

    public void OnEnter()
    {
        SceneManager.LoadScene("StartScene");
    }
}
