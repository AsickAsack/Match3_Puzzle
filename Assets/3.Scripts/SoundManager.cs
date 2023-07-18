using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource bgmSource;
    [SerializeField]
    private AudioSource effectsource;

    [SerializeField]
    private AudioClip[] effectClips;



    /*
     * 0 스왑
     * 1 일반 터지기
     * 2 장애물 터지기
     * 3 가로세로
     * 4 폭탄
     * 다이아
     * */
    public void PlayEffect(int index)
    {
        effectsource.PlayOneShot(effectClips[index]);
    }
    
    //터트리는 이펙트
    public void PlayPopEffect(PuzzleType type)
    {
        PlayEffect((int)type+1);
    }


}
