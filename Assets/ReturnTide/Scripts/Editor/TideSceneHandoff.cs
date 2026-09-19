using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ReturnTide.Editor {
    [InitializeOnLoad]
    public static class TideSceneHandoff {
        static TideSceneHandoff(){EditorApplication.update+=Update;}
        static void Update(){
            if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            const string request="Temp/ReturnTide.open";
            if(!File.Exists(request)||!File.Exists("Assets/ReturnTide/Workshop/Scenes/Workshop.unity"))return;
            // Never replace a scene containing unsaved user work.
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)return;
            File.Delete(request);EditorSceneManager.OpenScene("Assets/ReturnTide/Workshop/Scenes/Workshop.unity");
            var game=Object.FindObjectOfType<ReturnTide.Workshop.WorkshopGame>();if(game)Selection.activeGameObject=game.gameObject;
            SceneView.lastActiveSceneView?.LookAt(new Vector3(0,0,8),Quaternion.Euler(40,0,0),24);
        }
    }
}

