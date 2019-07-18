using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    public void TriggerOnClear()
    {
        StartCoroutine(OnClear());
    }

    public void TriggerTravelToObjectLocation(Transform location)
    {
        StartCoroutine(TravelToGoalObjectLocation(location));
    }

    private IEnumerator TravelToGoalObjectLocation(Transform location)
    {
        // move the object from it's spot in the sky down to it's goal location.
        yield return null;
    }

    private IEnumerator OnClear()
    {
        //put clearing effects here.
        yield return null;
        Destroy(this);
    }
}
