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
            Debug.Log(needFill);
            yield return new WaitForSeconds(0.3f);
        }

        isProcess = false;
    }


    public bool FillRoutine()
    {
        Debug.Log("�ʷ�ƾ ȣ���");
        bool isBlockMove = false;

        for (int i = 0; i < X; i++)
        {
            for (int j = Y - 2; j >= 0; j--) //�Ʒ����� ���� ���� �Ȱ� �ö�
            {

                if (puzzles[i, j] == null || puzzles[i, j].type == PuzzleType.Empty || puzzles[i, j].type == PuzzleType.Obstacle) continue;

                Puzzle curPuzzle = puzzles[i, j];
                Puzzle belowPuzzle = puzzles[i, j + 1];

                if (belowPuzzle == null || belowPuzzle.type == PuzzleType.Empty) //���� ���ٸ� �׳� ����
                {

                    PuzzleChange(curPuzzle, i, j + 1);
                    isBlockMove = true;

                }
                else
                {

                    if (belowPuzzle.type == PuzzleType.Obstacle || CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))//�Ʒ�or���� ��ֹ��̶�� �밢�� �¿� �ϴ� Ž�� 
                    {
                        for (int diag = -1; diag <= 1; diag += 2)
                        {
                            if (i + diag < 0 || i + diag >= X) continue;

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


        //�ֻ�� ���� ��������
        for (int i = 0; i < X; i++)
        {
            if (puzzles[i, 0] == null || puzzles[i, 0].type == PuzzleType.Empty)
            {
                Puzzle newPuzzle = maker.MakeNewPuzzle(i, -1,PuzzleType.Normal);

                newPuzzle.Move(i, 0, 0.1f);
                puzzles[i, 0] = newPuzzle;

                newPuzzle.SetCoordinate(i, 0);

                isBlockMove = true;
            }
        }

        return isBlockMove;
    }

    //���� �̵� �� �迭,x,y�� �ٲٱ�
    void PuzzleChange(Puzzle curPuzzle, int newX, int newY)
    {
        curPuzzle.Move(newX, newY, 0.1f);
        puzzles[curPuzzle.x, curPuzzle.y] = null;
        puzzles[newX, newY] = curPuzzle;

        curPuzzle.SetCoordinate(newX, newY);
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
    public bool CheckPuzzle()
    {
        bool isDestroyBlock = false;


        List<Puzzle> destroyPuzzles = new List<Puzzle>();
        Queue<Puzzle> searchQueue = new Queue<Puzzle>();

        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < Y; j++)
            {

                if (puzzles[i, j] == null || puzzles[i, j].type == PuzzleType.Empty || puzzles[i, j].type == PuzzleType.Obstacle) continue;

                HashSet<Puzzle> visitPuzzles = new HashSet<Puzzle>();
                searchQueue.Enqueue(puzzles[i, j]);
                visitPuzzles.Add(puzzles[i, j]);

                PuzzleType rewardType = PuzzleType.Empty;
                Vector2Int rewardCoordinate = new Vector2Int(X, Y);

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

                            if (puzzles[newX, newY] == null || puzzles[newX, newY].type == PuzzleType.Empty || curPuzzle.color != puzzles[newX, newY].color) break;

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

                    //���ΰ� ���� ����� �����Ǵ���
                    if (find[0].Count+find[2].Count >= 2)
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

                    //���⼭���� ���� Ȯ��

                    //if (rewardType == PuzzleType.Rainbow) continue;

                    if ((find[0].Count + find[2].Count >= 4) || (find[1].Count + find[3].Count >= 4))
                    {
                        rewardType = PuzzleType.Rainbow;

                        if(curPuzzle.x < rewardCoordinate.x)
                        {
                            rewardCoordinate.x = curPuzzle.x;
                        }

                        if(curPuzzle.y < rewardCoordinate.y)
                        {
                            rewardCoordinate.y = curPuzzle.y;
                        }

                        continue;
                    }
                    else
                    {
                        if (rewardType == PuzzleType.Rainbow) continue;

                        //L������ �˻�
                        if ((find[0].Count >= 2 || find[2].Count >= 2) && (find[1].Count >= 2 || find[3].Count >= 2))
                        {
                            rewardType = PuzzleType.Bomb;
                            rewardCoordinate.x = curPuzzle.x;
                            rewardCoordinate.y = curPuzzle.y;
                            continue;
                        }

                        if (rewardType == PuzzleType.Bomb) continue;

                        //L�� �ƴϸ� 4���� �Ѵ��� �˻�
                        if ((find[0].Count + find[2].Count >= 3))
                        {
                            rewardType = PuzzleType.Vertical;


                            if (curPuzzle.x < rewardCoordinate.x)
                            {
                                rewardCoordinate.x = curPuzzle.x;
                            }

                            if (curPuzzle.y < rewardCoordinate.y)
                            {
                                rewardCoordinate.y = curPuzzle.y;
                            }

                            continue;
                        }
                        else if ((find[1].Count + find[3].Count >= 3))
                        {
                            rewardType = PuzzleType.Horizontal;

                            if (curPuzzle.x < rewardCoordinate.x)
                            {
                                rewardCoordinate.x = curPuzzle.x;
                            }

                            if (curPuzzle.y < rewardCoordinate.y)
                            {
                                rewardCoordinate.y = curPuzzle.y;
                            }
                            continue;
                        }
                    }
                    //���⼭�� �������� Ȯ���ؼ� ������ �����Ұ����� Ȯ���ϰ� , �ݺ��� �P���� ������ ���� �˻� �ϼ�
                }
                //bfs��

                if (destroyPuzzles.Count >= 1)
                {
                    isDestroyBlock = true;
                    foreach (Puzzle puzzle in destroyPuzzles)
                    {
                        if (puzzle != null || puzzle.type != PuzzleType.Empty)
                        {
                            isDestroyBlock = true;
                            puzzle.DestroyRoutine();
                        }
                    }

                    destroyPuzzles.Clear();
                }

                if (rewardType != PuzzleType.Empty)
                {
                    isDestroyBlock = true;
                    puzzles[rewardCoordinate.x,rewardCoordinate.y] = maker.MakeNewPuzzle(rewardCoordinate.x, rewardCoordinate.y, rewardType);
                }
            }
        }


        return isDestroyBlock;
    }
}
