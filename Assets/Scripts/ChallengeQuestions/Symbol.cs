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

    private void RetrieveSymbolMeshes(string meshes)
    {
        foreach(char c in meshes)
        {
            print("Game/SymbolModels/" + c);
            GameObject symbol = Resources.Load("Game/SymbolModels/" + c) as GameObject;
        }
    }

    private IEnumerator OnClear()
    {
        //put clearing effects here.
        yield return null;
        Destroy(this);
    }
}
