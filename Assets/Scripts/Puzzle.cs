using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PuzzleType
{
    Normal, Obstacle, Horizontal, Vertical, Bomb, Rainbow, Empty
}

public enum PuzzleColor
{
    Blue, Green, Red, Purple, None
}

public class Puzzle : MonoBehaviour
{

    //��ǥ
    public int x;
    public int y;

    public PuzzleType type;
    public PuzzleColor color;

    public RectTransform myRect;
    private Image myImage;

    [SerializeField]
    protected Animator animator;

    public Animation[] destroyAnimation;

    private MoveablePuzzle moveable;

    //�Ŵ���
    protected PuzzleManager manager;

    private void Awake()
    {
        myImage = GetComponentInChildren<Image>();
        moveable = this.GetComponent<MoveablePuzzle>();
    }

    //�����ϼ� �ִ� �������� Ȯ��
    public bool IsMoveable()
    {
        return moveable != null;
    }

    public void Move(int x, int y, float fillTime)
    {
        moveable.Move(x, y, fillTime);
    }

    public void Move(float fillTime)
    {
        moveable.Move(fillTime);
    }

    public void SetAndMove(int newX, int newY)
    {
        SetCoordinate(newX, newY);
        Move(0.1f);
    }

    public void SetColor(PuzzleColor color)
    {
        switch (type)
        {
            case PuzzleType.Normal:
                myImage.sprite = manager.maker.puzzleSprs[(int)color];
                break;
            case PuzzleType.Horizontal:
                myImage.sprite = manager.maker.horizontalSprs[(int)color];
                break;
            case PuzzleType.Vertical:
                myImage.sprite = manager.maker.verticalSprs[(int)color];
                break;
        }

        this.color = color;
    }


    public void SetColor()
    {
        int rand = Random.Range(0, manager.maker.puzzleSprs.Length);

        myImage.sprite = manager.maker.puzzleSprs[rand];

        color = (PuzzleColor)rand;
    }

    //���� �ʱ�ȭ
    public void Init(int x, int y, PuzzleType type, PuzzleManager manager, PuzzleColor color = PuzzleColor.None)
    {
        this.manager = manager;
        if (IsMoveable())
            moveable.SetManager(manager);

        this.x = x;
        this.y = y;

        this.type = type;


        if (this.color != PuzzleColor.None)
        {
            if (color == PuzzleColor.None)
            {
                SetColor();
            }
            else
            {
                SetColor(color);
            }
        }


        SetPos(manager.maker.GetPos(x, y));
    }

    public void SetCoordinate(int newX, int newY)
    {
        this.x = newX;
        this.y = newY;

    }

    //��ġ ����
    public void SetPos(Vector2 pos)
    {
        myRect.anchoredPosition = pos;
    }


    //�ı� ������ ��ƾ
    public virtual void DestroyRoutine(bool isIgnore = false)
    {
        if (isIgnore)
        {
            EndDestroyAnimation();
        }
        else
        {

            manager.puzzles[x, y] = null;

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetTrigger(color.ToString());

        }


    }

    //�ִϸ��̼��� ���� �� ó��
    public void EndDestroyAnimation()
    {
        //type = PuzzleType.Empty;
        Destroy(this.gameObject);
    }

}
