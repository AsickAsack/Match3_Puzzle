using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    [SerializeField]
    private TextAsset puzzleData;

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

    private ObjectPool<Puzzle> puzzlePool;
    public ObjectPool<Puzzle> PuzzlePool => puzzlePool;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        //데이터 세팅
        if(puzzleData != null )
        {
            string[] data = puzzleData.text.Split("\n");

            gameTheme = (PuzzleColor)int.Parse(data[0]);
            this.x = int.Parse(data[1].Split("/")[0]);
            this.y = int.Parse(data[1].Split("/")[1]);

            manager.InitPuzzles(x, y);
            puzzlePool = new ObjectPool<Puzzle>(x * y, MakePoolPuzzle, PoolInitAction, PoolReturnAction);

            for (int j = 0; j < y; j++)
            {
                string[] puzzleData = data[j+2].Split("/");

                for (int i = 0; i < x; i++)
                {
                    manager.SetPuzzle(i, j, MakeNewPuzzle(i, j, (PuzzleType)int.Parse(puzzleData[i][0].ToString()), (PuzzleColor)int.Parse(puzzleData[i][1].ToString())));
                    
                }
            }
        }
        else
        {
            manager.InitPuzzles(x, y);
            puzzlePool = new ObjectPool<Puzzle>(x * y, MakePoolPuzzle, PoolInitAction, PoolReturnAction);
        }


        //외곽선 세팅
        puzzleBackFrame.sizeDelta = puzzleFrame.sizeDelta = new Vector2((puzzleSize.x * x) + puzzleSpacing, (puzzleSize.y * y) + puzzleSpacing);
        puzzleBackFrame.GetComponent<Image>().sprite = frameSprs[(int)gameTheme];

     


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
            Puzzle newInitPuzzle = MakeNewPuzzle(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y,initPuzzles[i].type);
            manager.SetPuzzle(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y,newInitPuzzle);
        }
        
        manager.Fill();
    }

    //새로운 퍼즐 생성
    public Puzzle MakeNewPuzzle(int x, int y, PuzzleType type, PuzzleColor color = PuzzleColor.None)
    {
        Puzzle newPuzzle = null;

        if(type == PuzzleType.Normal)
        {
            newPuzzle = puzzlePool.BorrowFromPool();
        }
        else
        {
           newPuzzle = Instantiate(puzzlePrefab[(int)type], puzzleFrame.transform).GetComponent<Puzzle>();
        }

        newPuzzle.Init(x, y, this.manager, color);

        return newPuzzle;
    }

    //좌표값에 맞는 퍼즐 위치 리턴
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + puzzleSpacing / 2, -y * puzzleSize.y - puzzleSpacing / 2);
    }



    #region 오브젝트 풀


    //풀 퍼즐 생성 
    public Puzzle MakePoolPuzzle()
    {
        Puzzle newPuzzle = Instantiate(puzzlePrefab[0], puzzleFrame.transform).GetComponent<Puzzle>();
        newPuzzle.gameObject.SetActive(false);

        return newPuzzle;
    }

    //풀 빌려주기 전 실행할 함수
    public void PoolInitAction(Puzzle puzzle)
    {
        puzzle.gameObject.SetActive(true);
    }

    //풀 돌려받을때 실행할 함수
    public void PoolReturnAction(Puzzle puzzle)
    {
        puzzle.gameObject.SetActive(false);
    }



    #endregion

}
