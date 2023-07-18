using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIManager : MonoBehaviour
{
    [SerializeField]
    private Text pointText;
    

    //����Ʈ UI����
    public void SetPointUI(int points)
    {
        pointText.text = points.ToString("N0")+ "��";
    }
}
