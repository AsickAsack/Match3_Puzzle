using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MoveablePuzzle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    private Puzzle myPuzzle;
    private Coroutine coMove;

    private PuzzleManager manager;

    private int X => myPuzzle.X;
    private int Y => myPuzzle.Y;


    private void Awake()
    {
        myPuzzle = GetComponent<Puzzle>();
    }

    //퍼즐 매니저 가져오기
    public void SetManager(PuzzleManager manager)
    {
        this.manager = manager;
    }


    //배열에 참조하고 움직이기
    public void Move(float fillTime)
    {
        manager.SetPuzzle(X, Y,myPuzzle);

        if (coMove != null)
        {
            StopCoroutine(coMove);
        }

        coMove = StartCoroutine(MoveCoroutine(X, Y, fillTime));
    }
    public void Move(int x, int y, float fillTime, UnityAction callback)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
        }

        coMove = StartCoroutine(MoveCoroutine(x, y, fillTime, callback));
    }


    IEnumerator MoveCoroutine(int x, int y, float fillTime,UnityAction callback = null)
    {
        float curtime = 0.0f;
        Vector2 startPos = myPuzzle.myRect.anchoredPosition;
        Vector2 targetPos = manager.Maker.GetPos(x, y);

        while (curtime < fillTime)
        {
            curtime += Time.deltaTime;
            myPuzzle.myRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curtime / fillTime);

            yield return null;
        }

        myPuzzle.SetPos(targetPos);

        callback?.Invoke();
    }



    //퍼즐 눌렀을때
    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager.isProcess) return;

        manager.SelectPuzzle = myPuzzle;
        manager.isClick = true;
    }

    //터치 뗐을때
    public void OnPointerUp(PointerEventData eventData)
    {
        manager.isClick = false;
    }

    //누른상태로 퍼즐에 터치 들어왔을때
    public void OnPointerEnter(PointerEventData eventData)
    {

        if (manager.isProcess == true || manager.isClick == false || manager.SelectPuzzle == this || manager.SelectPuzzle == null) return;

        manager.SwapPuzzle(this.myPuzzle);

    }




}
