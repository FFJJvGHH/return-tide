using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace ReturnTide.Editor {
    // Batch smoke test: actual Play Mode, serialized scene and gameplay components.
    [InitializeOnLoad]
    public static class TideValidation {
        static double next;
        static int stage;
        static string Evidence=>Path.GetFullPath("Evidence");
        static TideValidation(){EditorApplication.update+=Tick;}
        public static void Run(){
            Directory.CreateDirectory(Evidence);File.WriteAllText(Path.Combine(Evidence,"validation.txt"),"");File.Delete(Path.Combine(Evidence,"COMPLETE.txt"));TideBuilder.Build();
            Check(Object.FindObjectsOfType<TideLesion>().Length==4,"Four removable lesions exist");
            Check(Object.FindObjectsOfType<TideEnemy>().Length==4,"Four enemies exist");Check(AssetDatabase.LoadAssetAtPath<Material>("Assets/ReturnTide/Materials/Glow.mat").IsKeywordEnabled("_EMISSION"),"Bioluminescent material has emission enabled");
            Check(Object.FindObjectOfType<TideMerchant>()!=null,"Merchant exists");
            Check(Object.FindObjectsOfType<Renderer>().All(r=>r.sharedMaterials.All(m=>m!=null)),"No missing materials");
            SessionState.SetBool("TideSmoke",true);SessionState.SetInt("TideStage",0);
            EditorApplication.isPlaying=true;
        }
        static void Check(bool value,string label){if(!value)throw new Exception("TIDE_SMOKE_FAIL: "+label);Debug.Log("TIDE_SMOKE_PASS: "+label);File.AppendAllText(Path.Combine(Evidence,"validation.txt"),"PASS "+label+"\n");}
        static void Tick(){
            if(!SessionState.GetBool("TideSmoke",false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.8;
            try{
                var g=TideGame.Instance;if(!g)return;stage=SessionState.GetInt("TideStage",0);
                if(stage==0){Capture(g,"01-title.png");g.replayIntroEveryTime=true;g.Begin();next=EditorApplication.timeSinceStartup+3;}
                if(stage==1){Capture(g,"02-fishing.png");next=EditorApplication.timeSinceStartup+1.5;}
                if(stage==2){if(!g.Playing){Capture(g,"02b-radiant-fish.png");next=EditorApplication.timeSinceStartup+4;return;}Check(g.Playing,"Opening transitions into gameplay");Check(g.player.Health==g.settings.maxHealth,"Player starts healthy");Check(!g.EverCollected&&g.hud.samples.text=="","Inventory hidden before collection");Capture(g,"03-gameplay.png");}
                if(stage==3){g.player.Teleport(new Vector3(0,.6f,15));g.cameraRig.Snap();next=EditorApplication.timeSinceStartup+1;}
                if(stage==4){Capture(g,"04-coral-garden.png");var e=Object.FindObjectOfType<TideEnemy>();e.Hit(Vector3.forward);e.Hit(Vector3.forward);var merchant=Object.FindObjectOfType<TideMerchant>();merchant.Trade(g);Check(!g.HasStabilizer,"Trade requires samples");var lesions=Object.FindObjectsOfType<TideLesion>().Where(l=>!l.isCore).ToArray();foreach(var l in lesions)l.Complete(g);Check(g.CutCount==3,"All three lesions can be excised");}
                if(stage==5){var samples=Object.FindObjectsOfType<TidePickup>();Check(samples.Length==3,"Excision creates three physical sample pickups");g.player.Teleport(samples[0].transform.position-Vector3.up*.5f);next=EditorApplication.timeSinceStartup+.5;}
                if(stage==6){Check(g.Samples>=1&&g.EverCollected,"Proximity pickup reveals inventory");var samples=Object.FindObjectsOfType<TidePickup>();foreach(var p in samples){g.AddSample(p.amount);Object.Destroy(p.gameObject);}Object.FindObjectOfType<TideMerchant>().Trade(g);Check(g.HasStabilizer&&g.Samples==0,"Merchant exchanges three samples for stabilizer");var core=Object.FindObjectsOfType<TideLesion>().First(l=>l.isCore);core.Complete(g);Check(g.CoreCured&&g.exitGlow.gameObject.activeSelf,"Core cure opens return portal");g.player.Teleport(new Vector3(0,.6f,40));g.cameraRig.Snap();}
                if(stage==7){Capture(g,"05-core-cured.png");g.player.Damage(2);g.Respawn();Check(g.player.Health==g.settings.maxHealth,"Checkpoint restores health");g.ReturnHome();next=EditorApplication.timeSinceStartup+2;}
                if(stage==8){Check(g.hud.endingGroup.activeSelf&&!g.Playing,"Return reaches lakeside epilogue");Capture(g,"06-return.png");SessionState.SetBool("TideSmoke",false);EditorApplication.isPlaying=false;EditorApplication.delayCall+=Finish;}
                SessionState.SetInt("TideStage",stage+1);
            }catch(Exception ex){Debug.LogException(ex);SessionState.SetBool("TideSmoke",false);EditorApplication.Exit(1);}
        }
        static void Capture(TideGame game,string file){
            var cam=game.cameraRig.GetComponent<Camera>();var canvas=game.hud.GetComponent<Canvas>();var prev=canvas.renderMode;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();
            var active=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Evidence,file),tex.EncodeToPNG());
            cam.targetTexture=null;RenderTexture.active=active;canvas.renderMode=prev;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
        }
        static void Finish(){
            try{
                PlayerPrefs.DeleteKey("ReturnTide.IntroSeen");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ReturnTide/Scenes/ReturnTide.unity"},locationPathName="Build/ReturnTide.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                Check(report.summary.result==BuildResult.Succeeded,"Windows player builds successfully");
                File.WriteAllText(Path.Combine(Evidence,"COMPLETE.txt"),"RETURN_TIDE_SMOKE_AND_BUILD_OK");EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
        }
    }
}

