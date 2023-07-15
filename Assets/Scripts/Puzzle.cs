using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


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

    //좌표
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

    //퍼즐 초기화
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

    //위치 세팅
    public void SetPos(Vector2 pos)
    {
        myRect.anchoredPosition = pos;
    }


    //파괴 됐을때 루틴
    public virtual void DestroyRoutine(bool isIgnore = false,UnityAction callBack = null)
    {
        manager.puzzles[x, y] = null;

        if (isIgnore)
        {
            EndDestroyAnimation();
        }
        else
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetTrigger(color.ToString());

        }

        callBack?.Invoke();

    }

    //애니메이션이 끝난 후 처리
    public void EndDestroyAnimation()
    {
        //type = PuzzleType.Empty;
        Destroy(this.gameObject);
    }

    Coroutine twinkle = null;

    public void Twinkle(bool isStart = true)
    {
        if(isStart)
        {
            if (twinkle != null)
            {
                StopCoroutine(twinkle);
            }

            twinkle = StartCoroutine(TwinkleCo());
        }
        else
        {
            if (twinkle != null)
            {
                StopCoroutine(twinkle);
            }

            Color color = myImage.color;

            color.a = 1.0f;
            myImage.color = color;

        }
        
    }

    IEnumerator TwinkleCo()
    {

        while (true)
        {

            while(myImage.color.a > 0.25f)
            {
                Color color = myImage.color;

                color.a -= Time.deltaTime;
                myImage.color = color;

                yield return null;
            }

            while (myImage.color.a < 1.0f)
            {

                Color color = myImage.color;

                color.a += Time.deltaTime;
                myImage.color = color;

                yield return null;
            }

            yield return null;

        }


    }

}
