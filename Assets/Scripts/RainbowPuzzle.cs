using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowPuzzle : Puzzle
{

    private PuzzleColor destroyColor = PuzzleColor.None;

    public void SetDestroyColor(PuzzleColor destroyColor)
    {
        this.destroyColor = destroyColor;
    }


    public override void DestroyRoutine(bool isIgnore = false)
    {
        //색깔 건너받아야함
        manager.puzzles[this.x, this.y] = null;

        for(int i=0;i<manager.X;i++)
        {
            for(int j=0;j<manager.Y;j++)
            {
                if (manager.puzzles[i,j] != null && manager.puzzles[i, j].color == destroyColor)
                {
                    manager.puzzles[i, j].DestroyRoutine();
                }

            }
        }

        Destroy(this.gameObject);
    }

}
