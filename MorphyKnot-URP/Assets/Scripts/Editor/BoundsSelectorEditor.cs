using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(BoundsSelector))]
public class BoundsSelectorEditor : UnityEditor.Editor
{
    private bool m_editToggled;
    private BoundsSelector m_target;

    private BoxBoundsHandle m_boundsHandle = new BoxBoundsHandle();

    private void OnEnable()
    {
        m_editToggled = false;
        m_target = target as BoundsSelector;

        Tools.hidden = m_editToggled;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        // Draw edit button
        EditorGUILayout.BeginHorizontal();
            // Label
            EditorGUILayout.PrefixLabel("Edit Bound");
            // Button
            var editTex = EditorGUIUtility.FindTexture("CustomTool");
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fixedWidth = 32;
            m_editToggled = GUILayout.Toggle(m_editToggled, editTex, buttonStyle);
            var color = EditorGUILayout.ColorField(m_target.gizmoColor, GUILayout.Width(48));
        EditorGUILayout.EndHorizontal();

        // Draw fields
        var center = m_target.center;
        var size = m_target.size;
        center = EditorGUILayout.Vector3Field("Center", center);
        size = EditorGUILayout.Vector3Field("Size", size);
        // Update component
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(m_target, "Bound3D component update");
            m_target.gizmoColor = color;
            m_target.center = center;
            m_target.size = size;
            SceneView.RepaintAll();
        }
    }

    private void HandleEvent()
    {
        // No event
    }

    private void Draw()
    {
        // Handle repaint event
        EventType eventType = Event.current.type;
        if (eventType != EventType.Repaint)
        {
            return ;
        }
    }

    private void HandleEdit()
    {
        // Hide transform tools
        Tools.hidden = m_editToggled;
        // Draw bound handler
        if (m_editToggled)
        {
            m_boundsHandle.center = m_target.center;
            m_boundsHandle.size = m_target.size;

            var transMat = m_target.transform.localToWorldMatrix;
            using (new Handles.DrawingScope(Color.green, transMat))
            {
                EditorGUI.BeginChangeCheck();
                m_boundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target, "Bound3D change bound");
                    // Update bounds
                    m_target.center = m_boundsHandle.center;
                    m_target.size = m_boundsHandle.size;
                }
            }
        }
    }

    void OnSceneGUI()
    {
        HandleEvent();
        Draw();
        HandleEdit();

        Repaint();
        // Prevent from deselecting object
        Selection.activeGameObject = m_target.gameObject;
    }
}
