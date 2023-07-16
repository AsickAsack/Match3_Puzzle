using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMaker : MonoBehaviour
{
    //게임 테마(컬러)
    [Header("[게임 테마]")]
    [SerializeField]
    private PuzzleColor gameTheme;

    [Header("[퍼즐 매니저]")]
    [SerializeField]
    private PuzzleManager manager;

    [Header("[퍼즐 세팅]")]
    //퍼즐 가로세로 크기
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;

    public int X => x;
    public int Y => y;

    [SerializeField]
    private int puzzleSpacing; //퍼즐 간격

    [SerializeField]
    private RectTransform puzzleBackFrame;
    [SerializeField]
    private RectTransform puzzleFrame;

    [Header("[프리펩]")]
    [SerializeField]
    private GameObject puzzleBackPrefab;
    [SerializeField]
    private GameObject[] puzzlePrefab;

    [Header("[퍼즐 스프라이트]")]
    public Sprite[] puzzleSprs;
    public Sprite[] verticalSprs;
    public Sprite[] horizontalSprs;
    public Sprite[] puzzleBackSprs;
    public Sprite[] frameSprs;

    [Header("[초기 퍼즐 생성]")]
    [SerializeField]
    private InitPuzzle[] initPuzzles;

    private Vector2 puzzleSize;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        //외곽선 세팅
        puzzleBackFrame.sizeDelta = puzzleFrame.sizeDelta = new Vector2((puzzleSize.x * x) + puzzleSpacing, (puzzleSize.y * y) + puzzleSpacing);
        puzzleBackFrame.GetComponent<Image>().sprite = frameSprs[(int)gameTheme];

        manager.InitPuzzles(x, y);

        //퍼즐 배경 생성
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Instantiate(puzzleBackPrefab, puzzleBackFrame.transform).GetComponent<PuzzleBackGround>().Init(GetPos(i, j), puzzleBackSprs[(int)gameTheme]);
            }
        }

        //초기 세팅 퍼즐이 있다면 생성
        for (int i = 0; i < initPuzzles.Length; i++)
        {
            Puzzle newInitPuzzle = Instantiate(puzzlePrefab[(int)initPuzzles[i].type], puzzleFrame.transform).GetComponent<Puzzle>();
            newInitPuzzle.Init(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y, this.manager);

            manager.SetPuzzle(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y,newInitPuzzle);
        }

        manager.Fill();
    }

    //새로운 퍼즐 생성
    public Puzzle MakeNewPuzzle(int x, int y, PuzzleType type, PuzzleColor color = PuzzleColor.None)
    {
        Puzzle newPuzzle = Instantiate(puzzlePrefab[(int)type], puzzleFrame.transform).GetComponent<Puzzle>();
        newPuzzle.Init(x, y, this.manager, color);

        return newPuzzle;
    }

    //좌표값에 맞는 퍼즐 위치 리턴
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + puzzleSpacing / 2, -y * puzzleSize.y - puzzleSpacing / 2);
    }

}
