using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;


[CustomEditor(typeof(StageManager))]
public class StageManagerEditor : Editor
{
    public SerializedProperty x_property;
    public SerializedProperty y_property;
    public SerializedProperty backGrounds;
    public SerializedProperty prefabs;
    public SerializedProperty normalPuzzleSprites;
    public SerializedProperty horizontalPuzzleSprites;
    public SerializedProperty verticalPuzzleSprites;
    public SerializedProperty gameTheme;

    private int curGridIndex;
    private Puzzle[,] puzzles;
    private PuzzleType curPuzzleType;

    private void OnEnable()
    {

        // Stage의 프로퍼티들을 직렬화 객체로 참조한다.
        x_property = serializedObject.FindProperty("x");
        y_property = serializedObject.FindProperty("y");
        backGrounds = serializedObject.FindProperty("backGrounds");
        prefabs = serializedObject.FindProperty("prefabs");
        normalPuzzleSprites = serializedObject.FindProperty("normalPuzzleSprites");
        horizontalPuzzleSprites = serializedObject.FindProperty("horizontalPuzzleSprites");
        verticalPuzzleSprites = serializedObject.FindProperty("verticalPuzzleSprites");
        gameTheme = serializedObject.FindProperty("gameTheme");

        puzzles = new Puzzle[x_property.intValue, y_property.intValue];
    }

    public override void OnInspectorGUI()
    {

        StageManager myScript = (StageManager)target;

        EditorGUI.BeginChangeCheck();

        //DrawDefaultInspector();
        // 각 프로퍼티들에 대한 기본 ui 필드들을 생성한다.
        EditorGUILayout.PropertyField(x_property);
        EditorGUILayout.PropertyField(y_property);
        EditorGUILayout.PropertyField(backGrounds);
        EditorGUILayout.PropertyField(prefabs);
        EditorGUILayout.PropertyField(normalPuzzleSprites);
        EditorGUILayout.PropertyField(horizontalPuzzleSprites);
        EditorGUILayout.PropertyField(verticalPuzzleSprites);
        EditorGUILayout.PropertyField(gameTheme);
        
        //값 변경 감지

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 20;
        btnStyle.fixedHeight = 50;
        btnStyle.fixedWidth = 100;

        GUIStyle btnStyle2 = new GUIStyle(GUI.skin.button);
        btnStyle2.fontSize = 15;
        btnStyle2.fixedHeight = 50;
        btnStyle2.fixedWidth = 100;

        GUIStyle labeStyle = new GUIStyle();
        labeStyle.fontSize = 20;
        labeStyle.normal.textColor = Color.white;

        EditorGUILayout.Space(50);
        GUILayout.Label(new GUIContent("퍼즐 종류"), labeStyle);

        if (EditorGUI.EndChangeCheck())
        {

            if (x_property.intValue != ((StageManager)target).x || y_property.intValue != ((StageManager)target).y)
            {
                puzzles = new Puzzle[x_property.intValue, y_property.intValue];
                serializedObject.ApplyModifiedProperties();
 
                myScript.MakeFrames();
                DestoryAllPuzzles();
            }
        }

        List<Texture> textures = new List<Texture>();

        for (int i = 0; i < myScript.prefabs.Length; i++)
        {
            textures.Add(GetTexture(i,gameTheme.enumValueIndex,myScript));
        }
        textures.Add(null);

        curGridIndex = GUILayout.SelectionGrid(curGridIndex, textures.ToArray(), 7);
        SetCurPuzzleType();

        EditorGUILayout.Space(50);

        if (GUILayout.Button("리셋", style: btnStyle))
        {
            for (int y = 0; y < y_property.intValue; y++)
            {

                for (int x = 0; x < x_property.intValue; x++)
                {
                    if (puzzles[x,y] !=null)
                    {
                        DestroyImmediate(puzzles[x,y].gameObject);
                        puzzles[x,y] = null;
                    }
                }
            }
        }


        // 버튼 그리드 시작

        for (int y = 0; y < y_property.intValue; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < x_property.intValue; x++)
            {
                Texture tempTexture = null;

                if (puzzles[x, y] != null)
                {
                    tempTexture = puzzles[x, y].GetComponent<Image>().sprite.texture;
                }

                if (GUILayout.Button(image: tempTexture, style: btnStyle))
                {
                    if(tempTexture != null)
                    {
                        DestroyImmediate(puzzles[x, y].gameObject);
                    }

                    puzzles[x, y] = myScript.MakeNewPuzzleInGame(x, y, curPuzzleType,(PuzzleColor)gameTheme.enumValueIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        if (GUILayout.Button("저장", style: btnStyle))
        {
            //저장
            Save();
        }

        serializedObject.ApplyModifiedProperties();
    }

    //저장 함수
    public void Save()
    {
        StringBuilder saveString = new StringBuilder();

        saveString.Append(gameTheme.enumValueIndex+"\n");
        saveString.Append(x_property.intValue + "/" + y_property.intValue);

        for (int y = 0; y < y_property.intValue; y++)
        {
            saveString.Append("\n");
            for (int x = 0; x < x_property.intValue; x++)
            {
                if (puzzles[x, y] == null)
                {
                    Debug.LogError("빈 공간이 있습니다.");
                    return;
                }
                else
                {

                    saveString.Append(((int)puzzles[x, y].type).ToString() + ((int)puzzles[x, y].color).ToString());
                }

                if (x != x_property.intValue - 1) 
                    saveString.Append("/");
            }  
        }

        File.WriteAllText(Application.dataPath+"/puzzleData.txt", saveString.ToString());

        Debug.Log("저장이 완료됐습니다!");
    }

    //아이템 텍스쳐 Get함수
    public Texture GetTexture(int index,int color,StageManager manager)
    {
        try
        {
            switch (index)
            {
                //노말
                case 0:
                    return manager.normalPuzzleSprites[color].texture;
                //장애물
                case 1:
                    return manager.prefabs[index].GetComponent<Image>().sprite.texture;
                //가로
                case 2:
                    return manager.horizontalPuzzleSprites[color].texture;
                //세로
                case 3:
                    return manager.verticalPuzzleSprites[color].texture;
                //폭탄
                case 4:
                //다이아
                case 5:
                    return manager.prefabs[index].GetComponent<Image>().sprite.texture;
                case 6:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    
        return null;
    }

    //모든 퍼즐 제거
    public void DestoryAllPuzzles()
    {
        for(int i=0; i < x_property.intValue;i++)
        {
            for (int j = 0; j < y_property.intValue; j++)
            {
                if (puzzles[i, j] != null)
                {
                    DestroyImmediate(puzzles[i, j].gameObject);
                }
            }
        }
    }


    //현재 눌린 타입 세팅
    public void SetCurPuzzleType()
    {
        curPuzzleType = (PuzzleType)curGridIndex;
    }





}
