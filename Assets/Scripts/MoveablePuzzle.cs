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

    void Swap(int x, int y)
    {

        manager.puzzles[x, y].x = X;
        manager.puzzles[x, y].y = Y;

        manager.puzzles[myPuzzle.x, myPuzzle.y].x = x;
        manager.puzzles[X, Y].y = y;

        manager.puzzles[x, y].Move(0.1f);
        this.Move(0.1f);


    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager.isProcess) return;

        Debug.Log("클릭");

        manager.CurPuzzle = myPuzzle;
        manager.isClick = true;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("클릭 업");
        manager.isClick = false;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myPuzzle.type == PuzzleType.Obstacle) return;

        if (manager.isProcess == true || manager.isClick == false || manager.CurPuzzle == this || manager.CurPuzzle == null) return;

        Debug.Log("클릭 에터");

        StartCoroutine(SwapPuzzleRoutine());
    }


    IEnumerator SwapPuzzleRoutine()
    {


        int newX = manager.CurPuzzle.x;
        int newY = manager.CurPuzzle.y;
        Puzzle targetPuzzle = manager.CurPuzzle;
        manager.CurPuzzle = null;

        if (newX == X && (newY == Y - 1 || newY == Y + 1))
        {
            targetPuzzle.x = X;
            targetPuzzle.y = Y;

            X = newX;
            Y = newY;

            targetPuzzle.Move(0.1f);
            this.Move(0.1f);

            yield return new WaitForSeconds(0.15f);



            if (manager.CheckPuzzle())
            {
                manager.Fill();
            }
            else
            {

                X = targetPuzzle.x;
                Y = targetPuzzle.y;

                targetPuzzle.x = newX;
                targetPuzzle.y = newY;

                targetPuzzle.Move(0.1f);
                this.Move(0.1f);
                yield return new WaitForSeconds(0.15f);
            }

            manager.isClick = false;
        }
        else if (newY == Y && (newX == X - 1 || newX == X + 1))
        {
            targetPuzzle.x = X;
            targetPuzzle.y = Y;

            X = newX;
            Y = newY;

            targetPuzzle.Move(0.1f);
            this.Move(0.1f);
            yield return new WaitForSeconds(0.15f);


            if (manager.CheckPuzzle())
            {
                manager.Fill();
            }
            else
            {
                X = targetPuzzle.x;
                Y = targetPuzzle.y;

                targetPuzzle.x = newX;
                targetPuzzle.y = newY;

                targetPuzzle.Move(0.1f);
                this.Move(0.1f);
                yield return new WaitForSeconds(0.15f);
            }
            manager.isClick = false;
        }

    }


}
