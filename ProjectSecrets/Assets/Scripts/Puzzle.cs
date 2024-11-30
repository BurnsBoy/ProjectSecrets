using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public int answer;
    public bool[,] matrix;
    public int[] columnCount;
    public int[] rowCount;
    public Collider trigger;
    public Animator animator;
    public GameObject pickupObject;
    public bool solved;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<GameObject> puzzlesInScene = new List<GameObject>();
        puzzlesInScene.AddRange(GameObject.FindGameObjectsWithTag("Puzzle"));
        foreach(GameObject puzzle in puzzlesInScene)
        {
            if (puzzle.GetComponent<Puzzle>() != this && puzzle.GetComponent<Puzzle>().answer == answer)
                Destroy(gameObject);
        }

        //answer = Random.Range(0, 512);
        matrix = new bool[3, 3];
        columnCount = new int[3];
        rowCount = new int[3];
        TranslateIntToMatrix();
        SetCounts();
    }
    void TranslateIntToMatrix()
    {
        int num = answer;
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
    }

    void SetCounts()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (matrix[x, y])
                    columnCount[x]++;
            }
        }
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (matrix[x, y])
                    rowCount[y]++;
            }
        }
    }

    public void Pickup(PortalPad portalPad)
    {
        portalPad.puzzles.Add(this);
        portalPad.puzzleIndex = portalPad.puzzles.Count - 1;
        StartCoroutine(portalPad.HandlePuzzleAnimation());
        trigger.enabled = false;
        animator.enabled = false;
        Destroy(pickupObject);
        transform.parent = portalPad.transform;
        portalPad.ShowArrowButtons();
        portalPad.player.padHelp.SetActive(true);
    }
}
