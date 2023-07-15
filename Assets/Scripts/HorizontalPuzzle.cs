using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HorizontalPuzzle : Puzzle
{
    public GameObject effect;

    public override void DestroyRoutine(bool isIgnore = false, UnityAction callBack = null)
    {

        manager.puzzles[this.x, this.y] = null;

        Instantiate(effect,this.transform.position,Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndShoot(Vector2.right);
        Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndShoot(Vector2.left);


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
