using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;

#region Custom Editor
#if UNITY_EDITOR

[CustomEditor(typeof(CustomGravityEffector))]
public class CustomGravityEffectorEditor : Editor
{
    Texture2D bannerTexture;
    private int labelSize = 6;

    SerializedProperty useGravity;

    private void OnEnable()
    {
        useGravity = serializedObject.FindProperty("useGravity");

        bannerTexture = new Texture2D(0, 0);
        bannerTexture.LoadImage(System.IO.File.ReadAllBytes(Application.dataPath + "/Stupid/Generic/Icons/Banner.png"));
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Use Gravity:", GUILayout.Width(GUI.skin.label.lineHeight * labelSize));
        useGravity.boolValue = EditorGUILayout.Toggle(useGravity.boolValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        //watermark
        if (bannerTexture != null)
        {
            GUILayout.Button(bannerTexture, EditorStyles.label);
        }
    }
}

#endif
#endregion

[RequireComponent(typeof(Rigidbody)), AddComponentMenu("Physics/Custom Gravity Effector")]
public class CustomGravityEffector : MonoBehaviour
{
    public bool useGravity;

    [HideInInspector]
    public Vector3 gravityVelocity = Vector3.zero;

    private Rigidbody rb;
    [HideInInspector]
    public List<CustomGravityForcefield> inForceFields = new List<CustomGravityForcefield>();

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        calculateGravityVelocity();

        if (useGravity)
        {
            rb.AddForce(-gravityVelocity * Physics.gravity.y, ForceMode.Acceleration);
        }
    }

    private void calculateGravityVelocity()
    {
        int priority = -1;
        Vector3 summedGravityVelocity = Vector3.zero;
        List<CustomGravityForcefield> relevantForceFields = new List<CustomGravityForcefield>();

        //figure out the lowest priority
        foreach(CustomGravityForcefield cgf in inForceFields)
        {
            if (cgf.priority > priority)
            {
                priority = cgf.priority;
            }
        }

        //find all forcefields with this priority
        foreach (CustomGravityForcefield cgf in inForceFields)
        {
            if (cgf.priority == priority)
            {
                relevantForceFields.Add(cgf);
            }
        }
        
        //sum the gravity directions into this new velocity
        foreach (CustomGravityForcefield cgf in relevantForceFields)
        {
            switch (cgf.mode)
            {
                case "Gravitate towards centre":
                    summedGravityVelocity += ((cgf.gravityCentre + cgf.transform.position) - transform.position).normalized * cgf.power;
                    break;

                case "Gravitate towards direction":
                    summedGravityVelocity += cgf.gravityDirection.normalized * cgf.power;
                    break;
            }
        }

        //return the mean
        if(relevantForceFields.Count != 0)
        {
            gravityVelocity = summedGravityVelocity / relevantForceFields.Count;
        }
        else
        {
            gravityVelocity = summedGravityVelocity;
        }
    }
}
