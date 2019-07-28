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
        Debug.Log($"Instantiating symbols...");
        //Symbols must be sperated by '@' to seperate values with more than one character.
        //Create an array for each individual symbol(s)
        string[] processedSymbols = symbols.Split('@');

        //Create an array for the newly added symbols.
        List<Symbol> retrievedSymbols = new List<Symbol>();

        //Keep track of spacing.
        int spacingCounter = 0;

        //For each symbol in the array, get the game objects and add them.
        for (int i = 0; i < processedSymbols.Length; i++)
            for (int j = 0; j < processedSymbols[i].Length; j++)
            {
                string character = processedSymbols[i].Substring(j, 1);
                GameObject symbolObject = Instantiate(symbolPrefab, transform);
                symbolObject.transform.position = new Vector3(transform.position.x + (spacingCounter * letterSpacing), transform.position.y, transform.position.z);
                symbolObject.GetComponentInChildren<TextMeshPro>().text = character;
                retrievedSymbols.Add(symbolObject.GetComponent<Symbol>());

                spacingCounter++;
            }

        Debug.Log($"Done instantiating {spacingCounter} symbols!");

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
}
