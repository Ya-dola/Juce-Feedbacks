﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Juce.Feedbacks
{
    [CustomEditor(typeof(GraphicMaterialPropertyElement))]
    public class GraphicMaterialPropertyElementCE : Editor
    {
        private GraphicMaterialPropertyElement CustomTarget => (GraphicMaterialPropertyElement)target;

        private SerializedProperty graphicProperty;
        private SerializedProperty instantiateMaterialProperty;
        private SerializedProperty materialPropertyTypeProperty;
        private SerializedProperty propertyProperty;

        private Material material;
        private int propertyIndex = -1;

        private void OnEnable()
        {
            GatherProperties();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(graphicProperty);

            if (graphicProperty.objectReferenceValue != null)
            {
                Graphic graphic = (Graphic)graphicProperty.objectReferenceValue;

                if (graphic != null)
                {
                    EditorGUILayout.PropertyField(instantiateMaterialProperty);

                    Material material = graphic.materialForRendering;

                    int propertiesCount = ShaderUtil.GetPropertyCount(material.shader);

                    if (propertiesCount > 0)
                    {
                        MaterialPropertyType type = (MaterialPropertyType)materialPropertyTypeProperty.enumValueIndex;
                        ShaderUtil.ShaderPropertyType typeLookingFor = TypeToShaderType(type);

                        List<string> properties = new List<string>();

                        for (int i = 0; i < propertiesCount; ++i)
                        {
                            ShaderUtil.ShaderPropertyType currPropertyType = ShaderUtil.GetPropertyType(material.shader, i);

                            if (typeLookingFor == currPropertyType)
                            {
                                properties.Add(ShaderUtil.GetPropertyName(material.shader, i));
                            }
                        }

                        if (propertyIndex == -1)
                        {
                            propertyIndex = 0;

                            for (int i = 0; i < properties.Count; ++i)
                            {
                                if (string.Equals(properties[i], propertyProperty.stringValue))
                                {
                                    propertyIndex = i;
                                    break;
                                }
                            }
                        }

                        propertyIndex = EditorGUILayout.Popup("Properties", propertyIndex, properties.ToArray());

                        if (propertyIndex > -1)
                        {
                            if (propertyIndex < properties.Count)
                            {
                                propertyProperty.stringValue = properties[propertyIndex];
                            }
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(CustomTarget);
            }
        }

        private void GatherProperties()
        {
            graphicProperty = serializedObject.FindProperty("graphic");
            instantiateMaterialProperty = serializedObject.FindProperty("instantiateMaterial");
            materialPropertyTypeProperty = serializedObject.FindProperty("materialPropertyType");
            propertyProperty = serializedObject.FindProperty("property");
        }

        private ShaderUtil.ShaderPropertyType TypeToShaderType(MaterialPropertyType type)
        {
            switch (type)
            {
                case MaterialPropertyType.Color:
                    {
                        return ShaderUtil.ShaderPropertyType.Color;
                    }

                case MaterialPropertyType.Float:
                    {
                        return ShaderUtil.ShaderPropertyType.Float;
                    }

                case MaterialPropertyType.Vector:
                    {
                        return ShaderUtil.ShaderPropertyType.Vector;
                    }
            }

            return default;
        }
    }
}
