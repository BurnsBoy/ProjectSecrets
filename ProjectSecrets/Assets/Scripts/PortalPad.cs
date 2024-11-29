using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalPad : MonoBehaviour
{
    public InputSystem_Actions controls;
    public Animator animator;
    public Animator handAnimator;
    public Animator cameraAnimator;
    public Animator cardAnimator;
    public PlayerInput input;
    public Player player;
    public Dictionary<int, string> portalDestinations;
    bool[] buttonsPressed = new bool[9];
    public List<Puzzle> puzzles;
    public int puzzleIndex;
    public MeshRenderer[] columnLights;
    public MeshRenderer[] rowLights;
    public MeshRenderer[] correctLights;
    public Material unlitPuzzleLight;
    public Material litPuzzleLight;
    public MeshRenderer[] cardLights;
    public Material unlitCardLight;
    public Material litCardLight;
    public Camera cam;
    public Transform selectionCursor;
    public Transform[] buttonColliders;
    public PlayerAudio playerAudio;
    public CameraFades cameraFades;
    public TextMeshPro cardText;
    public GameObject[] arrowButtons;
    public GameObject helpMenu;
    public TextMeshProUGUI helpText;
    int selectedButtonIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controls = new InputSystem_Actions();
        animator.speed = 2;
        InstantiateDestinations();
        SceneManager.LoadScene(portalDestinations[141]);
        selectedButtonIndex = 4;
    }

    void Update()
    {
        selectionCursor.localScale = Vector3.one;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 10))
        {
            selectionCursor.localScale = Vector3.one;
            for (int i = 0; i < buttonColliders.Length; i++)
            {
                if (hit.collider.transform == buttonColliders[i])
                {
                    selectedButtonIndex = i;
                    selectionCursor.position = buttonColliders[i].position;
                }
            }
        }
        else if (Gamepad.all.Count == 0)
        {
            selectionCursor.localScale = Vector3.zero;
        }
        else
        {
            int verticalSelection = (int)Math.Round(controls.PortalPad.Point.ReadValue<Vector2>().y);
            int horizontalSelection = (int)Math.Round(controls.PortalPad.Point.ReadValue<Vector2>().x);
            selectedButtonIndex = (verticalSelection + 1) * 3 + horizontalSelection + 1;
            selectionCursor.position = buttonColliders[selectedButtonIndex].position;
        }
    }

    void InstantiateDestinations()
    {
        portalDestinations = new Dictionary<int, string>()
        {
            { 0, "Prototype" },
            { 141, "Realm of Darkness" },
            { 76, "Plains" },
            { 423, "Jetpack" },
            { 285, "Sky" },
            { 227, "Grapple Hook" },
            { 30, "Volcano" },
            { 346, "MagShoes" },
            { 425, "Monolith" },
        };
        int homeAddress = 0;
        do
        {
            homeAddress = UnityEngine.Random.Range(0, 512);
        }
        while (portalDestinations.ContainsKey(homeAddress));
        portalDestinations[homeAddress] = "Home";
    }

    //public void SetControls()

    void PressButton(int btn)
    {
        playerAudio.PadClick();
        buttonsPressed[btn - 1] = !buttonsPressed[btn - 1];
        animator.SetBool(btn.ToString(), buttonsPressed[btn - 1]);
        handAnimator.SetBool(btn.ToString(), buttonsPressed[btn - 1]);
        if (puzzles.Count > 0 && !puzzles[puzzleIndex].solved)
            CheckPuzzle();
    }

    void CheckPuzzle()
    {
        Puzzle puzzle = puzzles[puzzleIndex];
        bool showTotal = true;
        int totalCorrect = 0;
        bool[,] padMatrix = TranslateIntToMatrix(TranslateCodeToInt());
        for (int x = 0; x < 3; x++)
        {
            int btnCount = 0;
            for (int y = 0; y < 3; y++)
            {
                if (padMatrix[x, y])
                    btnCount++;
                if (padMatrix[x, y] == puzzle.matrix[x,y])
                {
                    totalCorrect++;
                }
            }
            if (btnCount == puzzle.columnCount[x])
            {
                if (columnLights[x].material.name == "PuzzleLight (Instance)")
                {
                    columnLights[x].material = litPuzzleLight;
                }
            }
            else
            {
                columnLights[x].material = unlitPuzzleLight;
                showTotal = false;
            }
        }
        for (int y = 0; y < 3; y++)
        {
            int btnCount = 0;
            for (int x = 0; x < 3; x++)
            {
                if (padMatrix[x, y])
                    btnCount++;
            }
            if (btnCount == puzzle.rowCount[y])
            {
                if (rowLights[y].material.name == "PuzzleLight (Instance)")
                {
                    rowLights[y].material = litPuzzleLight;
                }
            }
            else 
            {
                rowLights[y].material = unlitPuzzleLight;
                showTotal = false;    
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (showTotal && i < totalCorrect)
            {
                correctLights[i].material = litPuzzleLight;
            }
            else
                correctLights[i].material = unlitPuzzleLight;
        }
    }

    public void OnReturn()
    {
        player.SetControls("Player");
    }

    public void OnEnter()
    {
        playerAudio.Teleport();
        cameraAnimator.SetBool("MenuOpen", false);
        if (!cameraFades.fading)
            cameraFades.FadeWhite();
    }

    public void Teleport()
    {
        player.teleports++;
        player.attracted = false;
        player.teleporting = true;
        player.recoverPoint.parent = player.playerDependencies.transform;
        int address = TranslateCodeToInt();
        if (puzzles.Count > 0 && address == puzzles[puzzleIndex].answer)
        {
            puzzles[puzzleIndex].solved = true;
            StartCoroutine(HandlePuzzleAnimation());
        }
        try
        {
            SceneManager.LoadScene(portalDestinations[address]);
            player.sceneName = portalDestinations[address];
        }
        catch
        {
            SceneManager.LoadScene("DefaultScene");
            player.sceneName = "DefaultScene";
        }
        player.playerBody.Sleep();
    }

    public bool[,] TranslateIntToMatrix(int num)
    {
        bool[,] matrix = new bool[3, 3];
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int toPow = y * 3 + x + 1;
                int remainder = num % (int)Mathf.Pow(2, toPow);
                if (remainder != 0)
                {
                    num -= remainder;
                    matrix[x, y] = true;
                }
            }
        }
        return matrix;
    }

    public int TranslateCodeToInt()
    {
        int value = 0;
        for (int i = 0; i < 9; i++)
        {
            if (buttonsPressed[i])
            {
                value += (int)Mathf.Pow(2, i);
            }
        }
        return value;
    }

    public void OnNext()
    {
        if (puzzles.Count > 1)
        {
            puzzleIndex++;
            if (puzzleIndex >= puzzles.Count)
                puzzleIndex = 0;
            StartCoroutine(HandlePuzzleAnimation());
        }
    }
    public void OnPrevious()
    {
        if (puzzles.Count > 1)
        {
            puzzleIndex--;
            if (puzzleIndex < 0)
                puzzleIndex = puzzles.Count - 1;
            StartCoroutine(HandlePuzzleAnimation());
        }
    }

    public IEnumerator<WaitForSeconds> HandlePuzzleAnimation()
    {
        animator.SetBool("UsingPuzzle", false);
        cardAnimator.SetBool("HoldingCard", false);

        yield return new WaitForSeconds(.5f);

        if (puzzles[puzzleIndex].solved)
        {
            SetCardMaterials();
            cardAnimator.SetBool("HoldingCard", true);
        }
        else
        {
            animator.SetBool("UsingPuzzle", true);
        }
        if (!puzzles[puzzleIndex].solved)
            CheckPuzzle();
        else
        {
            foreach (MeshRenderer light in columnLights)
            {
                light.material = unlitPuzzleLight;
            }
            foreach (MeshRenderer light in rowLights)
            {
                light.material = unlitPuzzleLight;
            }
        }
    }

    void SetCardMaterials()
    {
        cardText.text = portalDestinations[puzzles[puzzleIndex].answer];
        for (int i = 0; i < 9; i++)
        {
            if (puzzles[puzzleIndex].matrix[i % 3, i / 3])
                cardLights[i].material = litCardLight;
            else
                cardLights[i].material = unlitCardLight;
        }
    }

    public void ShowArrowButtons()
    {
        if (!arrowButtons[0].activeInHierarchy && puzzles.Count > 1)
        {
            arrowButtons[0].SetActive(true);
            arrowButtons[1].SetActive(true);
        }
    }

    public void OnHelp()
    {
        helpMenu.SetActive(!helpMenu.activeInHierarchy);
        if (helpMenu.activeInHierarchy)
        {
            helpText.text = "Hide Help";
        }
        else
        {
            helpText.text = "Help";
        }
    }

    public void OnSelect()
    {
        switch(selectedButtonIndex)
        {
            case 0: On_1(); break;
            case 1: On_2(); break;
            case 2: On_3(); break;
            case 3: On_4(); break;
            case 4: On_5(); break;
            case 5: On_6(); break;
            case 6: On_7(); break;
            case 7: On_8(); break;
            case 8: On_9(); break;
        }
    }

    public void On_1()
    {
        PressButton(1);
    }
    public void On_2()
    {
        PressButton(2);
    }
    public void On_3()
    {
        PressButton(3);
    }
    public void On_4()
    {
        PressButton(4);
    }
    public void On_5()
    {
        PressButton(5);
    }
    public void On_6()
    {
        PressButton(6);
    }
    public void On_7()
    {
        PressButton(7);
    }
    public void On_8()
    {
        PressButton(8);
    }
    public void On_9()
    {
        PressButton(9);
    }
}
