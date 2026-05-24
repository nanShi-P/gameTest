using UnityEditor;
using UnityEngine;
using Game.Save;

namespace Game.EditorTools
{
    public static class SaveMenu
    {
        [MenuItem("Alchemist/Save/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(SaveSystem.GetSavePath());
        }

        [MenuItem("Alchemist/Save/Delete Save")]
        public static void DeleteSave()
        {
            SaveSystem.Delete();
            Debug.Log($"[SaveMenu] Deleted save at {SaveSystem.GetSavePath()}");
        }
    }
}
