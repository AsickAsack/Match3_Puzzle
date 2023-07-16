using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;


public enum Dir
{
    Up, Right, Down, Left
}


[System.Serializable]
public struct InitPuzzle //처음에 만들어둘 퍼즐
{
    public PuzzleType type;
    public Vector2Int coordinate;
}


public class PuzzleManager : MonoBehaviour
{

    [SerializeField]
    private PuzzleMaker maker;
    
    public PuzzleMaker Maker => maker;

    [SerializeField]
    private PuzzleUIManager uiManager;

    private int point;
    public int Point
    {
        get { return point; }
        set 
        { 
            point = value; 
            uiManager.SetPointUI(point);
        }
    }

    private Puzzle[,] puzzles;

    private Puzzle selectPuzzle;

    public int X => Maker.X;
    public int Y => Maker.Y;

    [SerializeField]
    private float hintTime;

    public bool isClick = false;
    public bool isProcess = true;


    public Puzzle SelectPuzzle
    {
        get
        {
            return selectPuzzle;
        }

        set
        {
            selectPuzzle = value;
        }
    }

    //퍼즐 초기화
    public void InitPuzzles(int x, int y)
    {
        if (puzzles != null) return;

        puzzles = new Puzzle[x, y];

    }

    //좌표로 퍼즐 가져오기, OutOfIndex 검사
    public Puzzle GetPuzzle(int x, int y)
    {
        if (IsOutOfIndex(x, y)) return null;

        return puzzles[x, y];
    }

    //컬러퍼즐인지 검사 후 가져오기
    public Puzzle GetColorPuzzle(int x, int y)
    {
        if (IsOutOfIndex(x, y) || puzzles[x, y].color == PuzzleColor.None) return null;

        return puzzles[x, y];
    }

    //퍼즐 배열에 참조, OutOfIndex 검사
    public bool SetPuzzle(int x, int y, Puzzle newPuzzle)
    {
        if (IsOutOfIndex(x, y)) return false;

        puzzles[x, y] = newPuzzle;
        return true;
    }

    //퍼즐 배열 레인지 밖으로 나가는지 확인
    public bool IsOutOfIndex(int x, int y)
    {
        if (x < 0 || y < 0 || x >= X || y >= Y) return true;

        return false;
    }


    #region 퍼즐 채우기, 터트리기 탐색

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

            yield return CheckPuzzleCo((isNeedFill) =>
            {
                needFill = isNeedFill;
            });

