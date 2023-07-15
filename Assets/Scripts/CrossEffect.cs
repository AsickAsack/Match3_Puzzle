using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossEffect : MonoBehaviour
{

    public RectTransform myRect;
    public float speed;

    private Vector2 limitHorizontal;
    private Vector2 limitVertical;

    public void SetAndShoot(Vector2 dir)
    {
        
        StartCoroutine(MoveAndRotate(dir));
    }
    
    IEnumerator MoveAndRotate(Vector2 dir)
    {
        float time = 0;
        while(time < 2.0f)
        {
            time += Time.deltaTime;

            myRect.anchoredPosition += dir * speed * Time.deltaTime;


             yield return null;
        }



    }


}
