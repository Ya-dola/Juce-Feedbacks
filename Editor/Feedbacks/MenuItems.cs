﻿using System;
using UnityEditor;
using UnityEngine;

namespace Juce.Feedbacks
{
    public static class MenuItems
    {
        [MenuItem("Juce/Feedbacks/Create Feedbacks Player", false, 1)]
        private static void CreateFeedbackPlayer()
        {
            GameObject newFeedbackPlayer = new GameObject("FeedbacksPlayer");
            newFeedbackPlayer.AddComponent<FeedbacksPlayer>();
        }

        [MenuItem("Juce/Feedbacks/🗎 Documentation", false, 3)]
        private static void Documentation()
        {
            Application.OpenURL("https://github.com/Juce-Assets/Juce-Feedbacks/wiki");
        }

        [MenuItem("Juce/Feedbacks/🐞 Report Bug", false, 3)]
        private static void ReportBug()
        {
            Application.OpenURL("https://github.com/Juce-Assets/Juce-Feedbacks/issues/new?assignees=&labels=bug&template=bug_report.md");
        }

        [MenuItem("Juce/Feedbacks/🖋 Send Feedback", false, 3)]
        private static void SendFeedback()
        {
            Application.OpenURL("https://github.com/Juce-Assets/Juce-Feedbacks/issues/new?assignees=&labels=enhancement&template=feature_request.md");
        }

        [MenuItem("Juce/Feedbacks/📆 Changelog", false, 3)]
        private static void Changelog()
        {
            Application.OpenURL("https://github.com/Juce-Assets/Juce-Feedbacks/issues/new?assignees=&labels=enhancement&template=feature_request.md");
        }
    }
}