using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalPuzzle : Puzzle
{
    public override void DestroyRoutine(bool isIgnore = false)
    {

        manager.puzzles[this.x, this.y] = null;

        for (int i = 0; i < manager.Y; i++)
        {

            if (manager.puzzles[this.x, i] != null)
            {
                manager.puzzles[this.x, i].DestroyRoutine();
            }
        }

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetTrigger(color.ToString());
    }
}
