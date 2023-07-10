using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum PuzzleType
{
    Empty, Basic, Horizontal, Vertical, Cross, Square
}

public enum PuzzleColor
{
    Blue, Green, Red, Purple
}


public class PuzzleManager : MonoBehaviour
{

    public PuzzleColor concept;

    public int x;
    public int y;

    public Text testText;

    public RectTransform frameRect;
    public int pSpacing;
    public GameObject puzzlePrefab;
    public GameObject puzzleBackPrefab;

    public Sprite[] puzzleSprs;
    public Sprite[] puzzleBackSprs;
    public Sprite[] frameSprs;

    public Vector2 puzzleSize;

    public Puzzle[,] puzzles;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        frameRect.sizeDelta = new Vector2(puzzleSize.x * x + pSpacing, puzzleSize.y * y + pSpacing);
        frameRect.GetComponent<Image>().sprite = frameSprs[(int)concept];

        puzzles = new Puzzle[x, y];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Instantiate(puzzleBackPrefab, frameRect.transform).GetComponent<PuzzleBackGround>().Init(GetPos(i, j), puzzleBackSprs[(int)concept]);
                //puzzles[i,j] = MakeNewPuzzle(i, j);
            }

        }

        Fill();

    }

    public void Fill()
    {
        StartCoroutine(FillCor());

    }


    IEnumerator FillCor()
    {
       
        bool needFill = true;

        while (needFill)
        {
            while (FillRoutine())
            {
                Debug.Log("필루틴 실행");

                yield return new WaitForSeconds(0.1f);
            }

            needFill = CheckPuzzle();
            yield return null;
        }
       
    }


    public bool FillRoutine()
    {
        bool isBlockMove = false;

        for (int i = 0; i < x; i++)
        {
            for (int j = y - 2; j >=0; j--) //아래에서 부터 위로 훑고 올라감
            {

                if (puzzles[i, j] == null || puzzles[i, j].type == PuzzleType.Empty) continue;

                Puzzle belowPuzzle = puzzles[i, j + 1];

                if (belowPuzzle == null || belowPuzzle.type == PuzzleType.Empty) //무언가 없다면 내려야함
                {
                    Puzzle curPuzzle = puzzles[i, j];

                    curPuzzle.Move(i, j+1, 0.1f);
                    puzzles[i, j] = null;
                    puzzles[i, j + 1] = curPuzzle;

                    curPuzzle.x = i;
                    curPuzzle.y = j + 1;

                    isBlockMove = true;
                }


            }
        }


        for (int i = 0; i < x; i++)
        {
            if (puzzles[i, 0] == null || puzzles[i, 0].type == PuzzleType.Empty)
            {
                Puzzle newPuzzle = MakeNewPuzzle(i, -1);
                newPuzzle.Move(i, 0,0.1f);

                puzzles[i, 0] = newPuzzle;

                newPuzzle.x = i;
                newPuzzle.y = 0;

                isBlockMove = true;
            }
        }



        return isBlockMove;
    }


    //퍼즐 생성
    public Puzzle MakeNewPuzzle(int x, int y)
    {
        Puzzle newPuzzle = Instantiate(puzzlePrefab, frameRect.transform).GetComponent<Puzzle>();
        newPuzzle.Init(x, y, this);

        return newPuzzle;
    }


    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + pSpacing / 2, -y * puzzleSize.y - pSpacing / 2);
    }



    
    //시계방향으로 검사
    private int[] dx = new int[] {0,1,0,-1};
    private int[] dy = new int[] {-1,0,1,0};

    
    //붙어있는 퍼즐 체크
    public bool CheckPuzzle()
    {
        bool isDestroyBlock = false;

        Queue<Puzzle> puzzleQueue = new Queue<Puzzle>();
        List<Puzzle> destoryPuzzle = new List<Puzzle>();
        List<Puzzle> visitPuzzle = new List<Puzzle>();

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (visitPuzzle.Contains(puzzles[i, j])) continue;

                puzzleQueue.Enqueue(puzzles[i, j]);


                while (!puzzleQueue.Count.Equals(0))
                {

                    Puzzle curPuzzle = puzzleQueue.Dequeue();
                    visitPuzzle.Add(curPuzzle);
                    List<Puzzle> tempDestroyPuzzle = new List<Puzzle>();

                    for (int dir = 0; dir < 4; dir++)
                    {
                        int newX = curPuzzle.x + dx[dir]; //세로
                        int newY = curPuzzle.y + dy[dir]; //가로

                        if (newX < 0 || newY < 0 || newX >= x || newY >= y) continue;
                        if (visitPuzzle.Contains(puzzles[newX, newY])) continue;

                        if (curPuzzle.color == puzzles[newX, newY].color)
                        {
                            puzzleQueue.Enqueue(puzzles[newX, newY]);
                        }

                        do
                        {
                            if (puzzles[newX, newY].color == curPuzzle.color)
                            {
                                tempDestroyPuzzle.Add(puzzles[newX, newY]);
                            }
                            else
                            {
                                break;
                            }

                            switch (dir)
                            {
                                //위
                                case 0:
                                    newY--;
                                    break;

                                //오른쪽
                                case 1:
                                    newX++;
                                    break;

                                //아래
                                case 2:
                                    newY++;
                                    break;

                                //왼쪽
                                case 3:
                                    newX--;
                                    break;
                            }

                            if (newX < 0 || newY < 0 || newX >= x || newY >= y) break;
                            if (visitPuzzle.Contains(puzzles[newX, newY])) break;
                        }
                        while (true);


                        if (tempDestroyPuzzle.Count >= 2)
                        {
                            if (!destoryPuzzle.Contains(curPuzzle))
                            {
                                destoryPuzzle.Add(curPuzzle);
                            }

                            destoryPuzzle.AddRange(tempDestroyPuzzle);
                        }

                        tempDestroyPuzzle.Clear();

                    }//4방향 for문 종료

                }
                //while문 종료

                if(destoryPuzzle.Count >= 1)
                {
                    isDestroyBlock = true;
                }


                foreach(Puzzle puzzle in destoryPuzzle)
                {

                    puzzle.type = PuzzleType.Empty;
                    Destroy(puzzle.gameObject);
                    
                }

                destoryPuzzle.Clear();


            }
        }

        return isDestroyBlock;
    }
    

}
