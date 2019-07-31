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
            ReportObjectCollectionToServer();
            
            //Display the answer that was in this goal object.
            //Give feedback as win
            //Give feedback as loss
        }
        else
        {
            ReportObjectHitToServer();
            //Reduce size / show effect shrinking effect.
        }
    }

    private void ReportObjectCollectionToServer()
    {
        //Check with the server to see if this is the correct object.
    }

    private void ReportObjectHitToServer()
    {
        //Tell the server the new "hits to collect" for this goal object.
    }
}
