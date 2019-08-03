using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    public string value;
    private float spacing = 1.0f;

    private void Start()
    {
        RetrieveSymbolMeshes(value);
    }

    private void RetrieveSymbolMeshes(string symbols)
    {
        int indexSpacer = 0;
        print("Trying to instantiate " + symbols);
        foreach(char c in symbols)
        {
            //convert string into readable value by unity.
            string symbolValue = c.ToString();
            switch(symbolValue)
            {
                case "*":
                    symbolValue = "Multiply";
                    break;
                case "/":
                    symbolValue = "Divide";
                    break;
                case "+":
                    symbolValue = "Add";
                    break;
                case "-":
                    symbolValue = "Subrtact";
                    break;
                case "=":
                    symbolValue = "Equals";
                    break;
                case "?":
                    symbolValue = "QuestionMark";
                    break;
                case " ":
                    symbolValue = "Space";
                    break;
                default:
                    print("Could not find symbol value.");
                    break;
            }

            GameObject symbol = Resources.Load("Game/SymbolModels/" + symbolValue) as GameObject;
            Vector3 position = new Vector3(transform.position.x + (indexSpacer * spacing),
                transform.position.y, transform.position.z);
            GameObject symbolClone = Instantiate(symbol, position, new Quaternion(0, 180, 0, 0), this.transform) as GameObject;

            indexSpacer++;
        }
    }

    private IEnumerator OnClear()
    {
        //put clearing effects here.
        yield return null;
        Destroy(this);
    }
}
