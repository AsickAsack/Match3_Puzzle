using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RainbowPuzzle : Puzzle
{
    [Header("[ÄÄÆ÷³ÍÆ®]")]
    [SerializeField]
    private GameObject effect;
    private PuzzleColor destroyColor = PuzzleColor.None;


    public override bool CheckItemCombination(Puzzle swapPuzzle)
    {

        if (swapPuzzle.isRainbowType)
        {
            swapPuzzle.Pop(true);
            this.SpecialPop();
        }
        else if (swapPuzzle.isBombType)
        {
            this.Pop(true);
            swapPuzzle.GetComponent<BombPuzzle>().SpecialPop(PuzzleType.Rainbow);
        }
        else
        {
            SetDestroyColor(swapPuzzle.color);
            this.Pop();
        }

        return true;
    }

    public void SetDestroyColor(PuzzleColor destroyColor)
    {
        this.destroyColor = destroyColor;
    }

    public override void Pop(bool isIgnore = false, UnityAction callBack = null)
    {
        if (manager.GetPuzzle(X, Y) == this)
        {
            manager.SetPuzzle(X, Y, null);
        }

        if (!isIgnore)
        {
            manager.SoundManager.PlayPopEffect(this.type);

            for (int i = 0; i < manager.X; i++)
            {
                for (int j = 0; j < manager.Y; j++)
                {
                    Puzzle curPuzzle = manager.GetPuzzle(i, j);

                    if (curPuzzle != null && curPuzzle.color == destroyColor)
                    {
                        Instantiate(effect, this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.Maker.GetPos(i, j);
                        bool effectIgnore = curPuzzle.type == PuzzleType.Normal ? true : false;
                        curPuzzle.Pop(effectIgnore);
                    }
                }
            }
        }

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger("Ex");

        callBack?.Invoke();
    }

    public void SpecialPop()
    {
        if (manager.GetPuzzle(X, Y) == this)
        {
            manager.SetPuzzle(X, Y, null);
        }

        manager.SoundManager.PlayPopEffect(this.type);

        for (int i = 0; i < manager.X; i++)
        {
            for (int j = 0; j < manager.Y; j++)
            {
                Puzzle curPuzzle = manager.GetPuzzle(i, j);

                if (curPuzzle != null)
                {
                    Instantiate(effect, this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.Maker.GetPos(i, j);
                    curPuzzle.Pop(true);
                }
            }
        }

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger("Ex");
    }


}
