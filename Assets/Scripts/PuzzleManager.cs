using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;





[System.Serializable]
public struct InitPuzzle
{
    public PuzzleType type;
    public Vector2Int coordinate;
}


public class PuzzleManager : MonoBehaviour
{


    [SerializeField]
    public PuzzleMaker maker;

    public Puzzle[,] puzzles;
    private Puzzle curPuzzle;

    public int X => maker.x;
    public int Y => maker.y;

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

    Coroutine fillco = null;

    //채우고 생성하는 함수
    public void Fill()
    {
        if (fillco == null)
        {
            fillco = StartCoroutine(FillCor());
        }

    }

    IEnumerator FillCor()
    {
        isProcess = true;
        bool needFill = true;

        while (needFill)
        {
            while (FillRoutine())
            {
                //내려오는 시간 0.1초 기다려줘야함
                yield return new WaitForSeconds(0.1f);
            }

            needFill = CheckPuzzle();
            yield return new WaitForSeconds(0.2f);
        }

        isProcess = false;
        fillco = null;

        FindMatchToUser();
    }


    public bool FillRoutine()
    {
        bool isBlockMove = false;

        for (int j = Y - 2; j >= 0; j--)
        {
            for (int i = 0; i < X; i++) //아래에서 부터 위로 훑고 올라감
            {

                if (puzzles[i, j] == null || !puzzles[i, j].IsMoveable()) continue;

                Puzzle curPuzzle = puzzles[i, j];
                Puzzle belowPuzzle = puzzles[i, j + 1];

                if (belowPuzzle == null) //무언가 없다면 그냥 내림
                {
                    PuzzleChange(curPuzzle, i, j + 1);
                    isBlockMove = true;
                }
                else
                {

                    if (/*belowPuzzle.type == PuzzleType.Obstacle ||*/ CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))//아래or옆이 장애물이라면 대각선 좌우 하단 탐색 
                    {


                        for (int diag = -1; diag <= 1; diag += 2)
                        {
                            if (i + diag < 0 || i + diag >= X) continue;

                            /*if (belowPuzzle.type == PuzzleType.Obstacle && puzzles[i + diag, j] != null) continue;*/

                            Puzzle newDiagPuzzle = puzzles[i + diag, j + 1];

                            if (newDiagPuzzle == null)
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


        //최상단 퍼즐 생성해줌
        for (int i = 0; i < X; i++)
        {
            if (puzzles[i, 0] == null)
            {
                Puzzle newPuzzle = maker.MakeNewPuzzle(i, -1, PuzzleType.Normal);

                newPuzzle.SetCoordinate(i, 0);
                newPuzzle.Move(0.1f);

                isBlockMove = true;
            }
        }

        return isBlockMove;
    }

    //퍼즐 이동 후 배열,x,y값 바꾸기
    void PuzzleChange(Puzzle curPuzzle, int newX, int newY)
    {
        puzzles[curPuzzle.x, curPuzzle.y] = null;
        curPuzzle.SetCoordinate(newX, newY);
        curPuzzle.Move(0.1f);
    }

    bool CheckIsObstacle(int x, int y)
    {
        //인덱스 범위 넘어가는지 체크
        if (x < 0 || y < 0 || x >= this.X || y >= this.Y)
            return false;

        if (puzzles[x, y] == null || puzzles[x, y].type != PuzzleType.Obstacle)
            return false;

        return true;
    }


    //시계방향으로 검사
    private int[] dx = new int[] { 0, 1, 0, -1 };
    private int[] dy = new int[] { -1, 0, 1, 0 };



    //붙어있는 퍼즐 체크
    public bool CheckPuzzle(int startX = 0, int startY = 0)
    {
        bool isDestroyBlock = false;

        List<Puzzle> rewardPuzzles = new List<Puzzle>();
        List<Puzzle> destroyPuzzles = new List<Puzzle>();
        Queue<Puzzle> searchQueue = new Queue<Puzzle>();

        for (int j = startY; j < Y; j++)
        {
            for (int i = startX; i < X; i++)
            {

                if (puzzles[i, j] == null || puzzles[i, j].color == PuzzleColor.None) continue;

                HashSet<Puzzle> visitPuzzles = new HashSet<Puzzle>();
                searchQueue.Enqueue(puzzles[i, j]);
                visitPuzzles.Add(puzzles[i, j]);

                PuzzleType rewardType = PuzzleType.Empty;

                while (searchQueue.Count != 0)
                {
                    Puzzle curPuzzle = searchQueue.Dequeue();

                    List<List<Puzzle>> find = new List<List<Puzzle>>();

                    List<Puzzle> left = new List<Puzzle>();
                    List<Puzzle> right = new List<Puzzle>();
                    List<Puzzle> up = new List<Puzzle>();
                    List<Puzzle> down = new List<Puzzle>();

                    find.Add(up);
                    find.Add(right);
                    find.Add(down);
                    find.Add(left);

                    //현재 퍼즐에서 동서남북 탐색
                    for (int k = 0; k < 4; k++)
                    {
                        int newX = curPuzzle.x + dx[k];
                        int newY = curPuzzle.y + dy[k];

                        do
                        {
                            if (newX < 0 || newY < 0 || newX >= X || newY >= Y) break;

                            if (puzzles[newX, newY] == null || curPuzzle.color != puzzles[newX, newY].color) break;

                            //이미 방문하지 않은 애라면
                            if (visitPuzzles.Add(puzzles[newX, newY]))
                            {
                                searchQueue.Enqueue(puzzles[newX, newY]);
                            }

                            if (!find[k].Contains(puzzles[newX, newY]) || destroyPuzzles.Contains(puzzles[newX, newY]))
                            {
                                find[k].Add(puzzles[newX, newY]);
                            }

                            newX += dx[k];
                            newY += dy[k];

                        } while (true);


                    }

                    if ((find[0].Count + find[1].Count + find[2].Count + find[3].Count) < 2) continue;

                    //레인보우 되는지 체크
                    if (rewardType != PuzzleType.Rainbow && ((find[0].Count + find[2].Count >= 4) || (find[1].Count + find[3].Count >= 4)))
                    {
                        Debug.Log("레입모우?");
                        rewardPuzzles.Clear();
                        rewardPuzzles.Add(curPuzzle);
                        rewardType = PuzzleType.Rainbow;

                        if (find[0].Count + find[2].Count >= 4)
                        {
                            rewardPuzzles.AddRange(find[0]);
                            rewardPuzzles.AddRange(find[2]);
                        }

                        if (find[1].Count + find[3].Count >= 4)
                        {
                            rewardPuzzles.AddRange(find[1]);
                            rewardPuzzles.AddRange(find[3]);
                        }
                    }

                    if ((rewardType == PuzzleType.Empty || (int)rewardType < 4) && ((find[0].Count >= 2 || find[2].Count >= 2) && (find[1].Count >= 2 || find[3].Count >= 2))) //L자
                    {

                        Debug.Log("L자");
                        rewardPuzzles.Clear();
                        rewardType = PuzzleType.Bomb;
                        rewardPuzzles.Add(curPuzzle);

                        for (int bombindex = 0; bombindex < 4; bombindex++)
                        {
                            if (find[bombindex].Count >= 2)
                            {
                                rewardPuzzles.AddRange(find[bombindex]);
                            }
                        }


                    }

                    if ((rewardType == PuzzleType.Empty || (int)rewardType < 2) && ((find[0].Count + find[2].Count >= 3) || (find[1].Count + find[3].Count >= 3)))
                    {

                        rewardPuzzles.Clear();
                        rewardPuzzles.Add(curPuzzle);

                        if ((find[0].Count + find[2].Count >= 3))
                        {
                            rewardType = PuzzleType.Vertical;
                            rewardPuzzles.AddRange(find[0]);
                            rewardPuzzles.AddRange(find[2]);
                        }
                        else if (find[1].Count + find[3].Count >= 3)
                        {
                            rewardType = PuzzleType.Horizontal;
                            rewardPuzzles.AddRange(find[1]);
                            rewardPuzzles.AddRange(find[3]);

                        }

                    }

                    //세로가 터질 요건이 충족되는지
                    if (find[0].Count + find[2].Count >= 2)
                    {
                        destroyPuzzles.Add(curPuzzle);
                        destroyPuzzles.AddRange(find[0]);
                        destroyPuzzles.AddRange(find[2]);
                    }

                    if (find[1].Count + find[3].Count >= 2)
                    {

                        destroyPuzzles.Add(curPuzzle);
                        destroyPuzzles.AddRange(find[1]);
                        destroyPuzzles.AddRange(find[3]);
                    }



                    //여기서는 동서남북 확인해서 아이템 지급할건지만 확인하고 , 반복문 긑나면 아이템 지급 검사 하셈
                }
                //bfs끝

                // TODO: 검사를 안에서 해도 될거같음. 스페셜이 있으면 굳이 검사 안해도 되니까.
                List<Puzzle> specialList = new List<Puzzle>();

                specialList.AddRange(destroyPuzzles.FindAll(x => x.type == PuzzleType.Vertical));
                specialList.AddRange(destroyPuzzles.FindAll(x => x.type == PuzzleType.Horizontal));

                if (specialList.Count >= 1)
                {
                    isDestroyBlock = true;
                    foreach (Puzzle p in specialList)
                    {
                        p.DestroyRoutine();
                    }

                    rewardPuzzles.Clear();
                    /*
                    destroyPuzzles.Clear();
                    
                    continue;
                    */

                }

                if (rewardPuzzles.Count >= 1)
                {
                    isDestroyBlock = true;

                    (int, int) temp = (rewardPuzzles[0].x, rewardPuzzles[0].y);
                    PuzzleColor tempColor = rewardPuzzles[0].color;

                    foreach (Puzzle puzzle in rewardPuzzles)
                    {
                        if (puzzle != null)
                        {
                            isDestroyBlock = true;
                            puzzle.DestroyRoutine();
                        }
                    }

                    puzzles[temp.Item1, temp.Item2] = maker.MakeNewPuzzle(temp.Item1, temp.Item2, rewardType, tempColor);


                }


                if (destroyPuzzles.Count >= 1)
                {
                    isDestroyBlock = true;
                    foreach (Puzzle puzzle in destroyPuzzles)
                    {
                        if (puzzle != null && !rewardPuzzles.Contains(puzzle))
                        {
                            isDestroyBlock = true;
                            puzzle.DestroyRoutine();
                        }
                    }


                }
                destroyPuzzles.Clear();
                rewardPuzzles.Clear();

            }
        }

        //터치는 시간만큼 기다려줘야함.

        return isDestroyBlock;
    }



    public void SwapPuzzle(Puzzle swapPuzzle)
    {
        isClick = false;

        TwinklePuzzles(false);

        int newX = curPuzzle.x;
        int newY = curPuzzle.y;


        if ((newX == swapPuzzle.x && (newY == swapPuzzle.y - 1 || newY == swapPuzzle.y + 1))
            || (newY == swapPuzzle.y && (newX == swapPuzzle.x - 1 || newX == swapPuzzle.x + 1)))
        {

            StartCoroutine(SwapPuzzleCor(newX, newY, swapPuzzle));
        }
    }


    IEnumerator SwapPuzzleCor(int newX, int newY, Puzzle swapPuzzle)
    {
        isProcess = true;
        curPuzzle.SetAndMove(swapPuzzle.x, swapPuzzle.y);
        swapPuzzle.SetAndMove(newX, newY);

        yield return new WaitForSeconds(0.1f);

        if (swapPuzzle.type == PuzzleType.Rainbow || curPuzzle.type == PuzzleType.Rainbow)
        {
            if (swapPuzzle.type == PuzzleType.Rainbow || swapPuzzle.type == PuzzleType.Rainbow)
            {
                swapPuzzle.GetComponent<RainbowPuzzle>().SetDestroyColor(curPuzzle.color);
                swapPuzzle.DestroyRoutine();

            }
            else
            {
                curPuzzle.GetComponent<RainbowPuzzle>().SetDestroyColor(swapPuzzle.color);
                curPuzzle.DestroyRoutine();
            }

            yield return new WaitForSeconds(0.1f);
            Fill();

            yield break;

        }


        /*
        if (swapPuzzle.type == PuzzleType.Horizontal || curPuzzle.type == PuzzleType.Horizontal || swapPuzzle.type == PuzzleType.Vertical || curPuzzle.type == PuzzleType.Vertical)
        {
            if (swapPuzzle.type == PuzzleType.Horizontal || swapPuzzle.type == PuzzleType.Vertical)
            {
                swapPuzzle.DestroyRoutine();

            }
            else 
            {
                curPuzzle.DestroyRoutine();
            }


            yield return new WaitForSeconds(0.2f);
            Fill();
            isProcess = false;
            yield break;
        }
        */

        if (swapPuzzle.type == PuzzleType.Bomb || curPuzzle.type == PuzzleType.Bomb)
        {
            if (swapPuzzle.type == PuzzleType.Bomb)
            {
                swapPuzzle.DestroyRoutine();



            }
            else
            {
                curPuzzle.DestroyRoutine();
            }


            yield return new WaitForSeconds(0.2f);
            Fill();

            yield break;
        }


        if (CheckPuzzle())
        {
            yield return new WaitForSeconds(0.2f);
            Fill();
        }
        else
        {
            swapPuzzle.SetAndMove(curPuzzle.x, curPuzzle.y);
            curPuzzle.SetAndMove(newX, newY);
            isProcess = false;
        }



        curPuzzle = null;

    }


    public void ReStart()
    {
        SceneManager.LoadSceneAsync(0);
    }


    public void TwinklePuzzles(bool isStart)
    {
        foreach(Puzzle p in hintPuzzles)
        {
            p.Twinkle(isStart);
        }

        if(isStart == false)
        {
            hintPuzzles.Clear();
        }
    }


    //맞는거 찾기
    public void FindMatchToUser()
    {
        // 5개 -> L자 -> 4개 -> 3개 순. 없으면 다 뿌수고 리필.

        if (FindMatch5())
        {
            TwinklePuzzles(true);
            Debug.Log("5개찾음");
            return;
        }

        if (FindMatchL())
        {
            TwinklePuzzles(true);
            Debug.Log("L자 찾음");
            return;
        }
        if (FindMatch4())
        {
            TwinklePuzzles(true);
            Debug.Log("4개찾음");
            return;

        }
        if (FindMatch3())
        {
            TwinklePuzzles(true);
            Debug.Log("3개찾음");
            return;
        }

        Debug.Log("암거도없음;");
        /*
        //없으면 다 부수고 리필

        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                Destroy(puzzles[i, j].gameObject);
                puzzles[i, j] = null;
            }
        }

        Fill();
        */
    }

    List<Puzzle> hintPuzzles = new List<Puzzle>();

    public bool FindMatch5()
    {


        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                List<Puzzle> findPuzzle = new List<Puzzle>();
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || !curPuzzle.IsMoveable() || curPuzzle.color == PuzzleColor.None) continue;

                for (int k = 0; k < 4; k++)
                {
                    int newX = curPuzzle.x + dx[k];
                    int newY = curPuzzle.y + dy[k];

                    //동서남북으로 해야함;
                    //if (curPuzzle.x + 4 >= X || curPuzzle.y + 4 >= Y) continue;


                    for (int l = 0; l < 4; l++)
                    {
                        if (newX < 0 || newY < 0 || newX >= X || newY >= Y) break;
                        //if (puzzles[newX, newY] == null || puzzles[newX, newY].color == PuzzleColor.None) continue;

                        findPuzzle.Add(puzzles[newX, newY]);

                        newX += dx[k];
                        newY += dy[k];


                    }

                    if (findPuzzle.Count == 4)
                    {

                        if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == 3)
                        {
                            Debug.Log("찾았다 같은거 3개");
                            Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                            findPuzzle.Remove(anotherPuzzle);

                            for (int h = -1; h <= 1; h += 2)
                            {
                                //위아래 검사니까 가로를 체크해봐야함
                                if (k == 0 || k == 2)
                                {
                                    if (anotherPuzzle.x + h < 0 || anotherPuzzle.x + h >= X) continue;

                                    if (puzzles[anotherPuzzle.x + h, anotherPuzzle.y].color == curPuzzle.color)
                                    {
                                        findPuzzle.Add(puzzles[anotherPuzzle.x + h, anotherPuzzle.y]);
                                        findPuzzle.Add(curPuzzle);

                                        hintPuzzles.AddRange(findPuzzle);
                                        


                                        return true;
                                    }
                                }
                                else //왼오검사니까 세로를 체크해봐야함
                                {
                                    if (anotherPuzzle.y + h < 0 || anotherPuzzle.y + h >= Y) continue;

                                    if (puzzles[anotherPuzzle.x, anotherPuzzle.y + h].color == curPuzzle.color)
                                    {
                                        findPuzzle.Add(puzzles[anotherPuzzle.x, anotherPuzzle.y + h]);
                                        findPuzzle.Add(curPuzzle);
                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.x + "," + p.y);
                                        }
                                        hintPuzzles.AddRange(findPuzzle);
                                        return true;
                                    }
                                }
                            }

                            findPuzzle.Clear();
                            continue;
                        }
                        else
                        {
                            findPuzzle.Clear();
                            continue;
                        }

                    }
                    else
                    {
                        findPuzzle.Clear();
                        continue;
                    }
                }

            }
        }


        return false;
    }

    public bool IsOutOfIndex(int x, int y)
    {
        if (x < 0 || y < 0 || x >= X || y >= Y) return true;

        return false;
    }

    public bool FindMatchL()
    {
        List<Puzzle> findPuzzle = new List<Puzzle>();


        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || curPuzzle.color == PuzzleColor.None) continue;
                if (IsOutOfIndex(i, j + 3)) continue;

                (bool isTrueShape, Puzzle[] puzzleList) result = isLShape(i, j);

                if (result.isTrueShape) // 왼쪽 맨아래 체크
                {
                    findPuzzle.AddRange(result.puzzleList);

                    hintPuzzles.AddRange(findPuzzle);
                    return true;
                }

                result = isReverseLShape(i, j);

                if (result.isTrueShape) // 왼쪽 맨아래 체크
                {
                    findPuzzle.AddRange(result.puzzleList);
                    hintPuzzles.AddRange(findPuzzle);
                    return true;
                }



            }
        }









        (bool, Puzzle[]) isLShape(int x, int y)
        {
            if (IsOutOfIndex(x + 2, y) || IsOutOfIndex(x, y + 2)) return (false, null);

            Puzzle curpuzzle = puzzles[x, y];

            if (puzzles[x, y + 1].color == curpuzzle.color && puzzles[x, y + 2].color != curpuzzle.color
                && puzzles[x + 1, y + 2].color == curpuzzle.color && puzzles[x + 2, y + 2].color == curpuzzle.color)
            {

                if (!IsOutOfIndex(x - 1, y + 2) && puzzles[x - 1, y + 2].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x + 1, y + 2], puzzles[x + 2, y + 2], puzzles[x - 1, y + 2] });
                }
                else if (!IsOutOfIndex(x, y + 3) && puzzles[x, y + 3].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x + 1, y + 2], puzzles[x + 2, y + 2], puzzles[x, y + 3] });
                }

            }

            return (false, null);

        }

        (bool, Puzzle[]) isReverseLShape(int x, int y)
        {
            if (IsOutOfIndex(x - 2, y) || IsOutOfIndex(x, y + 2)) return (false,null);

            Puzzle curpuzzle = puzzles[x, y];

            if(puzzles[x, y + 1].color == curpuzzle.color && puzzles[x, y + 2].color != curpuzzle.color
                && puzzles[x - 1, y + 2].color == curpuzzle.color && puzzles[x - 2, y + 2].color == curpuzzle.color)
            {
                if (!IsOutOfIndex(x + 1, y + 2) && puzzles[x + 1, y + 2].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x - 1, y + 2], puzzles[x - 2, y + 2], puzzles[x + 1, y + 2] });
                }
                else if (!IsOutOfIndex(x, y + 3) && puzzles[x, y + 3].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x - 1, y + 2], puzzles[x - 2, y + 2], puzzles[x, y + 3] });
                }
            }


            return (false, null);
        }

        /*
        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || curPuzzle.color == PuzzleColor.None) continue;
                if (IsOutOfIndex(i, j + 2)) continue;
                if (puzzles[i, j + 1].color != puzzles[i, j + 2].color) continue;

                List<Puzzle> downPuzzles = new List<Puzzle>();

                downPuzzles.Add(puzzles[i, j + 1]);
                downPuzzles.Add(puzzles[i, j + 2]);

                if (IsOutOfIndex(i-2, j) || puzzles[i-1, j].color != puzzles[i-2, j].color) continue;

                List<Puzzle> leftPuzzles = new List<Puzzle>();

                leftPuzzles.Add(puzzles[i-1, j]);
                leftPuzzles.Add(puzzles[i-2, j]);

                if (downPuzzles[0].color == leftPuzzles[0].color)
                {
                    //왼쪽이니까  오른쪽이랑 위 검사해야함.

                    bool isMatch = false;

                    if(!IsOutOfIndex(i, j-1) && downPuzzles[0].color == puzzles[i, j - 1].color)
                    {
                        isMatch = true;
                        findPuzzle.Add(puzzles[i, j - 1]);

                    }
                    else if(!IsOutOfIndex(i+1, j) && downPuzzles[0].color == puzzles[i+1,j].color)
                    {
                        isMatch = true;
                        findPuzzle.Add(puzzles[i+1, j]);
                    }

                    if(isMatch)
                    {
                        findPuzzle.AddRange(downPuzzles);
                        findPuzzle.AddRange(leftPuzzles);

                        return true;
                    }


                }


                if (!IsOutOfIndex(i + 2, j) || puzzles[i + 1, j].color != puzzles[i + 2, j].color) continue;

                List<Puzzle> rightPuzzles = new List<Puzzle>();

                rightPuzzles.Add(puzzles[i + 1, j]);
                rightPuzzles.Add(puzzles[i + 2, j]);


                if (downPuzzles[0].color == rightPuzzles[0].color)
                {
                    //오른쪽이니까 왼쪽이랑 위 검사해야함.

                    bool isMatch = false;

                    if (!IsOutOfIndex(i, j - 1) && downPuzzles[0].color == puzzles[i, j - 1].color)
                    {
                        isMatch = true;
                        findPuzzle.Add(puzzles[i, j - 1]);

                    }
                    else if (downPuzzles[0].color == puzzles[i - 1, j].color)
                    {
                        isMatch = true;
                        findPuzzle.Add(puzzles[i - 1, j]);
                    }

                    if (isMatch)
                    {
                        findPuzzle.AddRange(downPuzzles);
                        findPuzzle.AddRange(leftPuzzles);

                        return true;
                    }

                }
            }
        }
        */

        /*
        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || curPuzzle.color == PuzzleColor.None) continue;
                if (IsOutOfIndex(i, j + 2)) continue;
                if (puzzles[i, j + 1].color != curPuzzle.color || puzzles[i, j + 2].color == curPuzzle.color) continue;

                //왼쪽 검사
                if (!IsOutOfIndex(i - 2, j + 2) && !IsOutOfIndex(i - 1, j + 2))
                {
                    if (puzzles[i - 2, j + 2].color != curPuzzle.color || puzzles[i - 1, j + 2].color != curPuzzle.color) continue;

                    bool isMatch

                    if (!IsOutOfIndex(i + 1, j + 2))
                    {
                        if (puzzles[i + 1, j + 2].color == curPuzzle.color)
                        {
                            findPuzzle.Add(curPuzzle);
                            findPuzzle.Add(puzzles[i, j + 1]);
                            findPuzzle.Add(puzzles[i - 2, j + 2]);
                            findPuzzle.Add(puzzles[i - 1, j + 2]);
                            findPuzzle.Add(puzzles[i + 1, j + 2]);
                            foreach (Puzzle p in findPuzzle)
                            {
                                Debug.Log(p.x + "," + p.y);
                            }

                            return true;
                        }
                    }
                    else if (!IsOutOfIndex(i, j + 3))
                    {
                        if (puzzles[i, j + 3].color == curPuzzle.color)
                        {
                            findPuzzle.Add(curPuzzle);
                            findPuzzle.Add(puzzles[i, j + 1]);
                            findPuzzle.Add(puzzles[i - 2, j + 2]);
                            findPuzzle.Add(puzzles[i - 1, j + 2]);
                            findPuzzle.Add(puzzles[i, j + 3]);
                            foreach (Puzzle p in findPuzzle)
                            {
                                Debug.Log(p.x + "," + p.y);
                            }
                            return true;
                        }
                    }


                }

                //오른쪽검사
                if (!IsOutOfIndex(i + 2, j + 2) && !IsOutOfIndex(i + 1, j + 2))
                {
                    if (puzzles[i + 2, j + 2].color != curPuzzle.color || puzzles[i + 1, j + 2].color != curPuzzle.color) continue;

                    if (!IsOutOfIndex(i - 1, j + 2))
                    {
                        if (puzzles[i - 1, j + 2].color == curPuzzle.color)
                        {
                            findPuzzle.Add(curPuzzle);
                            findPuzzle.Add(puzzles[i, j + 1]);
                            findPuzzle.Add(puzzles[i + 2, j + 2]);
                            findPuzzle.Add(puzzles[i + 1, j + 2]);
                            findPuzzle.Add(puzzles[i - 1, j + 2]);
                            foreach (Puzzle p in findPuzzle)
                            {
                                Debug.Log(p.x + "," + p.y);
                            }
                            return true;
                        }
                    }
                    else if (!IsOutOfIndex(i, j + 3))
                    {
                        if (puzzles[i, j + 3].color == curPuzzle.color)
                        {
                            findPuzzle.Add(curPuzzle);
                            findPuzzle.Add(puzzles[i, j + 1]);
                            findPuzzle.Add(puzzles[i + 2, j + 2]);
                            findPuzzle.Add(puzzles[i + 1, j + 2]);
                            findPuzzle.Add(puzzles[i, j + 3]);
                            foreach (Puzzle p in findPuzzle)
                            {
                                Debug.Log(p.x + "," + p.y);
                            }
                            return true;
                        }
                    }
                }
            }
        }
        
        */
        return false;
    }

    public bool FindMatch4()
    {


        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                List<Puzzle> findPuzzle = new List<Puzzle>();
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || !curPuzzle.IsMoveable() || curPuzzle.color == PuzzleColor.None) continue;

                for (int k = 0; k < 4; k++)
                {
                    int newX = curPuzzle.x + dx[k];
                    int newY = curPuzzle.y + dy[k];

                    //동서남북으로 해야함;
                    //if (curPuzzle.x + 4 >= X || curPuzzle.y + 4 >= Y) continue;


                    for (int l = 0; l < 3; l++)
                    {
                        if (newX < 0 || newY < 0 || newX >= X || newY >= Y)
                        {
           
                            break;
                        }

                        
                        //if (puzzles[newX, newY] == null || puzzles[newX, newY].color == PuzzleColor.None) continue;

                        findPuzzle.Add(puzzles[newX, newY]);

                        newX += dx[k];
                        newY += dy[k];


                    }

                    if (findPuzzle.Count == 3)
                    {
                        Debug.Log("현 컬퍼즐" + i + "," + j);
                        foreach (Puzzle p in findPuzzle)
                        {

                            Debug.Log(k + "4개로 통과했을때" + p.x + "," + p.y);

                        }

                        if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == 2)
                        {

                            Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                            findPuzzle.Remove(anotherPuzzle);

                            for (int h = -1; h <= 1; h += 2)
                            {
                                //위아래 검사니까 가로를 체크해봐야함
                                if (k == 0 || k == 2)
                                {
                                    if (anotherPuzzle.x + h < 0 || anotherPuzzle.x + h >= X) continue;

                                    if (puzzles[anotherPuzzle.x + h, anotherPuzzle.y].color == curPuzzle.color)
                                    {
                                        findPuzzle.Add(puzzles[anotherPuzzle.x + h, anotherPuzzle.y]);
                                        findPuzzle.Add(curPuzzle);

                                        Debug.Log(k + "어나더:" + anotherPuzzle.x + "," + anotherPuzzle.y);
                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.x + "," + p.y);
                                        }
                                        hintPuzzles.AddRange(findPuzzle);
                                        return true;
                                    }
                                }
                                else //왼오검사니까 세로를 체크해봐야함
                                {
                                    if (anotherPuzzle.y + h < 0 || anotherPuzzle.y + h >= Y) continue;

                                    if (puzzles[anotherPuzzle.x, anotherPuzzle.y + h].color == curPuzzle.color)
                                    {
                                        Debug.Log(k + "어나더:" + anotherPuzzle.x + "," + anotherPuzzle.y);
                                        findPuzzle.Add(puzzles[anotherPuzzle.x, anotherPuzzle.y + h]);
                                        findPuzzle.Add(curPuzzle);
                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.x + "," + p.y);
                                        }
                                        hintPuzzles.AddRange(findPuzzle);
                                        return true;
                                    }
                                }

                               

                            }

                            findPuzzle.Clear();
                            continue;
                        }
                        else
                        {
                            findPuzzle.Clear();
                            continue;
                        }

                    }
                    else
                    {
                        findPuzzle.Clear();
                        continue;
                    }
                }

            }
        }


        return false;
    }

    public bool FindMatch3()
    {
        //Puzzle movePuzzle = null;
        

        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                List<Puzzle> findPuzzle = new List<Puzzle>();
                Puzzle curPuzzle = puzzles[i, j];
                
                if (curPuzzle == null || !curPuzzle.IsMoveable() || curPuzzle.color == PuzzleColor.None) continue;

                for (int k = 0; k < 4; k++)
                {
                    int newX = curPuzzle.x + dx[k];
                    int newY = curPuzzle.y + dy[k];

                    //동서남북으로 해야함;
                    //if (curPuzzle.x + 4 >= X || curPuzzle.y + 4 >= Y) continue;
                    

                    for (int l = 0; l < 2; l++)
                    {
                        if (newX < 0 || newY < 0 || newX >= X || newY >= Y) break;
                        //if (puzzles[newX, newY] == null || puzzles[newX, newY].color == PuzzleColor.None) break;

                        findPuzzle.Add(puzzles[newX, newY]);

                        newX += dx[k];
                        newY += dy[k];


                    }

                    if (findPuzzle.Count == 2)
                    {
                        Debug.Log("현 컬퍼즐" + i + "," + j);
                        foreach (Puzzle p in findPuzzle)
                        {
                            
                            Debug.Log(k+"2개로 통과했을때" + p.x + "," + p.y);
                            
                        }

                        if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == 1)
                        {

                            Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                            findPuzzle.Remove(anotherPuzzle);

                            for (int h = -1; h <= 1; h += 2)
                            {
                                //위아래 검사니까 가로를 체크해봐야함
                                if (k == 0 || k == 2)
                                {
                                    if (anotherPuzzle.x + h < 0 || anotherPuzzle.x + h >= X) continue;

                                    if (puzzles[anotherPuzzle.x + h, anotherPuzzle.y].color == curPuzzle.color)
                                    {
                                        findPuzzle.Add(puzzles[anotherPuzzle.x + h, anotherPuzzle.y]);
                                        findPuzzle.Add(curPuzzle);
                                        Debug.Log(k + "어나더: " + anotherPuzzle.x + "," + anotherPuzzle.y);

                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.x + "," + p.y);
                                        }
                                        hintPuzzles.AddRange(findPuzzle);
                                        return true;
                                    }
                                }
                                else //왼오검사니까 세로를 체크해봐야함
                                {
                                    if (anotherPuzzle.y + h < 0 || anotherPuzzle.y + h >= Y) continue;

                                    if (puzzles[anotherPuzzle.x, anotherPuzzle.y + h].color == curPuzzle.color)
                                    {
                                        Debug.Log(k + "어나더: " + anotherPuzzle.x + "," + anotherPuzzle.y);

                                        findPuzzle.Add(puzzles[anotherPuzzle.x, anotherPuzzle.y + h]);
                                        findPuzzle.Add(curPuzzle);
                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.x + "," + p.y);
                                        }
                                        hintPuzzles.AddRange(findPuzzle);
                                        return true;
                                    }
                                }
                            }
                            findPuzzle.Clear();
                            continue;

                        }
                        else
                        {
                            findPuzzle.Clear();
                            continue;
                        }

                    }
                    else
                    {
                        findPuzzle.Clear();
                        continue;
                    }
                }

            }
        }


        return false;
    }



}
