using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObstaclePuzzle : Puzzle
{
    public override void Pop(bool isIgnore = false, UnityAction callBack = null)
    {
        if (manager.GetPuzzle(X, Y) == this)
        {
            manager.SetPuzzle(X, Y, null);
        }

        if(!isIgnore)
        {
            manager.SoundManager.PlayPopEffect(this.type);
            EndDestroyAnimation();
        }
        else
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetTrigger("ex");
        }
        
    }

}
