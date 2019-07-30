// Staggart Creations
// http://staggart.xyz

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CompilerNotification : Editor
{
    public static Texture[] spins;
    public static int spinIndex;
    private static float spinTime = 0;
    private static readonly string[] dots = new string[] { " ", ".", "..", "..." };
    private static int dotIndex;
    private static float animateTime = 0;

    private static Rect rect;
    private static Color bgColor;
    private static Color contentColor;
    private static Color defaultColor;
    private static float alpha = 0f;

    //SessionSate survives compilation
    private static bool hasStarted
    {
        get { return SessionState.GetBool("CN_STARTED", false); }
        set { SessionState.SetBool("CN_STARTED", value); }
    }

    private static DateTime startTime;
    private static DateTime endTime;

    private static void OnScene(SceneView sceneView)
    {
        if (EditorApplication.isCompiling)
        {
            if (!hasStarted)
            {
                startTime = DateTime.Now;
            }
            hasStarted = true;


            if (alpha <= 1f) alpha += DateTime.Now.Subtract(startTime).Milliseconds * 0.0001f;
        }
        else
        {
            //Fire once
            if (hasStarted == true)
            {
                endTime = DateTime.Now;
                hasStarted = false;
                alpha = 1f;

                EditorApplication.Beep();
            }

            //Slowly decrease alpha over time
            if (alpha > 0)
            {
                alpha -= DateTime.Now.Subtract(endTime).Milliseconds * 0.00001f;
            }
        }

        //Personal skin
        if (EditorGUIUtility.isProSkin == false)
        {
            bgColor = new Color(0.76f, 0.76f, 0.76f, alpha - 0.25f);
            contentColor = new Color(0.5f, 0.5f, 0.5f, alpha);

            Text.normal.textColor = new Color(0.1f, 0.1f, 0.1f, alpha); ;
            GUI.contentColor = new Color(0.1f, 0.1f, 0.1f, alpha);
        }
        else
        {
            bgColor = new Color(0.33f, 0.33f, 0.33f, alpha - 0.25f);
            contentColor = new Color(0.95f, 0.95f, 0.95f, alpha);

            GUI.contentColor = contentColor;
        }

        Handles.BeginGUI();
        if (hasStarted)
        {
            rect = new Rect(sceneView.camera.pixelWidth - 135 - 10, sceneView.camera.pixelHeight - 35 - 10, 135, 35);
            EditorGUI.DrawRect(rect, bgColor);
            GUI.Label(rect, new GUIContent(" Compiling" + dots[dotIndex], spins[spinIndex]), Text);
        }
        else
        {
            rect = new Rect(sceneView.camera.pixelWidth - 135 - 10, sceneView.camera.pixelHeight - 35 - 10, 135, 35);
            EditorGUI.DrawRect(rect, bgColor);
            GUI.Label(rect, new GUIContent(" Completed!", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "Collab" : "vcs_check").image), Text);
        }
        Handles.EndGUI();

        GUI.contentColor = defaultColor;
    }

    private static void Update()
    {
        if (hasStarted == false) return;

        spinTime += Time.deltaTime;
        animateTime += Time.deltaTime;

        if (spinTime > 1f)
        {
            spinTime = 0;

            if (spinIndex == 11) spinIndex = 0;
            else spinIndex++;
        }

        if (animateTime > 6f)
        {
            animateTime = 0;

            if (dotIndex == 3) dotIndex = 0;
            else dotIndex++;
        }
    }

    [InitializeOnLoadMethod]
    //Fires after a recompile as well
    public static void Initialize()
    {
        if (EditorApplication.isPlaying) return;

        spins = new Texture[12];

        for (int i = 0; i < spins.Length; i++)
        {
            string id = (i < 10) ? "d_WaitSpin0" + i : "d_WaitSpin" + i;
            spins[i] = EditorGUIUtility.IconContent(id).image;
        }

        defaultColor = GUI.contentColor;

        SceneView.onSceneGUIDelegate += OnScene;
        EditorApplication.update += Update;
    }

    private static GUIStyle _Text;
    public static GUIStyle Text
    {
        get
        {
            if (_Text == null)
            {
                _Text = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.ImageLeft,
                    wordWrap = true,
                    fontSize = 16,
                    stretchWidth = true,
                    font = (Font)EditorGUIUtility.LoadRequired("Fonts/Lucida Grande Warning.ttf"),
                    padding = new RectOffset()
                    {
                        left = 10,
                        right = 5,
                        top = 5,
                        bottom = 5
                    }
                };
            }

            return _Text;
        }
    }
}
#endif