using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#region Custom Editor
#if UNITY_EDITOR

[CustomEditor(typeof(CustomGravityForcefield))]
public class CustomGravityForcefieldEditor : Editor
{
    Texture2D bannerTexture;
    private int labelSize = 6;

    string[] modeOptions = { "Gravitate towards centre", "Gravitate towards direction" };

    SerializedProperty priority;
    SerializedProperty power;
    SerializedProperty mode;
    SerializedProperty gravityDirection;
    SerializedProperty gravityCentre;
    SerializedProperty drawOnlyWhenSelected;
    SerializedProperty color;
    SerializedProperty gravityColor;

    private void OnEnable()
    {
        priority = serializedObject.FindProperty("priority");
        power = serializedObject.FindProperty("power");
        mode = serializedObject.FindProperty("mode");
        gravityDirection = serializedObject.FindProperty("gravityDirection");
        gravityCentre = serializedObject.FindProperty("gravityCentre");
        drawOnlyWhenSelected = serializedObject.FindProperty("drawOnlyWhenSelected");
        color = serializedObject.FindProperty("color");
        gravityColor = serializedObject.FindProperty("gravityColor");

        bannerTexture = new Texture2D(0, 0);
        bannerTexture.LoadImage(System.IO.File.ReadAllBytes(Application.dataPath + "/Stupid/Generic/Icons/Banner.png"));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();



        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("General settings:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Priority:", GUILayout.Width(GUI.skin.label.lineHeight * labelSize));
        priority.intValue = (ushort)EditorGUILayout.IntField(priority.intValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Power:", GUILayout.Width(GUI.skin.label.lineHeight * labelSize));
        power.floatValue = EditorGUILayout.FloatField(priority.intValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();



        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("Gravity settings:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Current mode:", GUILayout.Width(GUI.skin.label.lineHeight * labelSize));

        int currentMode = 0;

        for (int m = 0; m < modeOptions.Length; m++)
        {
            if (mode.stringValue == modeOptions[m])
            {
                currentMode = m;
            }
        }

        mode.stringValue = modeOptions[EditorGUILayout.Popup(currentMode, modeOptions)];
        EditorGUILayout.EndHorizontal();

        switch (mode.stringValue)
        {
            case "Gravitate towards centre":
                gravityCentre.vector3Value = EditorGUILayout.Vector3Field("Gravity center:", gravityCentre.vector3Value);
                break;

            case "Gravitate towards direction":
                gravityDirection.vector3Value = EditorGUILayout.Vector3Field("Gravity direction:", gravityDirection.vector3Value);
                break;
        }

        EditorGUILayout.EndVertical();



        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("Visuals:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Draw when selected:", GUILayout.Width(GUI.skin.label.lineHeight * (labelSize + 3)));
        drawOnlyWhenSelected.boolValue = EditorGUILayout.Toggle(drawOnlyWhenSelected.boolValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Area color:", GUILayout.Width(GUI.skin.label.lineHeight * (labelSize + 3)));
        color.colorValue = EditorGUILayout.ColorField(color.colorValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Gravity color:", GUILayout.Width(GUI.skin.label.lineHeight * (labelSize + 3)));
        gravityColor.colorValue = EditorGUILayout.ColorField(gravityColor.colorValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();



        serializedObject.ApplyModifiedProperties();



        //watermark
        if (bannerTexture != null)
        {
            GUILayout.Button(bannerTexture, EditorStyles.label);
        }
    }
}

#endif
#endregion

[AddComponentMenu("Physics/Custom Gravity Forcefield")]
public class CustomGravityForcefield : MonoBehaviour
{
    public ushort priority;
    public float power = 1;

    public Vector3 gravityDirection = Vector3.down;
    public Vector3 gravityCentre;

    public string mode = "Gravitate towards direction";

    public bool drawOnlyWhenSelected;
    public Color color = new Color(194, 232, 18);
    public Color gravityColor = new Color(255, 113, 91);

    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;
    private CapsuleCollider capsuleCollider;
    private MeshCollider meshCollider;

    private void OnTriggerEnter(Collider other)
    {
        CustomGravityEffector cge = other.GetComponent<CustomGravityEffector>();

        if (cge != null)
        {
            cge.inForceFields.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CustomGravityEffector cge = other.GetComponent<CustomGravityEffector>();

        if (cge != null)
        {
            cge.inForceFields.Remove(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (drawOnlyWhenSelected)
        {
            ShowForceField();
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawOnlyWhenSelected)
        {
            ShowForceField();
        }
    }

    public void ShowForceField()
    {
        Gizmos.color = gravityColor;

        switch (mode)
        {
            case "Gravitate towards centre":
                Gizmos.DrawWireSphere(gravityCentre + transform.position, 0.25f);
                break;

            case "Gravitate towards direction":
                Vector3 pos = transform.position;
                Vector3 direction = gravityDirection.normalized * 0.5f;

                Gizmos.DrawRay(pos, direction);

                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
                Gizmos.DrawRay(pos + direction, right * 0.25f);
                Gizmos.DrawRay(pos + direction, left * 0.25f);
                break;
        }

        Gizmos.color = color;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
        else
        {
            if (boxCollider.isTrigger)
            {
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
        }

        if (sphereCollider == null)
        {
            sphereCollider = GetComponent<SphereCollider>();
        }
        else
        {
            if (sphereCollider.isTrigger)
            {
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }
        }

        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }
        else
        {
            //I CANNOT FOR THE LIFE OF ME DRAW THIS SHIT I MEAN COME ON????????????????
        }

        if (meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();
        }
        else
        {
            if (meshCollider.isTrigger)
            {
                Gizmos.DrawWireMesh(meshCollider.sharedMesh);
            }
        }
    }
}
