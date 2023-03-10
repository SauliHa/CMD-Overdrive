using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manticore
{

    /// <summary>
    /// Utility class for easier scene management. 
    /// </summary>
    public class SceneUtils : MonoBehaviour
    {

        public static string NameFromIndex(int BuildIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
            int slash = path.LastIndexOf('/');
            string name = path.Substring(slash + 1);
            int dot = name.LastIndexOf('.');
            return name.Substring(0, dot);
        }


        public static int GetSceneIndexByName(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string testedScreen = NameFromIndex(i);
                //print("sceneIndexFromName: i: " + i + " sceneName = " + testedScreen);
                if (testedScreen == sceneName)
                    return i;
            }
            return -1;
        }
    }
}