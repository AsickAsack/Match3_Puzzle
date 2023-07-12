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

    //좌표
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

    //매니저
    protected PuzzleManager manager;

    private void Awake()
    {
        myImage = GetComponentInChildren<Image>();
        moveable = this.GetComponent<MoveablePuzzle>();
    }

    //움직일수 있는 퍼즐인지 확인
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

    //퍼즐 초기화
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

    //위치 세팅
    public void SetPos(Vector2 pos)
    {
        myRect.anchoredPosition = pos;
    }


    //파괴 됐을때 루틴
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

    //애니메이션이 끝난 후 처리
    public void EndDestroyAnimation()
    {
        type = PuzzleType.Empty;
        Destroy(this.gameObject);
    }

}
