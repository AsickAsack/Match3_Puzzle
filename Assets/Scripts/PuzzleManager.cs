using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



[System.Serializable]
public struct InitPuzzle
{
    public PuzzleType type;
    public Vector2Int coordinate;
}


public class PuzzleManager : MonoBehaviour
{
    //게임 테마(컬러)
    public PuzzleColor gameTheme;

    //퍼즐 크기
    public int x;
    public int y;

    public RectTransform frameRect;
    public int pSpacing;
    public GameObject[] puzzlePrefab;
    public GameObject puzzleBackPrefab;
    public GameObject obstacle;

    public Sprite[] puzzleSprs;
    public Sprite[] puzzleBackSprs;
    public Sprite[] frameSprs;

    public InitPuzzle[] initPuzzles;
    public Vector2 puzzleSize;

    public Puzzle[,] puzzles;

    private Puzzle curPuzzle;

    public bool isClick = false;
    public bool isProcess = true;


    public Puzzle CurPuzzle
    {
        get
        {
            return curPuzzle;
        }

        set
        {
            curPuzzle = value;
        }
    }



    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        frameRect.sizeDelta = new Vector2(puzzleSize.x * x + pSpacing, puzzleSize.y * y + pSpacing);
        frameRect.GetComponent<Image>().sprite = frameSprs[(int)gameTheme];





        puzzles = new Puzzle[x, y];


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
            Puzzle newObstacle = Instantiate(obstacle, frameRect.transform).GetComponent<Puzzle>();
            newObstacle.GetComponent<RectTransform>().anchoredPosition = GetPos(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y);

            puzzles[initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y] = newObstacle;

        }

        Fill();

    }

    //채우고 생성하는 함수
    public void Fill()
    {
        StartCoroutine(FillCor());
    }


    IEnumerator FillCor()
    {
        isProcess = true;
        bool needFill = true;

        while (needFill)
        {
            while (FillRoutine())
            {
                yield return new WaitForSeconds(0.1f);
            }

            needFill = CheckPuzzle();
            yield return null;
        }

        isProcess = false;
    }


    public bool FillRoutine()
    {
        bool isBlockMove = false;

        for (int i = 0; i < x; i++)
        {
            for (int j = y - 2; j >= 0; j--) //아래에서 부터 위로 훑고 올라감
            {

                if (puzzles[i, j] == null || puzzles[i, j].type == PuzzleType.Empty || puzzles[i, j].type == PuzzleType.Obstacle) continue;

                Puzzle curPuzzle = puzzles[i, j];
                Puzzle belowPuzzle = puzzles[i, j + 1];

                if (belowPuzzle == null || belowPuzzle.type == PuzzleType.Empty) //무언가 없다면 그냥 내림
                {

                    PuzzleChange(curPuzzle, i, j + 1);
                    isBlockMove = true;

                }
                else
                {

                    if (belowPuzzle.type == PuzzleType.Obstacle || CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))//아래가 장애물이라면 대각선 좌우 하단 탐색 
                    {
                        for (int diag = -1; diag <= 1; diag += 2)
                        {
                            if (i + diag < 0 || i + diag >= x) continue;

                            Puzzle newDiagPuzzle = puzzles[i + diag, j + 1];

                            if (newDiagPuzzle == null || newDiagPuzzle.type == PuzzleType.Empty)
                            {
                                PuzzleChange(curPuzzle, i + diag, j + 1);
                                isBlockMove = true;

                                break;
                            }
                        }

                    }


                }



            }
        }


        for (int i = 0; i < x; i++)
        {
            if (puzzles[i, 0] == null || puzzles[i, 0].type == PuzzleType.Empty)
            {
                Puzzle newPuzzle = MakeNewPuzzle(i, -1);

                newPuzzle.Move(i, 0, 0.1f);
                puzzles[i, 0] = newPuzzle;

                newPuzzle.SetCoordinate(i, 0);

                isBlockMove = true;
            }
        }

        return isBlockMove;
    }

    //퍼즐 이동 후 배열,x,y값 바꾸기
    void PuzzleChange(Puzzle curPuzzle, int newX, int newY)
    {
        curPuzzle.Move(newX, newY, 0.1f);
        puzzles[curPuzzle.x, curPuzzle.y] = null;
        puzzles[newX, newY] = curPuzzle;

        curPuzzle.SetCoordinate(newX, newY);
    }

    bool CheckIsObstacle(int x, int y)
    {
        //인덱스 범위 넘어가는지 체크
        if (x < 0 || y < 0 || x >= this.x || y >= this.y)
            return false;

        if (puzzles[x, y] == null || puzzles[x, y].type != PuzzleType.Obstacle)
            return false;

        return true;


    }


    //퍼즐 생성
    public Puzzle MakeNewPuzzle(int x, int y)
    {
        int rand = Random.Range(0, puzzleSprs.Length);
        Puzzle newPuzzle = Instantiate(puzzlePrefab[rand], frameRect.transform).GetComponent<Puzzle>();
        newPuzzle.Init(x, y, PuzzleType.Normal, this);

        return newPuzzle;
    }


    //좌표값에 맞는 퍼즐 위치 리턴
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + pSpacing / 2, -y * puzzleSize.y - pSpacing / 2);
    }




    //시계방향으로 검사
    private int[] dx = new int[] { 0, 1, 0, -1 };
    private int[] dy = new int[] { -1, 0, 1, 0 };


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
                if (visitPuzzle.Contains(puzzles[i, j]) || puzzles[i, j] == null) continue;

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

                if (destoryPuzzle.Count >= 1)
                {
                    isDestroyBlock = true;
                }


                foreach (Puzzle puzzle in destoryPuzzle)
                {
                    for (int dir = 0; dir < 4; dir++)
                    {
                        int newX = puzzle.x + dx[dir]; //세로
                        int newY = puzzle.y + dy[dir]; //가로

                        if(CheckIsObstacle(newX,newY))
                        {
                            puzzles[newX, newY].DestroyRoutine();
                        }
                    }


                     puzzle.DestroyRoutine();

                }

                destoryPuzzle.Clear();


            }
        }

        return isDestroyBlock;
    }


}
