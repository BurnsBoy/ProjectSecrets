using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public InputSystem_Actions controls;
    public PlayerInput input;
    public Player player;
    public Camera cam;
    public Animator animator;
    public Collider[] buttonColliders;
    public TextMeshPro lookSensitivityText;
    int lookSensitivitySetting;
    int buttonIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookSensitivitySetting = 5;
        controls = new InputSystem_Actions();
        SetSensitivity();
    }
    void Update()
    {
        if (input.enabled)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10))
            {
                for (int i = 0; i < buttonColliders.Length; i++)
                {
                    if (hit.collider ==  buttonColliders[i])
                        buttonIndex = i;
                }
                animator.SetInteger("ButtonIndex", buttonIndex);
            }
        }
    }
    public void OnResume()
    {
        player.OnPause();
    }
    public void OnContinueHover()
    {
        buttonIndex = 0;
        animator.SetInteger("ButtonIndex", buttonIndex);
    }
    public void OnQuitHover()
    {
        buttonIndex = 1;
        animator.SetInteger("ButtonIndex", buttonIndex);
    }
    public void OnDecreaseSensitivity()
    {
        if (lookSensitivitySetting > 1)
            lookSensitivitySetting--;
        lookSensitivityText.text = lookSensitivitySetting.ToString();
        SetSensitivity();
    }
    public void OnIncreaseSensitivity()
    {
        if (lookSensitivitySetting < 10)
            lookSensitivitySetting++;
        lookSensitivityText.text = lookSensitivitySetting.ToString();
        SetSensitivity();
    }

    void SetSensitivity()
    {
        player.lookSensitivity = player.minlookSensitivity +
           (lookSensitivitySetting - 1) * ((player.maxlookSensitivity - player.minlookSensitivity) / 10);
    }
    public void OnEnter()
    {
        if (buttonIndex == 0)
        {
            player.OnPause();
        }
        else if (buttonIndex == 2)
        {
            OnDecreaseSensitivity();
        }
        else if (buttonIndex == 3)
        {
            OnIncreaseSensitivity();
        }
        else
        {
            SceneManager.LoadScene("Restart");
        }
    }
}
