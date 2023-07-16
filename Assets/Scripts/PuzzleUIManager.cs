using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIManager : MonoBehaviour
{
    [SerializeField]
    private Text pointText;
    

    //포인트 UI세팅
    public void SetPointUI(int points)
    {
        pointText.text = points.ToString("N0")+ "점";
    }
}
