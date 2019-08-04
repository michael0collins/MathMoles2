using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    public string value;
    private float spacing = 1.0f;
    public float moveSpeed = 1.0f;
    public GoalObject myGoalObject = null;

    private void Start()
    {
        RetrieveSymbolMeshes(value);
    }

    private void RetrieveSymbolMeshes(string symbols)
    {
        int indexSpacer = 0;

        GameObject symbol = null;

        foreach (char c in symbols)
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
                    symbolValue = "Subtract";
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
                    break;
            }

            print("Trying to instantiate Game/SymbolModels/" + symbolValue);

            symbol = Resources.Load("Game/SymbolModels/" + symbolValue) as GameObject;
            Vector3 position = new Vector3(transform.position.x + (indexSpacer * spacing),
                transform.position.y, transform.position.z);
            /* FIX MODELS */ GameObject symbolClone = Instantiate(symbol, position, new Quaternion(0, 180, 0, 0), this.transform) as GameObject;

            indexSpacer++;
        }
    }

    private IEnumerator OnClear()
    {
        //put clearing effects here.    
        yield return null;
        Destroy(this);
    }
    
    public void FindMyGoalObject()
    {
        //find my goal object
        foreach (GoalObject go in FindObjectsOfType<GoalObject>())
        {
            if (go.answer == value)
                myGoalObject = go;
        }

        if (myGoalObject != null) { print("found goal object"); }
    }

    public void SetPositionToGoalObject(float offsetX, float offsetY, float offsetZ)
    {
        if(myGoalObject != null)
            transform.position = new Vector3(myGoalObject.transform.position.x + offsetX,
                myGoalObject.transform.position.y + offsetY,
                    myGoalObject.transform.position.z + offsetZ);
    }

    public void TriggerMoveTogoalObject()
    {
        StartCoroutine(MoveToGoalObject(myGoalObject.transform));
    }

    private IEnumerator MoveToGoalObject(Transform position)
    {      
        //replace with spline
        while (Vector3.Distance(transform.position, position.position) > .1f)
        {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, position.transform.position.x, moveSpeed * Time.deltaTime),
                Mathf.Lerp(transform.position.y, position.transform.position.y, moveSpeed * Time.deltaTime),
                    Mathf.Lerp(transform.position.z, position.transform.position.z, moveSpeed * Time.deltaTime));

            yield return null;
        }
    }
}
