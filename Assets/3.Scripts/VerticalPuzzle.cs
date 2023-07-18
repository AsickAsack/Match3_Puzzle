using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VerticalPuzzle : Puzzle
{
    [Header("[컴포넌트]")]
    [SerializeField]
    private GameObject effect;


    //아이템 조합 확인
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

        if(!isIgnore)
        {
            manager.SoundManager.PlayPopEffect(this.type);
            Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndMove(Vector2.up);
            Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndMove(Vector2.down);

            for (int i = 0; i < manager.Y; i++)
            {
                Puzzle curPuzzle = manager.GetPuzzle(this.X, i);

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
