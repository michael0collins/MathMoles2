using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionObjectPopulator : MonoBehaviour
{
    private float letterSpacing = 1.25f;
    private const string path = "Symbols";

    public Symbol[] PopulateContainerWithSymbols(string symbols)
    {
        //Symbols must be sperated by '@' to seperate values with more than one character.
        //Create an array for each individual symbol(s)
        string[] processedSymbols = symbols.Split('@');

        //Create an array for the newly added symbols.
        Symbol[] retrievedSymbols = new Symbol[processedSymbols.Length];

        //Keep track of spacing.
        int spacingCounter = 0;

        //For each symbol in the array, get the game objects and add them.
        for (int i = 0; i < processedSymbols.Length; i++)
        {
            for (int j = 0; j < processedSymbols[i].Length; j++)
            {
                string character = processedSymbols[i].Substring(j, 1);
                print(path + "/" + processedSymbols[i].Substring(j, 1));
                Symbol symbolToInstantiate;
                if(character == "*" || character == "/" || character == "+" || character == "-")
                {
                    string characterToRetrieve = "";
                    switch (character)
                    {
                        case "*":
                            characterToRetrieve = "Multiply";
                            break;
                        case "/":
                            characterToRetrieve = "Divide";
                            break;
                        case "+":
                            characterToRetrieve = "Add";
                            break;
                        case "-":
                            characterToRetrieve = "Subtract";
                            break;
                        default:
                            break;
                    }
                    symbolToInstantiate = Resources.Load<Symbol>(path + "/" + characterToRetrieve) as Symbol;
                } else
                {
                    symbolToInstantiate = Resources.Load<Symbol>(path + "/" + character) as Symbol;
                }
                Symbol symbolClone = Instantiate(symbolToInstantiate, transform);
                symbolClone.transform.position = new Vector3(transform.position.x + (spacingCounter * letterSpacing), transform.position.y, transform.position.z);
                spacingCounter++;
            }
        }
        
        return retrievedSymbols;
    }

    public void ClearObjects()
    {
        foreach(Symbol s in GetComponentsInChildren<Symbol>())
        {
            s.TriggerOnClear();
        }
    }
}