            // yield return new WaitForSeconds(0.2f); //터지고 내리는 시간 기다려주기
        }

        isProcess = false;
        fillco = null;

        CheckHintTime(true);
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
                Puzzle newPuzzle = Maker.MakeNewPuzzle(i, -1, PuzzleType.Normal);

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
        puzzles[curPuzzle.X, curPuzzle.Y] = null;
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


    //시계방향으로 검사 배열
    private int[] dx = new int[] { 0, 1, 0, -1 };
    private int[] dy = new int[] { -1, 0, 1, 0 };


    IEnumerator CheckPuzzleCo(Action<bool> callBack)
    {
        bool isDestroyBlock = false;

        List<Puzzle> itemPuzzles = new List<Puzzle>();
        List<Puzzle> destroyPuzzles = new List<Puzzle>();
        Queue<Puzzle> searchQueue = new Queue<Puzzle>();

        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {

                if (puzzles[i, j] == null || puzzles[i, j].color == PuzzleColor.None) continue;

                HashSet<Puzzle> visitPuzzles = new HashSet<Puzzle>();
                searchQueue.Enqueue(puzzles[i, j]);
                visitPuzzles.Add(puzzles[i, j]);

                PuzzleType rewardType = PuzzleType.Empty;

                while (searchQueue.Count != 0)
                {
                    Puzzle curPuzzle = searchQueue.Dequeue();

                    List<List<Puzzle>> findPuzzles = new List<List<Puzzle>>();

                    List<Puzzle> up = new List<Puzzle>();
                    List<Puzzle> right = new List<Puzzle>();
                    List<Puzzle> down = new List<Puzzle>();
                    List<Puzzle> left = new List<Puzzle>();

                    findPuzzles.Add(up);
                    findPuzzles.Add(right);
                    findPuzzles.Add(down);
                    findPuzzles.Add(left);

                    //현재 퍼즐에서 상하좌우 탐색
                    for (int k = 0; k < 4; k++)
                    {
                        int newX = curPuzzle.X + dx[k];
                        int newY = curPuzzle.Y + dy[k];

                        do
                        {
                            Puzzle newPuzzle = GetPuzzle(newX, newY);

                            if (newPuzzle == null || curPuzzle.color != newPuzzle.color) break;

                            //방문하지 않은 퍼즐이라면 큐에 넣어줌
                            if (visitPuzzles.Add(newPuzzle))
                            {
                                searchQueue.Enqueue(newPuzzle);
                            }

                            if (!itemPuzzles.Contains(newPuzzle) || !destroyPuzzles.Contains(newPuzzle))
                                findPuzzles[k].Add(newPuzzle);


                            newX += dx[k];
                            newY += dy[k];

                        } while (true);
                    }

                    //여기서부터 아이템 생성조건에 부합한지 체크

                    if ((findPuzzles[0].Count + findPuzzles[1].Count + findPuzzles[2].Count + findPuzzles[3].Count) < 2) continue;

                    //레인보우 되는지 체크(5개)
                    if (rewardType != PuzzleType.Rainbow &&
                        ((findPuzzles[0].Count + findPuzzles[2].Count >= 4) || (findPuzzles[1].Count + findPuzzles[3].Count >= 4)))
                    {
                        itemPuzzles.Clear();
                        itemPuzzles.Add(curPuzzle);
                        rewardType = PuzzleType.Rainbow;

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 4)
                        {
                            itemPuzzles.AddRange(findPuzzles[0]);
                            itemPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 4)
                        {
                            itemPuzzles.AddRange(findPuzzles[1]);
                            itemPuzzles.AddRange(findPuzzles[3]);
                        }
                    }
                    // L자 되는지 체크(폭탄)
                    else if ((rewardType == PuzzleType.Empty || (int)rewardType < 4)
                        && ((findPuzzles[0].Count >= 2 || findPuzzles[2].Count >= 2) && (findPuzzles[1].Count >= 2 || findPuzzles[3].Count >= 2))) //L자
                    {

                        itemPuzzles.Clear();
                        rewardType = PuzzleType.Bomb;
                        itemPuzzles.Add(curPuzzle);

                        for (int bombindex = 0; bombindex < 4; bombindex++)
                        {
                            if (findPuzzles[bombindex].Count >= 2)
                            {
                                itemPuzzles.AddRange(findPuzzles[bombindex]);
                            }
                        }


                    }
                    //4개 되는지 체크
                    else if ((rewardType == PuzzleType.Empty || (int)rewardType < 2)
                        && ((findPuzzles[0].Count + findPuzzles[2].Count >= 3) || (findPuzzles[1].Count + findPuzzles[3].Count >= 3)))
                    {

                        itemPuzzles.Clear();
                        itemPuzzles.Add(curPuzzle);

                        if ((findPuzzles[0].Count + findPuzzles[2].Count >= 3))
                        {
                            rewardType = PuzzleType.Vertical;
                            itemPuzzles.AddRange(findPuzzles[0]);
                            itemPuzzles.AddRange(findPuzzles[2]);
                        }
                        else if (findPuzzles[1].Count + findPuzzles[3].Count >= 3)
                        {
                            rewardType = PuzzleType.Horizontal;
                            itemPuzzles.AddRange(findPuzzles[1]);
                            itemPuzzles.AddRange(findPuzzles[3]);

                        }

                    }
                    //다 안되면 터지긴 하는지 체크
                    else
                    {

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 2)
                        {
                            if (!destroyPuzzles.Contains(curPuzzle))
                                destroyPuzzles.Add(curPuzzle);
                            destroyPuzzles.AddRange(findPuzzles[0]);
                            destroyPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 2)
                        {
                            if (!destroyPuzzles.Contains(curPuzzle))
                                destroyPuzzles.Add(curPuzzle);
                            destroyPuzzles.AddRange(findPuzzles[1]);
                            destroyPuzzles.AddRange(findPuzzles[3]);
                        }
                    }


                }
                //bfs끝


                if (destroyPuzzles.Count >= 1)
                {
                    //itemPuzzles = itemPuzzles.Distinct().ToList();
                    //destroyPuzzles = itemPuzzles.Distinct().ToList();

                    isDestroyBlock = true;

                    if (rewardType != PuzzleType.Empty)
                    {
                        Puzzle itemPuzzle = Maker.MakeNewPuzzle(itemPuzzles[0].X, itemPuzzles[0].Y, rewardType, itemPuzzles[0].color);

                        Action<bool, UnityEngine.Events.UnityAction> action = null;

                        foreach (Puzzle puzzle in itemPuzzles)
                        {
                            if (puzzle != null && puzzle != itemPuzzle)
                            {
                                if (puzzle.X != itemPuzzle.X || puzzle.Y != itemPuzzle.Y)
                                {
                                    SetPuzzle(puzzle.X, puzzle.Y, null);
                                }

                                if (puzzle.type == PuzzleType.Normal)
                                    puzzle.Move(itemPuzzle.X, itemPuzzle.Y, 0.1f, () => puzzle.Pop(true));
                                else
                                    action += puzzle.Pop;
                            }
                        }

                        action?.Invoke(false, null);

                        foreach (Puzzle puzzle in destroyPuzzles)
                        {
                            if (puzzle != null)
                            {
                                puzzle.Pop();
                            }
                        }

                        SetPuzzle(itemPuzzle.X, itemPuzzle.Y, itemPuzzle);
                        itemPuzzles.Clear();
                    }
                    else
                    {
                        foreach (Puzzle puzzle in destroyPuzzles)
                        {
                            if (puzzle != null)
                            {

                                puzzle.Pop();
                            }
                        }
                    }

                    destroyPuzzles.Clear();

                }

            }

            yield return null;
        }

        //터치는 시간만큼 기다려줘야함.

        if (isDestroyBlock)
            yield return new WaitForSeconds(0.1f);

        callBack?.Invoke(isDestroyBlock);

    }



    /*

    //붙어있는 퍼즐 체크
    //TODO: 스왑할때 체크는 거기서부터.
    public bool CheckPuzzle(int startX = 0, int startY = 0)
    {
        bool isDestroyBlock = false;

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

                    List<List<Puzzle>> findPuzzles = new List<List<Puzzle>>();

                    List<Puzzle> up = new List<Puzzle>();
                    List<Puzzle> right = new List<Puzzle>();
                    List<Puzzle> down = new List<Puzzle>();
                    List<Puzzle> left = new List<Puzzle>();

                    findPuzzles.Add(up);
                    findPuzzles.Add(right);
                    findPuzzles.Add(down);
                    findPuzzles.Add(left);

                    //현재 퍼즐에서 상하좌우 탐색
                    for (int k = 0; k < 4; k++)
                    {
                        int newX = curPuzzle.X + dx[k];
                        int newY = curPuzzle.Y + dy[k];

                        do
                        {
                            Puzzle newPuzzle = GetPuzzle(newX, newY);

                            if (newPuzzle == null || curPuzzle.color != newPuzzle.color) break;

                            //방문하지 않은 퍼즐이라면 큐에 넣어줌
                            if (visitPuzzles.Add(newPuzzle))
                            {
                                searchQueue.Enqueue(newPuzzle);
                            }

                            findPuzzles[k].Add(newPuzzle);


                            newX += dx[k];
                            newY += dy[k];

                        } while (true);
                    }

                    //여기서부터 아이템 생성조건에 부합한지 체크

                    if ((findPuzzles[0].Count + findPuzzles[1].Count + findPuzzles[2].Count + findPuzzles[3].Count) < 2) continue;

                    //레인보우 되는지 체크(5개)
                    if (rewardType != PuzzleType.Rainbow && ((findPuzzles[0].Count + findPuzzles[2].Count >= 4) || (findPuzzles[1].Count + findPuzzles[3].Count >= 4)))
                    {
                        destroyPuzzles.Clear();
                        destroyPuzzles.Add(curPuzzle);
                        rewardType = PuzzleType.Rainbow;

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 4)
                        {
                            destroyPuzzles.AddRange(findPuzzles[0]);
                            destroyPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 4)
                        {
                            destroyPuzzles.AddRange(findPuzzles[1]);
                            destroyPuzzles.AddRange(findPuzzles[3]);
                        }
                    }
                    // L자 되는지 체크(폭탄)
                    else if ((rewardType == PuzzleType.Empty || (int)rewardType < 4) && ((findPuzzles[0].Count >= 2 || findPuzzles[2].Count >= 2) && (findPuzzles[1].Count >= 2 || findPuzzles[3].Count >= 2))) //L자
                    {

                        destroyPuzzles.Clear();
                        rewardType = PuzzleType.Bomb;
                        destroyPuzzles.Add(curPuzzle);

                        for (int bombindex = 0; bombindex < 4; bombindex++)
                        {
                            if (findPuzzles[bombindex].Count >= 2)
                            {
                                destroyPuzzles.AddRange(findPuzzles[bombindex]);
                            }
                        }


                    }
                    //4개 되는지 체크
                    else if ((rewardType == PuzzleType.Empty || (int)rewardType < 2) && ((findPuzzles[0].Count + findPuzzles[2].Count >= 3) || (findPuzzles[1].Count + findPuzzles[3].Count >= 3)))
                    {

                        destroyPuzzles.Clear();
                        destroyPuzzles.Add(curPuzzle);

                        if ((findPuzzles[0].Count + findPuzzles[2].Count >= 3))
                        {
                            rewardType = PuzzleType.Vertical;
                            destroyPuzzles.AddRange(findPuzzles[0]);
                            destroyPuzzles.AddRange(findPuzzles[2]);
                        }
                        else if (findPuzzles[1].Count + findPuzzles[3].Count >= 3)
                        {
                            rewardType = PuzzleType.Horizontal;
                            destroyPuzzles.AddRange(findPuzzles[1]);
                            destroyPuzzles.AddRange(findPuzzles[3]);

                        }

                    }
                    //다 안되면 터지긴 하는지 체크
                    else
                    {

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 2)
                        {
                            destroyPuzzles.Add(curPuzzle);
                            destroyPuzzles.AddRange(findPuzzles[0]);
                            destroyPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 2)
                        {
                            destroyPuzzles.Add(curPuzzle);
                            destroyPuzzles.AddRange(findPuzzles[1]);
                            destroyPuzzles.AddRange(findPuzzles[3]);
                        }
                    }

                }
                //bfs끝

                // TODO: 검사를 안에서 해도 될거같음. 스페셜이 있으면 굳이 검사 안해도 되니까.
   

                if (destroyPuzzles.Count >= 1)
                {
                    isDestroyBlock = true;

                    if (rewardType != PuzzleType.Empty)
                    {
                        Puzzle itemPuzzle = Maker.MakeNewPuzzle(destroyPuzzles[0].X, destroyPuzzles[0].Y, rewardType, destroyPuzzles[0].color);
                        SetPuzzle(itemPuzzle.X, itemPuzzle.Y, itemPuzzle);
                    }

                    foreach (Puzzle puzzle in destroyPuzzles)
                    {
                        if (puzzle != null)
                        {
                            puzzle.Pop();
                        }
                    }

                    destroyPuzzles.Clear();
                }

            }
        }

        //터치는 시간만큼 기다려줘야함.

        return isDestroyBlock;
    }
    */
    #endregion


    #region 퍼즐 스왑

    //퍼즐 스왑 
    public void SwapPuzzle(Puzzle swapPuzzle)
    {
        //당한쪽에서 호출하는거임. 즉 swapPuzzle이 눌럿던 퍼즐임
        isClick = false;

        int newX = selectPuzzle.X;
        int newY = selectPuzzle.Y;

        if ((newX == swapPuzzle.X && (newY == swapPuzzle.Y - 1 || newY == swapPuzzle.Y + 1))
            || (newY == swapPuzzle.Y && (newX == swapPuzzle.X - 1 || newX == swapPuzzle.X + 1)))
        {
            CheckHintTime(false);
            StartCoroutine(SwapPuzzleCor(newX, newY, swapPuzzle));
        }
    }


    IEnumerator SwapPuzzleCor(int newX, int newY, Puzzle swapPuzzle)
    {
        //당한쪽에서 호출하는거임. 즉 swapPuzzle이 눌럿던 퍼즐임
        isProcess = true;
        selectPuzzle.SetAndMove(swapPuzzle.X, swapPuzzle.Y);
        swapPuzzle.SetAndMove(newX, newY);

        
        if (swapPuzzle.type == PuzzleType.Rainbow || selectPuzzle.type == PuzzleType.Rainbow)
        {
            if (swapPuzzle.type == PuzzleType.Rainbow || swapPuzzle.type == PuzzleType.Rainbow)
            {
                swapPuzzle.GetComponent<RainbowPuzzle>().SetDestroyColor(selectPuzzle.color);
                swapPuzzle.Pop();

            }
            else
            {
                selectPuzzle.GetComponent<RainbowPuzzle>().SetDestroyColor(swapPuzzle.color);
                selectPuzzle.Pop();
            }

            yield return new WaitForSeconds(0.1f);
            Fill();

            yield break;

        }


        if (swapPuzzle.type == PuzzleType.Bomb || selectPuzzle.type == PuzzleType.Bomb)
        {
            if (swapPuzzle.type == PuzzleType.Bomb)
            {
                swapPuzzle.Pop();

            }
            else
            {
                selectPuzzle.Pop();
            }


            yield return new WaitForSeconds(0.2f);
            Fill();

            yield break;
        }
        
        /*
        if (isSpecialMatch(swapPuzzle, selectPuzzle))
        {



            yield return new WaitForSeconds(0.1f);
        }
        */
        StartCoroutine(CheckPuzzleCo((isNeedFill) =>
        {
            //콜백
            if (isNeedFill)
            {
                Fill();
            }
            else
            {
                CheckHintTime(true);
                swapPuzzle.SetAndMove(selectPuzzle.X, selectPuzzle.Y);
                selectPuzzle.SetAndMove(newX, newY);
                isProcess = false;
                selectPuzzle = null;
            }

        }));



        yield return null;

    }

    public bool isSpecialMatch(Puzzle p, Puzzle p2)
    {
        if (p.type == PuzzleType.Normal || p.type == PuzzleType.Horizontal || p.type == PuzzleType.Vertical) return false;
        if (p2.type == PuzzleType.Normal || p2.type == PuzzleType.Horizontal || p2.type == PuzzleType.Vertical) return false;

        if (p.type == PuzzleType.Rainbow)
        {
            switch (p2.type)
            {
                case PuzzleType.Bomb:
                    break;
                case PuzzleType.Rainbow:
                    break;
            }
        }
        else if (p.type == PuzzleType.Bomb)
        {
            switch (p2.type)
            {
                case PuzzleType.Bomb:
                    break;
                case PuzzleType.Rainbow:
                    break;
            }
        }

        return true;
    }


    #endregion


    #region 매치가능한 퍼즐 탐색 기능들

    private Coroutine coHintTimeCheck;
    private List<Puzzle> hintPuzzles = new List<Puzzle>();

    //일정시간동안 입력없을시 힌트주는 시간 체크
    public void CheckHintTime(bool isCheck)
    {
        if (coHintTimeCheck != null)
        {
            StopCoroutine(coHintTimeCheck);
        }

        FlickerPuzzles(false);

        if (isCheck)
        {
            StartCoroutine(CheckHintTimeCoroutine());
        }
    }

    IEnumerator CheckHintTimeCoroutine()
    {
        float time = 0.0f;

        while (time < hintTime)
        {
            time += Time.deltaTime;

            yield return null;
        }

        FindMatchablePuzzle();
    }

    //찾은 힌트 퍼즐들 깜빡/해제
    public void FlickerPuzzles(bool isFlicker)
    {
        foreach (Puzzle p in hintPuzzles)
        {
            if (p != null)
            {
                p.Flicker(isFlicker);
            }

        }

        if (isFlicker == false)
        {
            hintPuzzles.Clear();
        }
    }

    //같은 컬러 퍼즐 찾기
    public Puzzle FindSameColor(Puzzle puzzle, int index, PuzzleColor color, Dir dir)
    {
        Puzzle findPuzzle = null;

        switch (dir)
        {
            case Dir.Up:
                findPuzzle = GetPuzzle(puzzle.X, puzzle.Y - index);
                break;

            case Dir.Right:
                findPuzzle = GetPuzzle(puzzle.X + index, puzzle.Y);
                break;

            case Dir.Down:
                findPuzzle = GetPuzzle(puzzle.X, puzzle.Y + index);
                break;

            case Dir.Left:
                findPuzzle = GetPuzzle(puzzle.X - index, puzzle.Y);
                break;
        }

        if (findPuzzle == null || findPuzzle.type == PuzzleType.Obstacle || findPuzzle.color != color) return null;

        return findPuzzle;

    }




    //매치할수있는 퍼즐 찾기
    public void FindMatchablePuzzle()
    {
        // 5개 -> L자 -> 4개 -> 3개 순. 없으면 다 뿌수고 리필.

        try
        {
            if (FindMatch(5) || FindMatchL() || FindMatch(4) || FindMatch(3))
            {
                FlickerPuzzles(true);
                return;
            }

            for (int j = 0; j < Y; j++)
            {
                for (int i = 0; i < X; i++)
                {
                    if (puzzles[i, j] != null)
                    {
                        puzzles[i, j].Pop();
                    }
                }
            }

            Fill();
        }
        catch
        {
            return;
        }
        
    }


    //5,4,3 모양 탐색
    public bool FindMatch(int MatchCount)
    {

        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                List<Puzzle> findPuzzle = new List<Puzzle>();
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || curPuzzle.color == PuzzleColor.None) continue;

                if (!IsOutOfIndex(i + MatchCount - 1, j))
                {
                    for (int k = 1; k < MatchCount; k++)
                    {
                        findPuzzle.Add(GetPuzzle(i + k, j));
                    }

                    if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == MatchCount - 2)
                    {
                        Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                        findPuzzle.Remove(anotherPuzzle);

                        for (int h = 0; h < 2; h++)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Up : Dir.Down) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Up : Dir.Down));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }
                        }

                        if (MatchCount == 3 && anotherPuzzle.X == curPuzzle.X + 2)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Right) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Right));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }

                        }
                    }
                }

                findPuzzle.Clear();

                if (!IsOutOfIndex(i, j + MatchCount - 1))
                {
                    for (int k = 1; k < MatchCount; k++)
                    {
                        findPuzzle.Add(GetPuzzle(i, j + k));
                    }

                    if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == MatchCount - 2)
                    {
                        Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                        findPuzzle.Remove(anotherPuzzle);


                        for (int h = 0; h < 2; h++)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Right : Dir.Left) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Right : Dir.Left));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }
                        }

                        if (MatchCount == 3 && anotherPuzzle.Y == curPuzzle.Y + 2)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Down) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Down));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }

                        }
                    }
                }
            }
        }


        return false;
    }


    //L모양 탐색
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

        //Lshape탐색

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

        //L뒤집은 모양 탐색
        (bool, Puzzle[]) isReverseLShape(int x, int y)
        {
            if (IsOutOfIndex(x - 2, y) || IsOutOfIndex(x, y + 2)) return (false, null);

            Puzzle curpuzzle = puzzles[x, y];

            if (puzzles[x, y + 1].color == curpuzzle.color && puzzles[x, y + 2].color != curpuzzle.color
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

        return false;
    }

    /*
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
                    int newX = curPuzzle.X + dx[k];
                    int newY = curPuzzle.Y + dy[k];

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


                        if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == 1)
                        {

                            Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                            findPuzzle.Remove(anotherPuzzle);

                            for (int h = -1; h <= 1; h += 2)
                            {
                                //위아래 검사니까 가로를 체크해봐야함
                                if (k == 0 || k == 2)
                                {
                                    if (anotherPuzzle.X + h < 0 || anotherPuzzle.X + h >= X) continue;

                                    if (puzzles[anotherPuzzle.X + h, anotherPuzzle.Y].color == curPuzzle.color)
                                    {
                                        findPuzzle.Add(puzzles[anotherPuzzle.X + h, anotherPuzzle.Y]);
                                        findPuzzle.Add(curPuzzle);
                                        Debug.Log(k + "어나더: " + anotherPuzzle.X + "," + anotherPuzzle.Y);

                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.X + "," + p.Y);
                                        }
                                        hintPuzzles.AddRange(findPuzzle);
                                        return true;
                                    }
                                }
                                else //왼오검사니까 세로를 체크해봐야함
                                {
                                    if (anotherPuzzle.Y + h < 0 || anotherPuzzle.Y + h >= Y) continue;

                                    if (puzzles[anotherPuzzle.X, anotherPuzzle.Y + h].color == curPuzzle.color)
                                    {
                                        Debug.Log(k + "어나더: " + anotherPuzzle.X + "," + anotherPuzzle.Y);

                                        findPuzzle.Add(puzzles[anotherPuzzle.X, anotherPuzzle.Y + h]);
                                        findPuzzle.Add(curPuzzle);
                                        foreach (Puzzle p in findPuzzle)
                                        {
                                            Debug.Log(p.X + "," + p.Y);
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
    */

    #endregion

    #region 체크, 효과 함수

    public void ReStart()
    {
        SceneManager.LoadSceneAsync(0);
    }


    #endregion

}

