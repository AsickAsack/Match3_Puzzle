using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossEffect : MonoBehaviour
{
    [SerializeField]
    private float speed;

    [SerializeField]
    private RectTransform myRect;

    //방향 설정 후 움직이기
    public void SetAndMove(Vector2 dir)
    {
        StartCoroutine(MoveCoroutine(dir));
    }
    
    IEnumerator MoveCoroutine(Vector2 dir)
    {
        float time = 0;
        while(time < 1.0f)
        {
            time += Time.deltaTime;

            myRect.anchoredPosition += dir * speed * Time.deltaTime;

             yield return null;
        }

        Destroy(gameObject);
    }


}
