using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
public class ChallengeQuestion : MonoBehaviour
{
    public int incorrectAnswerCount = 3;
    public float questionShowDelay = 2.0f;
    public float questionShowTime = 2.0f;

    private GameManager gameManager;
    private Text questionTextObject;
    private string questionText = "";
    private int answer;

    internal bool completed = false;

    private readonly byte mathQuestionEvent = 0;

    private void Start()
    {
        questionTextObject = GetComponentInChildren<Text>();
    }

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

    public void PopulateQuestionEvent()
    {
        GenerateChallengeQuestion();
        object[] content = new object[3] { questionText, answer, CreateIncorrectAnswers() }; // Array contains the target position and the IDs of the selected units
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
            a = UnityEngine.Random.Range(1, 9);
            b = UnityEngine.Random.Range(1, 9);
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

    private IEnumerator ShowQuestion(string question, string[] incorrectAnswers)
    {
        //GetComponent<QuestionObjectPopulator>().PopulateLetterObjects(question);
        questionTextObject.enabled = true;
        questionTextObject.text = question;
        yield return new WaitForSeconds(questionShowTime);
        //do something fancy with text
        questionTextObject.text = "";
        questionTextObject.enabled = false;
        StartCoroutine(ShowAnswers(incorrectAnswers));
    }

    private IEnumerator ShowAnswers(string[] incorrectAnswers)
    {
        questionTextObject.enabled = true;
        List<string> listOfAnswers = new List<string>(incorrectAnswers);
        listOfAnswers.Add(answer.ToString());
        string answerToDisplay = "";
        foreach(string s in listOfAnswers)
        {
            answerToDisplay = answerToDisplay + "   " + s;
        }
        questionTextObject.text = answerToDisplay;
        yield return new WaitForSeconds(5);
        questionTextObject.text = "";
        questionTextObject.enabled = false;
        completed = true;
    }
}
