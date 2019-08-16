using UnityEngine;

public class QuestionsExample : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.NewQuestion += OnNewQuestion;
    }

    private void OnNewQuestion(string question, string[] answers)
    {
        Debug.Log($"Hey! New question arrived! Question is: {question}");
        Debug.Log("Avaiable answers:");
        for (int i = 0; i < answers.Length; i++)
            Debug.Log(answers[i]);
    }

    private void OnDisable()
    {
        NetworkManager.NewQuestion -= OnNewQuestion;
    }
}
