﻿using System;
using UnityEngine;

namespace Juce.Feedbacks
{
    public class RendererMaterialPropertyElement : Element
    {
        [SerializeField] [HideInInspector] private Renderer renderer = default;
        [SerializeField] [HideInInspector] private int materialIndex = default;
        [SerializeField] [HideInInspector] private MaterialPropertyType materialPropertyType = default;
        [SerializeField] [HideInInspector] private string property = default;

        public Renderer Renderer => renderer;
        public int MaterialIndex => materialIndex;
        public MaterialPropertyType MaterialPropertyType { set => materialPropertyType = value; }
        public string Property => property;
    }
}
