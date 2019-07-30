using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGoalObjects : MonoBehaviour
{
    public float spacing = 1.0f;
    public GoalObject goalObjectPrefab;

    private NetworkManager nManager;

    private void Start()
    {
        nManager = FindObjectOfType<NetworkManager>();

        SpawnObjects();
    }

    void SpawnObjects()
    {
        string[] answers = new string[] { "10", "12", "5", "7" }; //REPLACE WITH SERVER ANSWER VALUES.
        for (int i = 0; i < answers.Length; i++)
        {
            Vector3 location = new Vector3(transform.position.x + (i * spacing), transform.position.y, transform.position.z);
            GoalObject goalObjectClone = Instantiate(goalObjectPrefab, location, Quaternion.identity) as GoalObject;
            goalObjectClone.answer = answers[i];
        }
    }
}