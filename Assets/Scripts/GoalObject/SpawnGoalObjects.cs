using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGoalObjects : MonoBehaviour
{
    public GoalObject goalObjectPrefab;
    public float spacing = 1.0f;

    private void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        for (int i = 0; i < NetworkManager.Answers.Length; i++)
        {
            Vector3 location = new Vector3(transform.position.x + (i * spacing), transform.position.y, transform.position.z);
            GoalObject goalObjectClone = Instantiate(goalObjectPrefab, location, new Quaternion(0, Random.Range(0,360), 0,0)) as GoalObject;
            goalObjectClone.answer = NetworkManager.Answers[i];
            goalObjectClone.goalIndex = i;
        }
    }
}