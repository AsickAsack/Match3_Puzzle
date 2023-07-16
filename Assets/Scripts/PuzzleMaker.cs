using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMaker : MonoBehaviour
{
    //���� �׸�(�÷�)
    [Header("[���� �׸�]")]
    [SerializeField]
    private PuzzleColor gameTheme;

    [Header("[���� �Ŵ���]")]
    [SerializeField]
    private PuzzleManager manager;

    [Header("[���� ����]")]
    //���� ���μ��� ũ��
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;

    public int X => x;
    public int Y => y;

    [SerializeField]
    private int puzzleSpacing; //���� ����

    [SerializeField]
    private RectTransform puzzleBackFrame;
    [SerializeField]
    private RectTransform puzzleFrame;

    [Header("[������]")]
    [SerializeField]
    private GameObject puzzleBackPrefab;
    [SerializeField]
    private GameObject[] puzzlePrefab;

    [Header("[���� ��������Ʈ]")]
    public Sprite[] puzzleSprs;
    public Sprite[] verticalSprs;
    public Sprite[] horizontalSprs;
    public Sprite[] puzzleBackSprs;
    public Sprite[] frameSprs;

    [Header("[�ʱ� ���� ����]")]
    [SerializeField]
    private InitPuzzle[] initPuzzles;

    private Vector2 puzzleSize;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        //�ܰ��� ����
        puzzleBackFrame.sizeDelta = puzzleFrame.sizeDelta = new Vector2((puzzleSize.x * x) + puzzleSpacing, (puzzleSize.y * y) + puzzleSpacing);
        puzzleBackFrame.GetComponent<Image>().sprite = frameSprs[(int)gameTheme];

        manager.InitPuzzles(x, y);

        //���� ��� ����
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Instantiate(puzzleBackPrefab, puzzleBackFrame.transform).GetComponent<PuzzleBackGround>().Init(GetPos(i, j), puzzleBackSprs[(int)gameTheme]);
            }
        }

        //�ʱ� ���� ������ �ִٸ� ����
        for (int i = 0; i < initPuzzles.Length; i++)
        {
            Puzzle newInitPuzzle = Instantiate(puzzlePrefab[(int)initPuzzles[i].type], puzzleFrame.transform).GetComponent<Puzzle>();
            newInitPuzzle.Init(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y, this.manager);

            manager.SetPuzzle(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y,newInitPuzzle);
        }

        manager.Fill();
    }

    //���ο� ���� ����
    public Puzzle MakeNewPuzzle(int x, int y, PuzzleType type, PuzzleColor color = PuzzleColor.None)
    {
        Puzzle newPuzzle = Instantiate(puzzlePrefab[(int)type], puzzleFrame.transform).GetComponent<Puzzle>();
        newPuzzle.Init(x, y, this.manager, color);

        return newPuzzle;
    }

    //��ǥ���� �´� ���� ��ġ ����
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + puzzleSpacing / 2, -y * puzzleSize.y - puzzleSpacing / 2);
    }

}
