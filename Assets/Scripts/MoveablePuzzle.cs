using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveablePuzzle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    private Puzzle myPuzzle;
    private Coroutine moveCor;

    private PuzzleManager manager;


    private int X
    {
        get
        {
            return myPuzzle.x;
        }
        set
        {
            myPuzzle.x = value;
        }
    }
    private int Y
    {
        get
        {
            return myPuzzle.y;
        }
        set
        {
            myPuzzle.y = value;
        }
    }
    private void Awake()
    {
        myPuzzle = GetComponent<Puzzle>();
    }

    public void SetManager(PuzzleManager manager)
    {
        this.manager = manager;
    }

    public void Move(int x, int y, float fillTime)
    {

        if (moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        moveCor = StartCoroutine(CoMove(x, y, fillTime));
    }

    public void Move(float fillTime)
    {

        manager.puzzles[X, Y] = myPuzzle;

        if (moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        moveCor = StartCoroutine(CoMove(X, Y, fillTime));
    }

    IEnumerator CoMove(int x, int y, float fillTime)
    {
        float curtime = 0.0f;
        Vector2 startPos = myPuzzle.myRect.anchoredPosition;
        Vector2 targetPos = Vector2.zero;

        targetPos = manager.maker.GetPos(x, y);



        while (curtime < fillTime)
        {
            curtime += Time.deltaTime;
            myPuzzle.myRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curtime / fillTime);

            yield return null;
        }

        myPuzzle.myRect.anchoredPosition = targetPos;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager.isProcess) return;

        manager.CurPuzzle = myPuzzle;
        manager.isClick = true;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        manager.isClick = false;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {

        if (manager.isProcess == true || manager.isClick == false || manager.CurPuzzle == this || manager.CurPuzzle == null) return;

            manager.SwapPuzzle(this.myPuzzle);   
       
    }




}
