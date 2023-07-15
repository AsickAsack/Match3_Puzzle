using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VerticalPuzzle : Puzzle
{
    public GameObject effect;

    public override void DestroyRoutine(bool isIgnore = false, UnityAction callBack = null)
    {
        Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndShoot(Vector2.up);
        Instantiate(effect, this.transform.position, Quaternion.identity, this.transform.parent).GetComponent<CrossEffect>().SetAndShoot(Vector2.down);

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
