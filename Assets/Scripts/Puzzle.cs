using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;

public class Puzzle : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{

    public Image image;

    public int x;
    public int y;

    public PuzzleType type;
    public PuzzleColor color;
    private PuzzleManager manager;

    private Coroutine moveCor;

    //∆€¡Ò √ ±‚»≠
    public void Init(int x,int y,PuzzleManager manager)
    {
        this.manager = manager;

        this.x = x;
        this.y = y;

        type = PuzzleType.Basic;

        int rand = UnityEngine.Random.Range(0, manager.puzzleSprs.Length);
        this.image.sprite = manager.puzzleSprs[rand];
        this.color = (PuzzleColor)rand;

        image.rectTransform.anchoredPosition = manager.GetPos(x,y);
    }


    public void Move(int x, int y, float fillTime)
    {

        if(moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        moveCor = StartCoroutine(CoMove(x,y,fillTime));
    }
    public void Move(float fillTime)
    {

        if (moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        moveCor = StartCoroutine(CoMove(x, y, fillTime));
    }



    IEnumerator CoMove(int x,int y, float fillTime)
    {
        float curtime = 0.0f;
        Vector2 startPos = image.rectTransform.anchoredPosition;
        Vector2 targetPos = manager.GetPos(x, y);

        while(curtime < fillTime)
        {
            curtime += Time.deltaTime;
            image.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, curtime/fillTime);

            yield return null;
        }

        image.rectTransform.anchoredPosition = targetPos;
    }

    bool isClick = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isClick = true;
        startpos = eventData.position;
        Debug.Log("x : " + x + ", " + y);
    }

    

    Vector2 startpos = Vector2.zero;

    void Swap(int x,int y)
    {
        isClick = false;

        manager.puzzles[x, y].x = this.x;
        manager.puzzles[x, y].y = this.y;

        manager.puzzles[this.x, this.y].x = x;
        manager.puzzles[this.x, this.y].y = y;

        manager.puzzles[x, y].Move(0.1f);
        this.Move(0.1f);

      
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        manager.testText.text = this.y.ToString();
    }
}
