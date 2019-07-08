using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionObjectPopulator : MonoBehaviour
{
    public Answer[] PopulateLetterObjects(string data)
    {
        Answer[] answerPrefabs = Resources.LoadAll<Answer>("Prefabs/AnswerObjects");
        foreach(char c in data)
        {
            
        }
        return null;
    }
}
