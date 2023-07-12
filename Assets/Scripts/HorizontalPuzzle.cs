using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalPuzzle : Puzzle
{

    //°¡·Î ÀüºÎ ÆÄ±«½ÃÅ´
    public override void DestroyRoutine(bool isIgnore = false)
    {
        for (int i = 0; i < manager.X; i++)
        {
            Puzzle destroyPuzzle = manager.puzzles[i, this.y];

            if (destroyPuzzle == null || destroyPuzzle.type == PuzzleType.Empty || destroyPuzzle == this) continue;

            destroyPuzzle.DestroyRoutine();

        }

        EndDestroyAnimation();
    }
}
