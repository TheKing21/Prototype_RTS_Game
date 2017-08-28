#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(CameraControl))]
public class CameraControlEditor : Editor {

    public override void OnInspectorGUI()
    {
        var script = target as CameraControl;

        EditorGUILayout.LabelField("General settings");

        EditorGUI.indentLevel++;

        var serializedObject = new SerializedObject(target);
        var propertyModes = serializedObject.FindProperty("LstModesCamera");
        serializedObject.Update();
        EditorGUILayout.PropertyField(propertyModes, true);
        serializedObject.ApplyModifiedProperties();
        
        var propertyBounds = serializedObject.FindProperty("BoundsLimitCamera");
        serializedObject.Update();
        EditorGUILayout.PropertyField(propertyBounds, true);
        serializedObject.ApplyModifiedProperties();

        script.LockCursor = EditorGUILayout.Toggle("Lock cursor?", script.LockCursor);
        script.IsAfficheDebug = EditorGUILayout.Toggle("Affiche debug?", script.IsAfficheDebug);

        EditorGUI.indentLevel--;


        EditorGUILayout.Separator();


        // Mode - Cursor on edge
        using (var groupCursorOnEdge = new EditorGUILayout.FadeGroupScope(script.LstModesCamera.Contains(CameraControl.enmModeMoveCamera.CursorOnEdge) ? 1 : 0))
        {
            if (groupCursorOnEdge.visible)
            {
                EditorGUILayout.LabelField("CursorOnEdge mode");

                EditorGUI.indentLevel++;

                // General Settings
                script.CameraCursorEdgeSpeed = EditorGUILayout.FloatField("Camera speed", script.CameraCursorEdgeSpeed);
                script.IsCameraMoveWhenCursorOutScreen = EditorGUILayout.Toggle("Is camera move when cursor out of screen?", script.IsCameraMoveWhenCursorOutScreen);
                script.IsSpeedProgressive = EditorGUILayout.Toggle("Camera speed progressive", script.IsSpeedProgressive);


                // Screen edge dectection section
                EditorGUILayout.LabelField("Screen edge dectection");

                EditorGUI.indentLevel++;

                script.PourcentScreenEdgeWidth = EditorGUILayout.FloatField("Pourcent screen edge width", script.PourcentScreenEdgeWidth);
                using (new EditorGUI.DisabledScope(script.IsPourcentScreenEdgeRespectRatio))
                {
                    script.PourcentScreenEdgeHeight = EditorGUILayout.FloatField("Pourcent screen edge height", script.PourcentScreenEdgeHeight);
                }
                script.IsPourcentScreenEdgeRespectRatio = EditorGUILayout.Toggle("Pourcent screen height respect ratio", script.IsPourcentScreenEdgeRespectRatio);

                EditorGUI.indentLevel--;

                script.IsLimitEdgeDectectionToExcludeUI = EditorGUILayout.Toggle("Limit camera for GUI", script.IsLimitEdgeDectectionToExcludeUI);

                using (var groupLimitCamera = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(script.IsLimitEdgeDectectionToExcludeUI)))
                {
                    if (groupLimitCamera.visible)
                    {
                        EditorGUI.indentLevel++;

                        script.EdgeDectectionLimitsLeft = EditorGUILayout.FloatField("Camera Limit Left", script.EdgeDectectionLimitsLeft);
                        script.EdgeDectectionLimitsTop = EditorGUILayout.FloatField("Camera Limit Top", script.EdgeDectectionLimitsTop);
                        script.EdgeDetectionLimitsRight = EditorGUILayout.FloatField("Camera Limit Right", script.EdgeDetectionLimitsRight);
                        script.EdgeDetectionLimitsBottom = EditorGUILayout.FloatField("Camera Limit Bottom", script.EdgeDetectionLimitsBottom);

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }


        EditorGUILayout.Separator();


        // Mode - Click and drag
        using (var groupClickAndDrag = new EditorGUILayout.FadeGroupScope(script.LstModesCamera.Contains(CameraControl.enmModeMoveCamera.ClickAndDrag) ? 1 : 0))
        {
            if (groupClickAndDrag.visible)
            {
                EditorGUILayout.LabelField("ClickAndDrag mode");

                EditorGUI.indentLevel++;

                // General Settings
                script.IsUseRightButtonToDragCamera = EditorGUILayout.Toggle("Is use right button to drag camera?", script.IsUseRightButtonToDragCamera);

                EditorGUI.indentLevel--;
            }
        }


        EditorGUILayout.Separator();


        // Mode - Arrow keys
        using (var groupArrowKeys = new EditorGUILayout.FadeGroupScope(script.LstModesCamera.Contains(CameraControl.enmModeMoveCamera.MoveWithArrowKeys) ? 1 : 0))
        {
            if (groupArrowKeys.visible)
            {
                EditorGUILayout.LabelField("ArrowKeys mode");

                EditorGUI.indentLevel++;

                // General Settings
                script.CameraMoveSpeedArrows = EditorGUILayout.FloatField("Camera speed", script.CameraMoveSpeedArrows);

                EditorGUI.indentLevel--;
            }
        }


        EditorGUILayout.Separator();


        // Zoom
        EditorGUILayout.LabelField("Zoom settings");
        EditorGUI.indentLevel++;
        script.CameraZoomSpeed = EditorGUILayout.FloatField("Zoom speed", script.CameraZoomSpeed);
        script.CameraZoomSmoothSpeed = EditorGUILayout.FloatField("Zoom smooth speed", script.CameraZoomSmoothSpeed);
        script.CameraZoomMinOrtho = EditorGUILayout.FloatField("Min ortho", script.CameraZoomMinOrtho);
        script.CameraZoomMaxOrtho = EditorGUILayout.FloatField("Max ortho", script.CameraZoomMaxOrtho);
        EditorGUI.indentLevel--;


        EditorGUILayout.Separator();
    }
}

#endif