using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PuzzleType
{
    Normal, Obstacle, Horizontal, Vertical, Bomb, FindSameColor, Empty
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
    private Animator animator;

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

    public void Move(int x,int y,float fillTime)
    {
        moveable.Move(x,y,fillTime);
    }

    public void Move(float fillTime)
    {
        moveable.Move(fillTime);
    }

    public void SetColor()
    {
        int rand = Random.Range(0, manager.maker.puzzleSprs.Length);
        myImage.sprite = manager.maker.puzzleSprs[rand];
        color = (PuzzleColor)rand;
    }

    public void Init(int x, int y, PuzzleManager manager)
    {
        this.manager = manager;
        moveable.SetManager(manager);

        this.x = x;
        this.y = y;

        this.type = PuzzleType.Normal;

        SetPos(manager.maker.GetPos(x, y));
        SetColor();
        //animator.enabled = true;
    }

    //���� �ʱ�ȭ
    public void Init(int x, int y, PuzzleType type, PuzzleManager manager)
    {
        this.manager = manager;
        moveable.SetManager(manager);

        this.x = x;
        this.y = y;

        this.type = type;

        SetPos(manager.maker.GetPos(x, y));
    }

    public void SetCoordinate(int newX,int newY)
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
            animator.enabled = true;
            //animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetTrigger(color.ToString());
            //animator.SetTrigger("Ex");
        }

        
    }

    //�ִϸ��̼��� ���� �� ó��
    public void EndDestroyAnimation()
    {
        type = PuzzleType.Empty;
        Destroy(this.gameObject);
    }

}
