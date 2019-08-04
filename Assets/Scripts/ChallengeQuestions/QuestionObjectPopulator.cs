using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionObjectPopulator : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject symbolPrefab;

    private float symbolSpacing = 1.5f;

    private void Start()
    {
       
    }

    private void OnEnable()
    {
        NetworkManager.NewQuestion += OnNewQuestion;
    }

    private void OnDisable()
    {
        NetworkManager.NewQuestion -= OnNewQuestion;
    }

    private void OnNewQuestion(string question, string[] answers)
    { 
        
        StartCoroutine(QuestionRoutine(question, answers));
        //throw new NotImplementedException();
    }

    public List<Symbol> PopulateContainerWithSymbols(string symbols)
    {
        //Split the symbols into an array.
        string[] processedSymbols = symbols.Split('@');

        List<Symbol> retrievedSymbols = new List<Symbol>();

        //For each symbol in the array, create a container object (symbol) and set it's symbol value.
        //The container will then handle spawning in the meshes.

        int indexSpacer = 0;

        foreach(string s in processedSymbols)
        { 
            Vector3 position = new Vector3(transform.position.x + (indexSpacer * symbolSpacing),
                transform.position.y, transform.position.z);

            GameObject symbolClone = Instantiate(symbolPrefab, position, Quaternion.identity, this.transform) as GameObject;
            //Set the symbol value (the value is the meshes that will be spawned).
            symbolClone.GetComponent<Symbol>().value = s;

            indexSpacer++;
        }

        return retrievedSymbols;
    }

    public void ClearObjects()
    {  
        foreach (Symbol s in GetComponentsInChildren<Symbol>())
            //s.TriggerOnClear();
            Destroy(s.gameObject);
    }
    
    private IEnumerator QuestionRoutine(string question, string[] answers)
    {
        PopulateContainerWithSymbols(question);
        yield return new WaitForSeconds(5.0f);
        ClearObjects();
        string allAnswers = "";
        foreach(string s in answers)
        {
            allAnswers = allAnswers + "@" + s;
        }
        PopulateContainerWithSymbols(allAnswers);
        foreach(Symbol symbol in FindObjectsOfType<Symbol>())
        {
            symbol.FindMyGoalObject();
            if (symbol.myGoalObject != null)
                symbol.SetPositionToGoalObject(0, 1.5f, 0);
        }

        yield return new WaitForSeconds(5.0f);

        foreach(Symbol symbol in FindObjectsOfType<Symbol>())
        {
            if(symbol.myGoalObject != null)
                symbol.TriggerMoveTogoalObject();
        }
        yield return new WaitForSeconds(2.5f);
        //cover letters
        ClearObjects();
        //report to server that game can start.
    }
}
    