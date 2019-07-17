using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
public class ChallengeQuestion : MonoBehaviour
{
    public int incorrectAnswerCount = 4;
    private string questionText = "";
    private int answer;

    internal bool completed = false;

    private void GenerateChallengeQuestion()
    {
        int a = UnityEngine.Random.Range(1, 9);
        int b = UnityEngine.Random.Range(1, 9);

        int question = UnityEngine.Random.Range(0, 4);
        switch (question)
        {
            case 0:
                Multiply(a, b);
                break;
            case 1:
                Divide(a, b);
                break;
            case 2:
                Add(a, b);
                break;
            case 3:
                Subtract(a, b);
                break;
            default:
                print("No challenge question chosen.");
                break;
        }
    }

    private string[] CreateIncorrectAnswers()
    {
        string[] incorrectAnswers = new string[incorrectAnswerCount];
        for(int i = 0; i < incorrectAnswerCount; i++)
        {
            incorrectAnswers[i] = UnityEngine.Random.Range(0, 20).ToString();
        }
        System.Random rnd = new System.Random();
        return incorrectAnswers = incorrectAnswers.OrderBy(x => rnd.Next()).ToArray(); ;
    }

    private void Multiply(int a, int b)
    {
        answer = a * b;
        questionText = a + " X " + b + " = ?";
    }

    private void Divide(int a, int b)
    {
        while (a % b > 0)
        {
            a = Random.Range(1, 9);
            b = Random.Range(1, 9);
        }
        answer = a / b;
        questionText = a + " % " + b + " = ?";
    }

    private void Add(int a, int b)
    {
        answer = a + b;
        questionText = a + " + " + b + " = ?";
    }

    private void Subtract(int a, int b)
    {
        answer = a - b;
        questionText = a + " - " + b + " = ?";
    }

    public bool CheckAnswer(int guess)
    {
        if (guess == answer)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
