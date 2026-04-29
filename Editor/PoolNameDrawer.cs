#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using ZJM_PoolSystem.Runtime;

[CustomPropertyDrawer(typeof(PoolNameAttribute))]
public class PoolNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            // 通过反射获取所有池名称常量
            List<string> options = new List<string>();
            var poolNamesType = typeof(ZJM_PoolSystem.Generated.PoolNames);

           
            var fields = poolNamesType.GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy
            );

            foreach (var field in fields)
            {
                // 确保只取 Literal（常量）字段，过滤掉 static 变量
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    options.Add((string)field.GetValue(null));
                }
            }

            if (options.Count > 0)
            {
                int index = options.IndexOf(property.stringValue);
                if (index == -1) index = 0;
                index = EditorGUI.Popup(position, label.text, index, options.ToArray());
                property.stringValue = options[index];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif