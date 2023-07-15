using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public void EndEffect()
    {
        Destroy(gameObject);
    }

    public void EndEffectParent()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
