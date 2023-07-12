using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMaker : MonoBehaviour
{
    //게임 테마(컬러)
    public PuzzleColor gameTheme;
    [SerializeField]
    private PuzzleManager manager;


    //퍼즐 크기
    public int x;
    public int y;

    public RectTransform frameRect;
    public int pSpacing;
    public GameObject puzzleBackPrefab;
    public GameObject[] puzzlePrefab;

    public Sprite[] puzzleSprs;
    public Sprite[] puzzleBackSprs;
    public Sprite[] frameSprs;

    public InitPuzzle[] initPuzzles;
    public Vector2 puzzleSize;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        frameRect.sizeDelta = new Vector2(puzzleSize.x * x + pSpacing, puzzleSize.y * y + pSpacing);
        frameRect.GetComponent<Image>().sprite = frameSprs[(int)gameTheme];





        manager.puzzles = new Puzzle[x, y];


        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Instantiate(puzzleBackPrefab, frameRect.transform).GetComponent<PuzzleBackGround>().Init(GetPos(i, j), puzzleBackSprs[(int)gameTheme]);
                //puzzles[i,j] = MakeNewPuzzle(i, j);
            }

        }


        for (int i = 0; i < initPuzzles.Length; i++)
        {
            GameObject temp = puzzlePrefab[(int)initPuzzles[i].type];



            Puzzle newObstacle = Instantiate(temp, frameRect.transform).GetComponent<Puzzle>();
            newObstacle.Init(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y, newObstacle.type, this.manager);


            manager.puzzles[initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y] = newObstacle;

        }

        manager.Fill();

    }


    //퍼즐 생성
    public Puzzle MakeNewPuzzle(int x, int y)
    {
        /*

        int rand = Random.Range(0, puzzleSprs.Length);
        Puzzle newPuzzle = Instantiate(puzzlePrefab[rand], frameRect.transform).GetComponent<Puzzle>();
        newPuzzle.Init(x, y, PuzzleType.Normal, this);*/

        Puzzle newPuzzle = Instantiate(puzzlePrefab[0], frameRect.transform).GetComponent<Puzzle>();
        newPuzzle.Init(x, y, this.manager);

        return newPuzzle;
    }


    //좌표값에 맞는 퍼즐 위치 리턴
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + pSpacing / 2, -y * puzzleSize.y - pSpacing / 2);
    }


}
