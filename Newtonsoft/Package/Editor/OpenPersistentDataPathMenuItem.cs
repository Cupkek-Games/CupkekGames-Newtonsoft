using UnityEngine;
using UnityEditor;
using System.IO;

namespace CupkekGames.Newtonsoft.Editor
{
    public class OpenPersistentDataPathMenuItem
    {
        [MenuItem("Tools/CupkekGames/Open Persistent Data Path", false, 6)]
        public static void OpenPersistentDataPath()
        {
            string path = Application.persistentDataPath;

            // Ensure the directory exists
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the folder in Windows Explorer
            EditorUtility.RevealInFinder(path);
        }
    }
}