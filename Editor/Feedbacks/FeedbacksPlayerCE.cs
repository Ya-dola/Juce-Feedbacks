﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Juce.Feedbacks
{
    [CustomEditor(typeof(FeedbacksPlayer))]
    public class FeedbacksPlayerCE : Editor
    {
        private FeedbacksPlayer CustomTarget => (FeedbacksPlayer)target;

        private readonly List<FeedbackTypeEditorData> feedbackTypes = new List<FeedbackTypeEditorData>();

        private readonly List<FeedbackEditorData> cachedEditorFeedback = new List<FeedbackEditorData>();

        private SerializedProperty feedbacksProperty;

        private void OnEnable()
        {
            GatherProperties();

            GatherFeedbackTypes();

            ChacheAllFeedbacksEditor();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.DrawDefaultInspector();

            DrawFeedbacksEditors();

            DrawAddFeedbackInspector();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(CustomTarget.gameObject);
            }
        }

        private void GatherProperties()
        {
            feedbacksProperty = serializedObject.FindProperty("feedbacks");
        }

        private Feedback AddFeedback(Type type)
        {
            FeedbackTypeEditorData editorData = GetFeedbackEditorDataByType(type);

            return AddFeedback(editorData);
        }

        private Feedback AddFeedback(FeedbackTypeEditorData feedbackTypeEditorData)
        {
            Feedback newFeedback = ScriptableObject.CreateInstance(feedbackTypeEditorData.Type) as Feedback;

            if (newFeedback == null)
            {
                Debug.LogError($"Could not create {nameof(Feedback)} instance, {nameof(feedbackTypeEditorData.Type)} does not inherit from {nameof(Feedback)}");
            }

            CacheFeedbackEditor(newFeedback);

            CustomTarget.AddFeedback(newFeedback);

            newFeedback.Init();

            return newFeedback;
        }

        private void RemoveFeedback(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new Exception();
            }

            RemoveCacheFeedbackEditor(feedback);

            CustomTarget.RemoveFeedback(feedback);
        }

        private void ChacheAllFeedbacksEditor()
        {
            for (int i = 0; i < feedbacksProperty.arraySize; ++i)
            {
                Feedback currFeedback = (Feedback)feedbacksProperty.GetArrayElementAtIndex(i).objectReferenceValue;

                CacheFeedbackEditor(currFeedback);
            }
        }

        private void CacheFeedbackEditor(Feedback feedback)
        {
            RemoveCacheFeedbackEditor(feedback);

            FeedbackTypeEditorData feedbackTypeEditorData = GetFeedbackEditorDataByType(feedback.GetType());

            Editor currEditor = Editor.CreateEditor(feedback);

            FeedbackEditorData feedbackEditorData = new FeedbackEditorData(feedback, feedbackTypeEditorData, currEditor);

            cachedEditorFeedback.Add(feedbackEditorData);
        }

        private void RemoveCacheFeedbackEditor(Feedback feedback)
        {
            for (int i = 0; i < cachedEditorFeedback.Count; ++i)
            {
                if (cachedEditorFeedback[i].Feedback == feedback)
                {
                    cachedEditorFeedback.RemoveAt(i);

                    break;
                }
            }
        }

        private void GatherFeedbackTypes()
        {
            feedbackTypes.Clear();

            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();

                for (int i = 0; i < types.Length; ++i)
                {
                    Type currType = types[i];

                    if (currType.IsSubclassOf(typeof(Feedback)))
                    {
                        FeedbackIdentifier identifier = currType.GetCustomAttribute(typeof(FeedbackIdentifier)) as FeedbackIdentifier;

                        if (identifier == null)
                        {
                            continue;
                        }

                        FeedbackDescription description = currType.GetCustomAttribute(typeof(FeedbackDescription)) as FeedbackDescription;

                        FeedbackTypeEditorData data;

                        if (description != null)
                        {
                            data = new FeedbackTypeEditorData(currType, identifier.Name, identifier.Path, description.Description);
                        }
                        else
                        {
                            data = new FeedbackTypeEditorData(currType, identifier.Name, identifier.Path);
                        }

                        feedbackTypes.Add(data);
                    }
                }
            }
        }

        private FeedbackTypeEditorData GetFeedbackEditorDataByType(Type type)
        {
            for (int i = 0; i < feedbackTypes.Count; ++i)
            {
                FeedbackTypeEditorData currFeedbackEditorData = feedbackTypes[i];

                if (currFeedbackEditorData.Type == type)
                {
                    return currFeedbackEditorData;
                }
            }

            return null;
        }

        private void DrawFeedbacksEditors()
        {
            Event e = Event.current;

            EditorGUILayout.Space(5);

            //FeedbacksPlayerStyling.DrawSplitter();

            EditorGUILayout.LabelField("Feedbacks", EditorStyles.boldLabel);

            for (int i = 0; i < cachedEditorFeedback.Count; ++i)
            {
                FeedbackEditorData currFeedback = cachedEditorFeedback[i];

                FeedbackTypeEditorData feedbackTypeEditorData = currFeedback.FeedbackTypeEditorData;

                //if (!string.IsNullOrEmpty(currFeedback.UserLabel))
                //{
                //    title += $" [{currFeedback.UserLabel}]";
                //}

                bool expanded = currFeedback.Feedback.Expanded;
                bool enabled = currFeedback.Feedback.Enabled;

                //Rect headerRect = FeedbacksPlayerStyling.DrawHeader(ref expanded, ref activeField, title, () =>
                //{
                //    ShowFeedbackContextMenu(currFeedback);
                //});

                //currFeedback.Expanded = expanded;
                //currFeedback.Active = activeField;

                //FeedbacksPlayerStyling.DrawSplitter();

                if (!string.IsNullOrEmpty(feedbackTypeEditorData.Description))
                {
                    EditorGUILayout.HelpBox(feedbackTypeEditorData.Description, MessageType.None);

                    EditorGUILayout.Space(2);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    Styling.DrawHeader(ref expanded, ref enabled, feedbackTypeEditorData.Name, () => ShowFeedbackContextMenu(currFeedback.Feedback));

                    currFeedback.Feedback.Expanded = expanded;
                    currFeedback.Feedback.Enabled = enabled;

                    string errors;
                    bool hasErrors = currFeedback.Feedback.GetFeedbackErrors(out errors);

                    if (hasErrors)
                    {
                        GUIStyle s = new GUIStyle(EditorStyles.label);
                        s.normal.textColor = Color.red;

                        EditorGUILayout.LabelField($"Warning: {errors}", s);
                    }

                    if (!string.IsNullOrEmpty(currFeedback.Feedback.UserData))
                    {
                        EditorGUILayout.LabelField($"{currFeedback.Feedback.UserData}");
                    }

                    if (!expanded)
                    {
                        string feedbackInfoString = currFeedback.Feedback.GetFeedbackInfo();

                        if (!string.IsNullOrEmpty(feedbackInfoString))
                        {
                            EditorGUILayout.LabelField(feedbackInfoString);
                        }
                    }
                    else
                    {
                        EditorGUILayout.Space(2);

                        Styling.DrawSplitter(1, -4, 4);

                        currFeedback.Editor.OnInspectorGUI();

                        foreach (Element element in currFeedback.Feedback.Elements)
                        {
                            EditorGUILayout.Space(2);

                            Editor elementEditor = Editor.CreateEditor(element);

                            EditorGUILayout.LabelField(element.ElementName, EditorStyles.boldLabel);

                            elementEditor.OnInspectorGUI();
                        }
                    }
                }

                // Check if we start dragging this feedback
                //draggingHelper.CheckDraggingItem(e, headerRect, FeedbacksPlayerStyling.ReorderRect, i);
            }

            // Finish dragging
            //int startIndex;
            //int endIndex;
            //bool dragged = draggingHelper.ResolveDragging(e, out startIndex, out endIndex);

            //if (dragged)
            //{
            //    customTarget.ReorderFeedback(startIndex, endIndex);
            //}
        }

        private void DrawAddFeedbackInspector()
        {
            EditorGUILayout.Space(8);

            Styling.DrawSplitter(2.0f);

            EditorGUILayout.Space(2);

            if (GUILayout.Button("Add feedback"))
            {
                ShowFeedbacksMenu();
            }
        }

        private void ShowFeedbacksMenu()
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < feedbackTypes.Count; ++i)
            {
                FeedbackTypeEditorData currFeedbackEditorData = feedbackTypes[i];

                menu.AddItem(new GUIContent($"{currFeedbackEditorData.Path}{currFeedbackEditorData.Name}"),
                    false, OnFeedbacksMenuPressed, currFeedbackEditorData);
            }

            menu.ShowAsContext();
        }

        private void OnFeedbacksMenuPressed(object userData)
        {
            FeedbackTypeEditorData feedbackEditorData = userData as FeedbackTypeEditorData;

            if (feedbackEditorData == null)
            {
                return;
            }

            AddFeedback(feedbackEditorData);
        }

        private void ShowFeedbackContextMenu(Feedback feedback)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Remove"), false, () => RemoveFeedback(feedback));

            menu.ShowAsContext();
        }
    }
}