using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RainbowPuzzle : Puzzle
{
    public GameObject exEffect;
    private PuzzleColor destroyColor = PuzzleColor.None;

    public void SetDestroyColor(PuzzleColor destroyColor)
    {
        this.destroyColor = destroyColor;
    }

    public override void DestroyRoutine(bool isIgnore = false, UnityAction callBack = null)
    {
        //색깔 건너받아야함
        manager.puzzles[this.x, this.y] = null;
        
        for(int i=0;i<manager.X;i++)
        {
            for(int j=0;j<manager.Y;j++)
            {
                if (manager.puzzles[i,j] != null && manager.puzzles[i, j].color == destroyColor)
                {
                    
                    Instantiate(exEffect,this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.maker.GetPos(i,j);
                    bool effectIgnore = manager.puzzles[i, j].type == PuzzleType.Normal ? true : false;
                    manager.puzzles[i, j].DestroyRoutine(effectIgnore);


                }

            }
        }

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger("Ex");

        callBack?.Invoke();


    }



}
