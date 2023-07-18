using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    //ÀÌÆåÆ® ³¡³µÀ»¶§ º»ÀÎ ÆÄ±«
    public void EndEffect()
    {
        Destroy(gameObject);
    }

    //ÀÌÆåÆ® ³¡³µÀ»¶§ ºÎ¸ð ÆÄ±«
    public void EndEffectParent()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
