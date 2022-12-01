using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
public class EnumMaskDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EnumMaskAttribute enumMaskAttribute = attribute as EnumMaskAttribute;
        enumMaskAttribute.Enum = EditorGUI.EnumFlagsField(position, label, enumMaskAttribute.Enum);
 
        if (enumMaskAttribute.EnumType == typeof(Enum))
        {
            Debug.Log(enumMaskAttribute.Enum);
            property.intValue = Convert.ToInt32(Enum.Parse(enumMaskAttribute.EnumType, Enum.GetName(enumMaskAttribute.EnumType, enumMaskAttribute.Enum)));
        }
        else
        {
            property.intValue = Convert.ToInt32(enumMaskAttribute.Enum);
        }

        EditorGUI.EndProperty();
    }
}