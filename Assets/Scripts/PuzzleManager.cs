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
            yield return null;
        }

        isProcess = false;
    }


    public bool FillRoutine()
    {
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

                    if (belowPuzzle.type == PuzzleType.Obstacle || CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))//�Ʒ��� ��ֹ��̶�� �밢�� �¿� �ϴ� Ž�� 
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
                Puzzle newPuzzle = maker.MakeNewPuzzle(i, -1);

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

        Queue<Puzzle> puzzleQueue = new Queue<Puzzle>();
        List<Puzzle> destoryPuzzle = new List<Puzzle>();
        List<Puzzle> visitPuzzle = new List<Puzzle>();

        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < Y; j++)
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
                        int newX = curPuzzle.x + dx[dir]; //����
                        int newY = curPuzzle.y + dy[dir]; //����
                        
                        if (newX < 0 || newY < 0 || newX >= X || newY >= Y) continue;
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
                                //��
                                case 0:
                                    newY--;
                                    break;

                                //������
                                case 1:
                                    newX++;
                                    break;

                                //�Ʒ�
                                case 2:
                                    newY++;
                                    break;

                                //����
                                case 3:
                                    newX--;
                                    break;
                            }

                            if (newX < 0 || newY < 0 || newX >= X || newY >= Y) break;
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

                    }//4���� for�� ����

                }
                //while�� ����

                if (destoryPuzzle.Count >= 1)
                {
                    isDestroyBlock = true;
                }


                foreach (Puzzle puzzle in destoryPuzzle)
                {
                    //�������� ��ֹ��� �ִ��� Ȯ��
                    for (int dir = 0; dir < 4; dir++) 
                    {
                        int newX = puzzle.x + dx[dir]; //����
                        int newY = puzzle.y + dy[dir]; //����

                        if(CheckIsObstacle(newX,newY))
                        {
                            puzzles[newX, newY].DestroyRoutine();
                        }
                    }

                    if(puzzle != null || puzzle.type != PuzzleType.Empty)
                    {
                        puzzle.DestroyRoutine();
                    }
                     

                }

                destoryPuzzle.Clear();


            }
        }

        return isDestroyBlock;
    }
}
