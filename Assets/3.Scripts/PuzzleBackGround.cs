using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PuzzleBackGround : MonoBehaviour
{
    [SerializeField]
    private Image image;


    //초기화 - 위치, 스프라이트 세팅
    public void Init(Vector2 pos, Sprite spr)
    {
        image.rectTransform.anchoredPosition = pos;
        image.sprite = spr;
    }


}
