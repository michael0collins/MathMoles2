using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalObject : MonoBehaviour
{
    public int hitsToCollect = 5;
    public string answer = "";

    public void RecieveHit()
    {
        hitsToCollect--;
        if(hitsToCollect == 0)
        {
            ReportObjectToServer();
            //Display the answer that was in this goal object.
            //Give feedback as win
            //Give feedback as loss
        }
        else
        {
            //Reduce size / show effect shrinking effect.
        }
    }

    private void ReportObjectToServer()
    {
        //Check with the server to see if this is the correct object.
    }
}
