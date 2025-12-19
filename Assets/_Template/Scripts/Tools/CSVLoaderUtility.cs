#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CSVLoaderUtility
{
    public static void LoadCSVToScriptableObject<T>(TextAsset csvFile, T scriptableObject) where T : ScriptableObject
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file is missing!");
            return;
        }

        Type objectType = scriptableObject.GetType();
        FieldInfo listField = objectType.GetField("Data", BindingFlags.Public | BindingFlags.Instance);

        if (listField == null)
        {
            Debug.LogError($"Field 'Data' not found in {objectType.Name}");
            return;
        }

        if (!listField.FieldType.IsGenericType || listField.FieldType.GetGenericTypeDefinition() != typeof(List<>))
        {
            Debug.LogError($"'{listField.Name}' must be a List<T>");
            return;
        }

        IList dataList = (IList)listField.GetValue(scriptableObject);
        Type itemType = listField.FieldType.GetGenericArguments()[0];

        dataList.Clear();

        string[] lines = csvFile.text.Split('\n');
        string[] headers = lines[0].Trim().Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Trim().Split(',');
            if (values.Length != headers.Length) continue;

            object item = Activator.CreateInstance(itemType);
            for (int j = 0; j < headers.Length; j++)
            {
                FieldInfo field = itemType.GetField(headers[j], BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object convertedValue = ConvertValue(values[j], field.FieldType);
                    field.SetValue(item, convertedValue);
                }
            }
            dataList.Add(item);
        }

        listField.SetValue(scriptableObject, dataList);
        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();
        Debug.Log("✅ CSV Loaded Successfully!");
    }

    private static object ConvertValue(string value, Type targetType)
    {

        if (string.IsNullOrEmpty(value))
            return "";

        try
        {
            if (targetType == typeof(int))
            {
                return int.Parse(value, CultureInfo.InvariantCulture);
            }
            if (targetType == typeof(float))
            {
                return float.Parse(value, CultureInfo.InvariantCulture);
            }
            if (targetType == typeof(double))
            {
                return double.Parse(value, CultureInfo.InvariantCulture);
            }
            if (targetType == typeof(bool))
                return bool.Parse(value);
            if (targetType == typeof(string))
                return value;
            if (targetType.IsEnum)
            {
                if (Enum.TryParse(targetType, value, out object result))
                    return result;

                Debug.LogError($"Error '{value}' To {targetType.Name}");
                return Enum.GetValues(targetType).GetValue(0);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error '{value}' To {targetType.Name}: {ex.Message}");
            return Activator.CreateInstance(targetType);
        }
    }
}
#endif