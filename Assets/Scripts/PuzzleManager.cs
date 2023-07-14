using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
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


    //ä��� �����ϴ� �Լ�
    public void Fill()
    {
        Debug.Log("��ȣ��");
        StartCoroutine(FillCor());
    }

    bool needFill = false;

    IEnumerator FillCor()
    {
        isProcess = true;
        bool needFill = true;

        Debug.Log("ȣ���");
        while (needFill)
        {
            while (FillRoutine())
            {
                Debug.Log("ȣ���2");
                //�������� �ð� 0.1�� ��ٷ������
                yield return new WaitForSeconds(0.1f);
            }

            needFill = CheckPuzzle();
            yield return new WaitForSeconds(0.2f);
        }

        isProcess = false;
    }


    public bool FillRoutine()
    {
        bool isBlockMove = false;

        for (int j = Y - 2; j >= 0; j--)
        {
            for (int i = 0; i < X; i++) //�Ʒ����� ���� ���� �Ȱ� �ö�
            {

                if (puzzles[i, j] == null || !puzzles[i, j].IsMoveable()) continue;

                Puzzle curPuzzle = puzzles[i, j];
                Puzzle belowPuzzle = puzzles[i, j + 1];

                if (belowPuzzle == null) //���� ���ٸ� �׳� ����
                {
                    PuzzleChange(curPuzzle, i, j + 1);
                    isBlockMove = true;
                }
                else
                {

                    if (/*belowPuzzle.type == PuzzleType.Obstacle ||*/ CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))//�Ʒ�or���� ��ֹ��̶�� �밢�� �¿� �ϴ� Ž�� 
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


        //�ֻ�� ���� ��������
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

    //���� �̵� �� �迭,x,y�� �ٲٱ�
    void PuzzleChange(Puzzle curPuzzle, int newX, int newY)
    {
        puzzles[curPuzzle.x, curPuzzle.y] = null;
        curPuzzle.SetCoordinate(newX, newY);
        curPuzzle.Move(0.1f);
    }

    bool CheckIsObstacle(int x, int y)
    {
        //�ε��� ���� �Ѿ���� üũ
        if (x < 0 || y < 0 || x >= this.X || y >= this.Y)
            return false;

        if (puzzles[x, y] == null || puzzles[x, y].type != PuzzleType.Obstacle)
            return false;

        return true;
    }


    //�ð�������� �˻�
    private int[] dx = new int[] { 0, 1, 0, -1 };
    private int[] dy = new int[] { -1, 0, 1, 0 };



    //�پ��ִ� ���� üũ
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

                    //���� ���񿡼� �������� Ž��
                    for (int k = 0; k < 4; k++)
                    {
                        int newX = curPuzzle.x + dx[k];
                        int newY = curPuzzle.y + dy[k];

                        do
                        {
                            if (newX < 0 || newY < 0 || newX >= X || newY >= Y) break;

                            if (puzzles[newX, newY] == null || curPuzzle.color != puzzles[newX, newY].color) break;

                            //�̹� �湮���� ���� �ֶ��
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

                    //���κ��� �Ǵ��� üũ
                    if (rewardType != PuzzleType.Rainbow && ((find[0].Count + find[2].Count >= 4) || (find[1].Count + find[3].Count >= 4)))
                    {
                        Debug.Log("���Ը��?");
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

                    if ((rewardType == PuzzleType.Empty || (int)rewardType < 4) && ((find[0].Count >= 2 || find[2].Count >= 2) && (find[1].Count >= 2 || find[3].Count >= 2))) //L��
                    {

                        Debug.Log("L��");
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
                        Debug.Log("��Ƽ��?");
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

                    //���ΰ� ���� ����� �����Ǵ���
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



                    //���⼭�� �������� Ȯ���ؼ� ������ �����Ұ����� Ȯ���ϰ� , �ݺ��� �P���� ������ ���� �˻� �ϼ�
                }
                //bfs��

                // TODO: �˻縦 �ȿ��� �ص� �ɰŰ���. ������� ������ ���� �˻� ���ص� �Ǵϱ�.
                List<Puzzle> specialList = new List<Puzzle>();

                specialList.AddRange(destroyPuzzles.FindAll(x => x.type == PuzzleType.Vertical));
                specialList.AddRange(destroyPuzzles.FindAll(x => x.type == PuzzleType.Horizontal));

                if(specialList.Count >=1)
                {
                    isDestroyBlock = true;
                    foreach(Puzzle p in specialList)
                    {
                        p.DestroyRoutine();
                    }
                    destroyPuzzles.Clear();
                    rewardPuzzles.Clear();
                    continue;

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

        //��ġ�� �ð���ŭ ��ٷ������.

        return isDestroyBlock;
    }



    public void SwapPuzzle(Puzzle swapPuzzle)
    {
        isClick = false;
        isProcess = true;

        int newX = curPuzzle.x;
        int newY = curPuzzle.y;


        if ((newX == swapPuzzle.x && (newY == swapPuzzle.y - 1 || newY == swapPuzzle.y + 1))
            || (newY == swapPuzzle.y && (newX == swapPuzzle.x - 1 || newX == swapPuzzle.x + 1)))
        {

            StartCoroutine(SwapPuzzleCor(newX, newY, swapPuzzle));
        }
    }


    IEnumerator SwapPuzzleCor(int newX,int newY, Puzzle swapPuzzle)
    {

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


            yield return new WaitForSeconds(0.2f);
            Fill();
            isProcess = false;
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
            isProcess = false;
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
        }



        curPuzzle = null;
        isProcess = false;
    }

}
