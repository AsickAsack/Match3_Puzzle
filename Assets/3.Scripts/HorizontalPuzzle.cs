using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HorizontalPuzzle : Puzzle
{
    [Header("[ÄÄÆ÷³ÍÆ®]")]
    [SerializeField]
    private GameObject effect;


    public override bool CheckItemCombination(Puzzle swapPuzzle)
    {

        if (swapPuzzle.isRainbowType || swapPuzzle.isBombType)
        {
            swapPuzzle.Pop();
            return true;
        }

        return false;
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
            Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndMove(Vector2.right);
            Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndMove(Vector2.left);


            for (int i = 0; i < manager.X; i++)
            {
                Puzzle curPuzzle = manager.GetPuzzle(i, this.Y);

                if (curPuzzle != null)
                {
                    curPuzzle.Pop();
                }
            }

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetTrigger(color.ToString());
        }
        else
        {
            EndDestroyAnimation();
        }


        callBack?.Invoke();
    }

 
}