using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalPuzzle : Puzzle
{

    public override void DestroyRoutine(bool isIgnore = false)
    {

        manager.puzzles[this.x, this.y] = null;

        for (int i = 0; i < manager.X; i++)
        {

            if (manager.puzzles[i, this.y] != null)
            {
                manager.puzzles[i, this.y].DestroyRoutine();
            }
        }

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger(color.ToString());
    }

}
