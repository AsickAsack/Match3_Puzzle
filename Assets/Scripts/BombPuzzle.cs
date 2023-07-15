using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BombPuzzle : Puzzle
{
    public GameObject explosionEffect;



    public override void DestroyRoutine(bool isIgnore = false, UnityAction callBack = null)
    {
        Instantiate(explosionEffect,this.transform.parent).GetComponent<RectTransform>().anchoredPosition = manager.maker.GetPos(this.x-1,this.y-1);

        manager.puzzles[this.x, this.y] = null;

        for (int i = this.x - 1; i <= this.x + 1; i++)
        {
            for (int j = this.y - 1; j <= this.y + 1; j++)
            {
                if (i < 0 || j < 0 || i >= manager.X || j >= manager.Y) continue;


                if (manager.puzzles[i,j] != null)
                {
                    manager.puzzles[i, j].DestroyRoutine();
                }


            }
        }

        Destroy(gameObject);
    }
}
