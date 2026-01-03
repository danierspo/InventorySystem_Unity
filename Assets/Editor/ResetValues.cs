using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ResetValues
{
    static ResetValues()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            var items = Resources.FindObjectsOfTypeAll<InventoryItem>();
            foreach (var item in items)
            {
                item.ResetCount();
            }
        }
    }
}
