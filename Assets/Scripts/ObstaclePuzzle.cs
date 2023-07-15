using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObstaclePuzzle : Puzzle
{
    public override void DestroyRoutine(bool isIgnore = false, UnityAction callBack = null)
    {
      
        manager.puzzles[this.x, this.y] = null;

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger("ex");

    }

}
