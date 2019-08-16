using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalObject : MonoBehaviour
{
    public int goalIndex;
    public int hitsToCollect = 10;
    public string answer = "";

    [SerializeField]
    private GameObject dirtMound;
    private float _scale = 1f;

    private void Awake() {
        NetworkManager.PlayerHitAnswerObject += OnPlayerHitAnswerObject;
    }

    private void OnPlayerHitAnswerObject(NetworkPlayer source, int answerIndex, int answerHealth)
    {
        if (answerIndex == goalIndex) {
            hitsToCollect = answerHealth;

            _scale = answerHealth / 10f;
        }
    }

    private void OnDestroy() {
        NetworkManager.PlayerHitAnswerObject -= OnPlayerHitAnswerObject;
    }

    private void OnDisable() {
        NetworkManager.PlayerHitAnswerObject -= OnPlayerHitAnswerObject;
    }

    private void Update() {
        dirtMound.transform.localScale = Vector3.Lerp(dirtMound.transform.localScale, new Vector3(_scale, _scale, _scale), Time.deltaTime * 5f);
    }
}
