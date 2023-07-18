using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BombPuzzle : Puzzle
{
    [Header("[ÄÄÆ÷³ÍÆ®]")]
    [SerializeField]
    private GameObject effect;


    public override bool CheckItemCombination(Puzzle swapPuzzle)
    {

        if (swapPuzzle.isRainbowType)
        {
            swapPuzzle.Pop(true);
            this.SpecialPop(PuzzleType.Rainbow);


        }
        else if (swapPuzzle.isBombType)
        {
            swapPuzzle.Pop(true);
            this.SpecialPop(PuzzleType.Bomb);
        }
        else
        {
            this.Pop();
        }

        return true;

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
            Instantiate(effect, this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.Maker.GetPos(this.X - 1, this.Y - 1);



            for (int i = this.X - 1; i <= this.X + 1; i++)
            {
                for (int j = this.Y - 1; j <= this.Y + 1; j++)
                {
                    //if (i < 0 || j < 0 || i >= manager.X || j >= manager.Y) continue;
                    Puzzle curPuzzle = manager.GetPuzzle(i, j);

                    if (curPuzzle != null)
                    {
                        curPuzzle.Pop();
                    }
                }
            }

        }


        callBack?.Invoke();
        Destroy(gameObject);
    }

    public void SpecialPop(PuzzleType type)
    {
        if (manager.GetPuzzle(X, Y) == this)
        {
            manager.SetPuzzle(X, Y, null);
        }

        manager.SoundManager.PlayPopEffect(this.type);
        Instantiate(effect, this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.Maker.GetPos(this.X - 1, this.Y - 1);


        for (int i = 0; i < manager.X; i++)
        {
            for (int j = this.Y - 1; j <= this.Y + 1; j++)
            {
                Puzzle curPuzzle = manager.GetPuzzle(i, j);

                if (curPuzzle != null)
                {
                    curPuzzle.Pop();
                }
            }
        }

        if (type == PuzzleType.Rainbow)
        {

            for (int i = this.X -1 ; i <= this.X+1; i++)
            {
                for (int j = 0; j < manager.Y; j++)
                {
                    Puzzle curPuzzle = manager.GetPuzzle(i, j);

                    if (curPuzzle != null)
                    {
                        curPuzzle.Pop();
                    }
                }
            }
        }

        

        Destroy(gameObject);
    }
}
