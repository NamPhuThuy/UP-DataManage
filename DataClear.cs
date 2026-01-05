using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace NamPhuThuy.DataManage
{
    public class DataClear
    {
#if UNITY_EDITOR
        [MenuItem("NamPhuThuy/DataManage/Clear Data", false, int.MaxValue)]
        static void Clear()
        {
            Debug.Log("Clear Player Data");
            PlayerPrefs.DeleteAll();
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
            foreach (string filePath in filePaths)
                if (filePath.Contains(DataConst.FILES_EXTENSION))
                    File.Delete(filePath);
        }
#endif
    }
}
