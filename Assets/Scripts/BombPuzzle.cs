using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BombPuzzle : Puzzle
{
    [Header("[ÄÄÆ÷³ÍÆ®]")]
    [SerializeField]
    private GameObject effect;

    public override void Pop(bool isIgnore = false, UnityAction callBack = null)
    {
        Instantiate(effect,this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.Maker.GetPos(this.X-1,this.Y-1);

        if (manager.GetPuzzle(X, Y) == this)
        {
            manager.SetPuzzle(X, Y, null);
        }

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

        callBack?.Invoke();

        Destroy(gameObject);
    }
}
