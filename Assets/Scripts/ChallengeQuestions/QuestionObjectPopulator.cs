using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionObjectPopulator : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject symbolPrefab;

    [Header("Variables")]
    public float letterSpacing = 1.25f;

    private void Start()
    {
        PopulateContainerWithSymbols("12@*@2@=");
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
        //StartCoroutine(QuestionRoutine(question, answers));
        //throw new NotImplementedException();
    }

    public List<Symbol> PopulateContainerWithSymbols(string symbols)
    {
        //Split the symbols into an array.
        string[] processedSymbols = symbols.Split('@');

        List<Symbol> retrievedSymbols = new List<Symbol>();

        //For each symbol in the array, create a container object (symbol) and set it's symbol value.
        //The container will then handle spawning in the meshes.
        foreach(string s in processedSymbols)
        {
            Symbol symbolClone = Instantiate(symbolPrefab, ) as Symbol;
        }

        return retrievedSymbols;
    }

    public void ClearObjects()
    {
        foreach (Symbol s in GetComponentsInChildren<Symbol>())
            //s.TriggerOnClear();
            Destroy(s.gameObject);
    }
    
    /*
    private IEnumerator QuestionRoutine(string question, string[] answers)
    {
        PopulateContainerWithSymbols(question);
        yield return new WaitForSeconds(5.0f);
        ClearObjects();
        foreach(string s in answers)
        {
            PopulateContainerWithSymbols(s);
            yield return new WaitForSeconds(2.5f);
            foreach (Symbol symbol in GetComponentsInChildren<Symbol>())
            {
                symbol.TriggerTravelToObjectLocation();
            }
            ClearObjects(); //Remove once the objects can travel to their goal locations.
        }
    }
    */
}
    