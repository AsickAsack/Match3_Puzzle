using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePuzzle : Puzzle
{
    public override void DestroyRoutine(bool isIgnore = false)
    {
      
        manager.puzzles[this.x, this.y] = null;

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger("ex");

    }

}
