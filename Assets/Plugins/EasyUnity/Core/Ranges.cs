using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct FloatRange
{
    public float Min, Max;
    public FloatRange(float min, float max) => (Min, Max) = (min, max);
}

[Serializable]
public struct IntRange
{
    public int Min, Max;
    public IntRange(int min, int max) => (Min, Max) = (min, max);
    public static implicit operator FloatRange(IntRange range) => new FloatRange(range.Min, range.Max);
}

[Serializable]
public struct ByteRange
{
    public byte Min, Max;
    public ByteRange(byte min, byte max) => (Min, Max) = (min, max);
    public static implicit operator IntRange(ByteRange range) => new IntRange(range.Min,range.Max);
    public static implicit operator FloatRange(ByteRange range) => new FloatRange(range.Min, range.Max);
}

public static class RangeHelper
{
    public static float ToAbsolute(this float f, FloatRange range) => ((range.Max - range.Min) * f) + range.Min;
    public static int ToAbsolute(this int i, IntRange range) => ((range.Max - range.Min) * i) + range.Min;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FloatRange))]
[CustomPropertyDrawer(typeof(IntRange))]
[CustomPropertyDrawer(typeof(ByteRange))]
public class RangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        object[] attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
        if (attributes?.Length > 0)
            label.tooltip = (attributes[0] as TooltipAttribute).tooltip;

        EditorGUI.BeginProperty(rect, label, property);
        rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float totalWidth = rect.width;
        float labelWidth = 30;
        float propertyWidth = (totalWidth - (labelWidth * 2f)) / 2f;

        rect.width = labelWidth;
        EditorGUI.LabelField(rect, "Min");

        rect.x += rect.width;
        rect.width = propertyWidth;
        EditorGUI.PropertyField(rect, property.FindPropertyRelative("Min"), GUIContent.none);

        rect.x += rect.width;
        rect.width = labelWidth;
        EditorGUI.LabelField(rect, "Max");

        rect.x += rect.width;
        rect.width = propertyWidth;
        EditorGUI.PropertyField(rect, property.FindPropertyRelative("Max"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif