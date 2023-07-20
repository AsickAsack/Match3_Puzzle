using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public int x;
    public int y;

    public GameObject backGrounds;
    public GameObject[] prefabs;

    public Sprite[] normalPuzzleSprites;
    public Sprite[] horizontalPuzzleSprites;
    public Sprite[] verticalPuzzleSprites;

    public PuzzleColor gameTheme;

    private Vector2 puzzleSize = new Vector2(128, 144);
    private int puzzleSpacing = 10;

    private List<GameObject> backList = new List<GameObject>();

    //퍼즐 생성
    public Puzzle MakeNewPuzzleInGame(int x,int y,PuzzleType type,PuzzleColor color)
    {
        Transform frame = GameObject.Find("Frame").transform;
        Puzzle newPuzzle = Instantiate(prefabs[(int)type], frame).GetComponent<Puzzle>();
        newPuzzle.SetPos(GetPos(x, y));

        switch (type)
        {
            case PuzzleType.Normal:
                newPuzzle.GetComponent<Image>().sprite = normalPuzzleSprites[(int)color];
                newPuzzle.color = color;
                break;
            case PuzzleType.Horizontal:
                newPuzzle.GetComponent<Image>().sprite = horizontalPuzzleSprites[(int)color];
                newPuzzle.color = color;
                break;
            case PuzzleType.Vertical:
                newPuzzle.GetComponent<Image>().sprite = verticalPuzzleSprites[(int)color];
                newPuzzle.color = color;
                break;
        }

        return newPuzzle;
    }


    //프레임 생성
    public void MakeFrames()
    {
        Transform frame = GameObject.Find("Frame").transform;
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2((puzzleSize.x * x) + puzzleSpacing, (puzzleSize.y * y) + puzzleSpacing);

        foreach (GameObject g in backList)
        {
            DestroyImmediate(g.gameObject);
        }

        backList.Clear();

        //퍼즐 배경 생성
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                backList.Add(Instantiate(backGrounds, frame));
                backList[^1].GetComponent<PuzzleBackGround>().Init(GetPos(i, j), backGrounds.GetComponent<Image>().sprite);
            }
        }
    }

    //좌표 위치 리턴
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + puzzleSpacing / 2, -y * puzzleSize.y - puzzleSpacing / 2);
    }

}
