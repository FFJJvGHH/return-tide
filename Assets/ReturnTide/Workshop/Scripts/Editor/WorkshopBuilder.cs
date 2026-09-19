using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace ReturnTide.Workshop.Editor {
 public static class WorkshopBuilder {
  public const string Root="Assets/ReturnTide/Workshop";
  public const string ScenePath=Root+"/Scenes/Workshop.unity";
  static Dictionary<string,Material> mats=new Dictionary<string,Material>();static Mesh stone;static WorkshopGame game;
  [MenuItem("Tools/归潮 Return Tide/生成解剖室",false,1)]
  public static void Generate(){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;if(File.Exists(ScenePath)){Directory.CreateDirectory(Root+"/Scenes/Backups");AssetDatabase.CopyAsset(ScenePath,Root+"/Scenes/Backups/Workshop_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity");}Build();}
  public static void Build(){
   foreach(var folder in new[]{"Materials","Prefabs","Settings","Scenes"})Directory.CreateDirectory(Root+"/"+folder);AssetDatabase.Refresh();Palette();
   stone=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/ReturnTide/Meshes/FacetedStone.asset");if(!stone)throw new Exception("Missing base faceted mesh");
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   game=Node("Workshop · 流程 / 经济 / 存档").AddComponent<WorkshopGame>();
   var balance=AssetDatabase.LoadAssetAtPath<WorkshopBalance>(Root+"/Settings/Balance.asset");if(!balance){balance=ScriptableObject.CreateInstance<WorkshopBalance>();AssetDatabase.CreateAsset(balance,Root+"/Settings/Balance.asset");}game.balance=balance;game.audioSource=game.gameObject.AddComponent<AudioSource>();
   Room();RefineRoom();Lights();BuildResearcher();BuildCamera();BuildSpecimen();BuildTools();BuildUI();
   var ambiance=Node("夜间声景").AddComponent<AudioSource>();ambiance.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ReturnTide/Audio/Belly_Ambience.wav");ambiance.loop=true;ambiance.playOnAwake=true;ambiance.volume=.35f;
   EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
   PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
   AssetDatabase.SaveAssets();Selection.activeGameObject=game.gameObject;SceneView.lastActiveSceneView?.LookAt(new Vector3(0,1,0),Quaternion.Euler(42,-15,0),10);
   Debug.Log("WORKSHOP_BUILD_OK");
  }
  static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
  static void Palette(){
   mats.Clear();string[] names={"Wall","Trim","TileA","TileB","Table","Copper","Ivory","Ink","Teal","Coral","Gold","Skin","Hair","Glow","Dark","Leather","FishSkin","FishDark","Pearl","Flesh","Organ","Bone","Core","Metal","Glass","WarmGlow","Window","Paper"};
   string[] colors={"182A35","2D4851","314953","2B414B","325A5B","AA7951","D9D5BC","1C2B3E","3C9187","BD7972","C9A767","EBC2AD","9EBBB9","65E6BE","142632","705441","427E80","183F49","ABC8BD","AB666D","753F62","DBC7A2","398D75","657984","417C79","FFE6B0","2D6177","C9C2A6"};
   for(int i=0;i<names.Length;i++){string path=Root+"/Materials/"+names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",C(colors[i]));m.SetFloat("_Smoothness",names[i]=="Metal"||names[i]=="Table"?.45f:.18f);m.SetFloat("_Metallic",names[i]=="Copper"||names[i]=="Metal"?.45f:0);bool emissive=names[i]=="Core"||names[i]=="Glow"||names[i]=="WarmGlow"||names[i]=="Window";m.SetColor("_EmissionColor",emissive?C(colors[i])*(names[i]=="Core"?.60f:names[i]=="WarmGlow"?1.6f:names[i]=="Window"?.22f:.60f):Color.black);m.globalIlluminationFlags=emissive?MaterialGlobalIlluminationFlags.BakedEmissive:MaterialGlobalIlluminationFlags.None;if(emissive)m.EnableKeyword("_EMISSION");else m.DisableKeyword("_EMISSION");mats[names[i]]=m;EditorUtility.SetDirty(m);}
  }
  static void Room(){
   var room=Node("01 · 解剖室 / 独立可编辑道具").transform;
   Box("地板基层",room,new Vector3(0,-.16f,1),new Vector3(13,.3f,10),"Dark");
   for(int x=-6;x<=6;x++)for(int z=-3;z<6;z++)Box("石砖",room,new Vector3(x,0,z),new Vector3(.97f,.045f,.97f),(x+z)%2==0?"TileA":"TileB");
   Box("后墙",room,new Vector3(0,2.7f,5.35f),new Vector3(13,5.4f,.25f),"Wall");
   for(int x=-6;x<=6;x++)Box("墙板竖缝",room,new Vector3(x,2.6f,5.17f),new Vector3(.035f,5.0f,.045f),"Trim");
   Box("墙裙",room,new Vector3(0,1.05f,5.10f),new Vector3(13,.13f,.10f),"Copper");Box("左墙",room,new Vector3(-6.4f,2.7f,1),new Vector3(.2f,5.4f,8.5f),"Wall");
   // A tall round-topped window; mullions and moon belong to the room, not the HUD.
   var window=Node("湖岸窗",room,new Vector3(-.4f,3.2f,5.0f)).transform;
   Shape("窗洞",window,Vector3.zero,new Vector3(1.35f,1.65f,.06f),"Copper");Shape("夜光玻璃",window,new Vector3(0,0,-.055f),new Vector3(1.23f,1.52f,.045f),"Window");
   Box("窗中框",window,new Vector3(0,0,-.12f),new Vector3(.07f,3,.08f),"Dark");Box("窗横框",window,new Vector3(0,-.25f,-.12f),new Vector3(2.4f,.06f,.08f),"Dark");
   Shape("月",window,new Vector3(.50f,.55f,-.12f),new Vector3(.26f,.26f,.025f),"WarmGlow");
   for(int i=0;i<6;i++)Box("窗外水纹",window,new Vector3(0,-.65f-i*.10f,-.12f),new Vector3(1.6f-i*.09f,.015f,.012f),"Pearl");
   var desk=Node("解剖台",room).transform;
   Box("钢制桌体",desk,new Vector3(0,.88f,0),new Vector3(6.7f,.32f,3.55f),"Dark",true);
   Box("搪瓷台面",desk,new Vector3(0,1.065f,0),new Vector3(6.75f,.075f,3.6f),"Table");
   for(int i=-1;i<=1;i+=2){Box("铜包边",desk,new Vector3(0,1.11f,i*1.79f),new Vector3(6.8f,.055f,.035f),"Copper");Box("桌侧包边",desk,new Vector3(i*3.37f,1.11f,0),new Vector3(.035f,.055f,3.6f),"Copper");}
   foreach(float x in new[]{-2.8f,2.8f})foreach(float z in new[]{-1.25f,1.25f}){Box("桌腿",desk,new Vector3(x,.45f,z),new Vector3(.15f,.9f,.15f),"Metal");Shape("铜脚",desk,new Vector3(x,.1f,z),new Vector3(.13f,.11f,.13f),"Copper");}
   Box("解剖衬垫",desk,new Vector3(-.35f,1.12f,0),new Vector3(4.8f,.035f,2.5f),"Pearl");
   for(int i=0;i<24;i++)Box("台面刻度",desk,new Vector3(-2.6f+i*.19f,1.145f,1.16f),new Vector3(.012f,.005f,i%5==0?.13f:.06f),"Ink");
   game.specimenAnchor=Node("标本落点 · 可编辑",desk,new Vector3(-.25f,1.16f,0)).transform;
   var tray=Node("右侧回收托盘",desk,new Vector3(2.85f,1.18f,-.10f)).transform;game.outputTray=tray;
   Shape("金属盘沿",tray,Vector3.zero,new Vector3(.70f,.08f,1.05f),"Metal");Shape("盘内",tray,new Vector3(0,.065f,0),new Vector3(.61f,.045f,.95f),"Dark");
   for(int i=0;i<4;i++)Box("前侧抽屉",desk,new Vector3(-2.35f+i*1.57f,.70f,-1.61f),new Vector3(1.40f,.34f,.12f),"Table");
   for(int i=0;i<4;i++)Rod("抽屉拉手",desk,new Vector3(-2.55f+i*1.57f,.74f,-1.71f),new Vector3(-2.15f+i*1.57f,.74f,-1.71f),.035f,"Copper");
   // Adjustable surgical lamp.
   var lamp=Node("关节手术灯",room).transform;
   Rod("立柱",lamp,new Vector3(-3.8f,0,1.5f),new Vector3(-3.8f,3.5f,1.5f),.07f,"Metal");Rod("灯臂",lamp,new Vector3(-3.8f,3.5f,1.5f),new Vector3(-1.4f,4.2f,1.3f),.06f,"Copper");
   Shape("灯罩",lamp,new Vector3(-1.4f,4.12f,1.3f),new Vector3(.62f,.20f,.52f),"Dark");Shape("灯面",lamp,new Vector3(-1.4f,3.99f,1.3f),new Vector3(.52f,.045f,.44f),"WarmGlow");
   for(int row=0;row<3;row++){
    Box("药品架",room,new Vector3(-4.45f,1.2f+row*1.1f,4.85f),new Vector3(2.25f,.12f,.65f),"Leather");
    for(int j=0;j<5;j++){Vector3 p=new Vector3(-5.35f+j*.43f,1.45f+row*1.1f,4.6f);Jar(room,p,.16f,.38f,(row+j)%3==0?"Glow":"Glass");}
   }
   var cabinet=Box("器械柜",room,new Vector3(-4.8f,.7f,2.9f),new Vector3(2,1.4f,1.2f),"Table");
   for(int i=0;i<3;i++){Box("柜抽屉",room,new Vector3(-4.8f,.35f+i*.37f,2.25f),new Vector3(1.8f,.30f,.06f),"Trim");Rod("柜把手",room,new Vector3(-5.05f,.35f+i*.37f,2.19f),new Vector3(-4.55f,.35f+i*.37f,2.19f),.025f,"Copper");}
   // Physical peeler and unlocked laser dock.
   var peeler=Node("剥皮机 · 升级可见",room,new Vector3(-4.3f,1.48f,2.8f));Box("机身",peeler.transform,Vector3.zero,new Vector3(1.1f,.5f,.75f),"Metal");for(int i=0;i<3;i++)Rod("滚筒",peeler.transform,new Vector3(-.4f,.3f,-.24f+i*.24f),new Vector3(.4f,.3f,-.24f+i*.24f),.1f,"Copper");game.peelerDisplay=peeler.transform;peeler.SetActive(false);
   var laser=Node("激光电源 · 升级可见",room,new Vector3(3.9f,.5f,-.5f));Box("电源箱",laser.transform,Vector3.zero,new Vector3(.75f,1,.85f),"Metal");Jar(laser.transform,new Vector3(0,.68f,0),.18f,.35f,"Core");game.laserDisplay=laser.transform;laser.SetActive(false);
   // Merchant occupies a real side counter. No introductory monologue.
   var trade=Node("鱼贩子柜台",room,new Vector3(4.2f,0,2.6f)).transform;
   Box("柜台",trade,new Vector3(0,.65f,0),new Vector3(2.8f,1.3f,1.5f),"Leather");Box("铜台面",trade,new Vector3(0,1.33f,0),new Vector3(3,.08f,1.65f),"Copper");
   var merchant=Model(Root+"/Models/Fishmonger_Refined.fbx",trade,new Vector3(.1f,0,1.25f));merchant.transform.rotation=Quaternion.Euler(0,180,0);merchant.transform.localScale=Vector3.one*1.15f;
   var bell=Shape("收购铃",trade,new Vector3(-.85f,1.53f,-.2f),new Vector3(.25f,.18f,.25f),"Gold");Hot(bell,WorkshopAction.Trade,"交易",new Vector3(.6f,.5f,.6f));
   for(int i=0;i<3;i++)Box("账本",trade,new Vector3(.65f,1.4f+i*.07f,0),new Vector3(.62f,.06f,.75f),i%2==0?"Ink":"Paper");
   Rod("天平支架",trade,new Vector3(-.1f,1.4f,.25f),new Vector3(-.1f,2.12f,.25f),.035f,"Metal");Rod("秤杆",trade,new Vector3(-.6f,2.1f,.25f),new Vector3(.4f,2.1f,.25f),.03f,"Gold");
   foreach(float x in new[]{-.6f,.4f}){Rod("吊链",trade,new Vector3(x,2.1f,.25f),new Vector3(x,1.7f,.25f),.009f,"Copper");Shape("秤盘",trade,new Vector3(x,1.7f,.25f),new Vector3(.30f,.045f,.30f),"Gold");}
   var doorFrame=Node("湖岸出口",room,new Vector3(2.1f,0,5)).transform;
   Box("门洞",doorFrame,new Vector3(0,1.3f,.01f),new Vector3(1.5f,2.6f,.08f),"Window");var door=Node("门轴",doorFrame,new Vector3(-.75f,0,-.09f)).transform;game.exitDoor=door;
   Box("木门",door,new Vector3(.75f,1.3f,0),new Vector3(1.5f,2.6f,.15f),"Table");Shape("门把手",door,new Vector3(1.3f,1.2f,-.12f),Vector3.one*.065f,"Gold");
   for(int i=0;i<5;i++)Box("门板",door,new Vector3(.15f+i*.3f,1.3f,-.085f),new Vector3(.013f,2.5f,.015f),"Copper");
   var crate=Node("来货箱",room,new Vector3(-4.4f,.30f,-1.4f)).transform;Box("木箱",crate,Vector3.zero,new Vector3(1.5f,.6f,1.8f),"Leather");for(int i=0;i<4;i++)Box("箍条",crate,new Vector3(0,.32f,-.7f+i*.45f),new Vector3(1.5f,.06f,.10f),"Copper");
   var received=Shape("接货铃",room,new Vector3(-3.65f,1.23f,-1.4f),new Vector3(.22f,.15f,.22f),"Gold");Hot(received,WorkshopAction.Next,"接收下一条",new Vector3(.6f,.5f,.6f));
  }
  static void RefineRoom(){
   var room=GameObject.Find("01 · 解剖室 / 独立可编辑道具").transform;var desk=room.Find("解剖台");
   foreach(var r in desk.GetComponentsInChildren<Renderer>())if(new[]{"钢制桌体","搪瓷台面","铜包边","桌侧包边","金属盘沿","盘内"}.Contains(r.name))r.enabled=false;
   Model(Root+"/Models/Surgical_Bench.fbx",desk,new Vector3(0,.88f,0));Model(Root+"/Models/Surgical_Tray.fbx",game.outputTray,Vector3.zero);
   var isolation=Node("隔离盘 · 毒囊落点",desk,new Vector3(-3.12f,1.18f,-.18f)).transform;game.isolationTray=isolation;var isolationModel=Model(Root+"/Models/Surgical_Tray.fbx",isolation,Vector3.zero);isolationModel.transform.localScale=Vector3.one*.74f;
   for(int i=0;i<3;i++)Box("隔离警示刻纹",isolation,new Vector3(-.32f+i*.16f,.04f,-.45f),new Vector3(.07f,.008f,.15f),"Gold");
   // Vessel silhouettes with neck, cap and label bands replace decorative diamonds.
   foreach(var t in room.GetComponentsInChildren<Transform>().Where(t=>t.name=="标本瓶").ToArray()){
    Vector3 p=t.position;float h=t.localScale.y;t.GetComponent<Renderer>().enabled=false;var bottle=Model(Root+"/Models/Specimen_Bottle.fbx",room,Vector3.zero);bottle.transform.position=p-Vector3.up*h;bottle.transform.localScale=Vector3.one*.9f;
   }
   foreach(var r in room.GetComponentsInChildren<Renderer>())if(r.name=="瓶塞")r.enabled=false;
   var fixtures=Node("工作区管线与仪表",room).transform;
   Rod("回水铜管",fixtures,new Vector3(-5.9f,.4f,4.9f),new Vector3(-5.9f,4.8f,4.9f),.055f,"Copper");Rod("横向气管",fixtures,new Vector3(-5.9f,4.8f,4.9f),new Vector3(4.9f,4.8f,4.9f),.045f,"Copper");
   for(int i=0;i<8;i++)Box("管线固定卡",fixtures,new Vector3(-5.3f+i*1.35f,4.8f,4.88f),new Vector3(.06f,.17f,.11f),"Metal");
   var gauge=Node("压力表",fixtures,new Vector3(-2.9f,1.90f,4.86f)).transform;Shape("表壳",gauge,Vector3.zero,new Vector3(.35f,.35f,.10f),"Copper");Shape("表盘",gauge,new Vector3(0,0,-.10f),new Vector3(.29f,.29f,.025f),"Paper");
   for(int i=0;i<11;i++){float a=(25+i*28)*Mathf.Deg2Rad;Rod("刻度",gauge,new Vector3(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.22f,-.14f),new Vector3(Mathf.Cos(a)*.255f,Mathf.Sin(a)*.255f,-.14f),.008f,"Ink");}Rod("表针",gauge,new Vector3(0,0,-.15f),new Vector3(-.12f,.13f,-.15f),.012f,"Coral");
   var board=Node("手绘解剖挂图",room,new Vector3(-2.95f,3.55f,4.94f)).transform;Box("挂图纸",board,Vector3.zero,new Vector3(1.5f,1.75f,.035f),"Paper");
   for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24,b=(i+1)*Mathf.PI*2/24;Rod("轮廓线",board,new Vector3(Mathf.Cos(a)*.59f,Mathf.Sin(a)*.25f,-.025f),new Vector3(Mathf.Cos(b)*.59f,Mathf.Sin(b)*.25f,-.025f),.007f,"Ink");}
   Rod("脊柱线",board,new Vector3(-.5f,0,-.03f),new Vector3(.5f,0,-.03f),.009f,"Coral");for(int i=0;i<8;i++){float x=-.4f+i*.11f;Rod("肋线",board,new Vector3(x,-.20f,-.03f),new Vector3(x, .20f,-.03f),.005f,"Ink");}
   // Cloth folds, small clipped notes and instrument rests stay outside hit regions.
   for(int i=0;i<5;i++)Box("折叠擦拭布",desk,new Vector3(-2.45f,1.17f+i*.012f,1.36f),new Vector3(.66f-i*.015f,.014f,.45f),i%2==0?"Ivory":"Pearl");
   for(int i=0;i<3;i++){var paper=Box("交接票据",desk,new Vector3(2.7f,1.16f,1.43f-i*.07f),new Vector3(.52f,.009f,.31f),"Paper");paper.transform.localRotation=Quaternion.Euler(0,i*8,0);}
   for(int i=0;i<3;i++)Box("工具承托凹槽",desk,new Vector3(-1.35f+i*.73f,1.12f,-1.48f),new Vector3(.38f,.025f,.57f),"Dark");
  }
  static void Lights(){
   var root=Node("02 · 冷窗光 / 暖台灯 / 体积氛围").transform;
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=C("527281")*.55f;RenderSettings.ambientEquatorColor=C("344C58")*.6f;RenderSettings.ambientGroundColor=C("17222F");RenderSettings.fog=false;
   var sun=Node("月光",root).AddComponent<Light>();sun.type=LightType.Directional;sun.color=C("97C8DB");sun.intensity=.65f;sun.transform.rotation=Quaternion.Euler(48,155,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.65f;
   Spot("台灯聚光",root,new Vector3(-1.4f,3.92f,1.3f),new Vector3(0,1,0),C("FFE5AE"),14,80);
   Point("窗边冷光",root,new Vector3(-.4f,3.3f,3.8f),C("83C4DF"),5,9);
   Point("柜台暖光",root,new Vector3(4.0f,3.8f,2.5f),C("FFBC78"),6,7);
   Point("正面柔光",root,new Vector3(0,3,-4),C("9CC2C0"),3.5f,10);
   Point("人物柔光",root,new Vector3(0,3.2f,.7f),C("FFDDB7"),2.1f,4);
   var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/URP-HighFidelity.asset");QualitySettings.renderPipeline=pipeline;pipeline.shadowDistance=35;
   var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"/Settings/Lighting.asset");if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Root+"/Settings/Lighting.asset");var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.35f);bloom.threshold.Override(1);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.25f);vignette.smoothness.Override(.7f);var grade=profile.Add<ColorAdjustments>(true);grade.postExposure.Override(.35f);grade.saturation.Override(-5);foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);}
   var volume=Node("调色与辉光",root).AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
  }
  static void BuildResearcher(){
   var actor=Node("科研员 · 可编辑关节与动作",null,new Vector3(-.4f,0,2.22f));actor.transform.rotation=Quaternion.Euler(0,180,0);
   var model=Model(Root+"/Models/Mio_Refined.fbx",actor.transform,Vector3.zero);var anim=actor.AddComponent<WorkshopAnimator>();game.researcher=anim;anim.model=model.transform;
   var head=Node("HeadPivot · 注视 / 点头",model.transform,new Vector3(0,1.6f,0));anim.headPivot=head.transform;
   var transforms=model.GetComponentsInChildren<Transform>().ToArray();var eyes=new List<Transform>();
   foreach(var t in transforms){if(t==head.transform||t==model.transform)continue;string name=t.name;
    if(name.StartsWith("Sleeve")||name.StartsWith("Glove")||name.StartsWith("Scalpel")){t.gameObject.SetActive(false);continue;}
    if(name.StartsWith("Head")||name.StartsWith("Hair")||name.StartsWith("Fringe")||name.StartsWith("Eye")||name.StartsWith("Cheek")||name.StartsWith("Goggles")||name.StartsWith("Mouth")){t.SetParent(head.transform,true);if(name=="Eye"||name.StartsWith("Eye."))eyes.Add(t);}
   }
   anim.eyes=eyes.ToArray();foreach(var r in head.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("Eye")||r.name.StartsWith("Cheek")||r.name.StartsWith("Mouth"))r.shadowCastingMode=ShadowCastingMode.Off;anim.leftShoulder=Node("左肩",actor.transform,new Vector3(-.27f,1.42f,0)).transform;anim.rightShoulder=Node("右肩",actor.transform,new Vector3(.27f,1.42f,0)).transform;
   anim.leftUpper=Cylinder("左上臂",actor.transform,"Ivory").transform;anim.leftForearm=Cylinder("左前臂",actor.transform,"Ivory").transform;anim.rightUpper=Cylinder("右上臂",actor.transform,"Ivory").transform;anim.rightForearm=Cylinder("右前臂",actor.transform,"Ivory").transform;
   anim.leftHand=Model(Root+"/Models/Research_Glove.fbx",actor.transform,new Vector3(-.4f,1.2f,.7f)).transform;anim.rightHand=Model(Root+"/Models/Research_Glove.fbx",actor.transform,new Vector3(.4f,1.2f,.7f)).transform;anim.leftHand.localScale=anim.rightHand.localScale=Vector3.one*.14f;
   PrefabUtility.SaveAsPrefabAsset(actor,Root+"/Prefabs/Researcher_Working.prefab");
  }
  static void BuildCamera(){
   var root=Node("03 · 镜头机位 / 可在 Scene 中调整").transform;
   var camGO=Node("Main Camera");camGO.tag="MainCamera";var camera=camGO.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=C("101F2B");camera.fieldOfView=43;camera.nearClipPlane=.1f;camera.farClipPlane=75;camera.allowHDR=true;
   var data=camGO.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;camGO.AddComponent<AudioListener>();
   var rig=camGO.AddComponent<WorkshopCamera>();game.cameraRig=rig;
   rig.titlePose=Pose("标题 · 房间全景",root,new Vector3(9,7.2f,-11.5f),new Vector3(.3f,1.4f,1));rig.benchPose=Pose("解剖 · 固定特写",root,new Vector3(1.2f,7.8f,-6.7f),new Vector3(0,1.15f,.20f));rig.tradePose=Pose("交易 · 鱼贩子",root,new Vector3(7.2f,4.0f,-3.7f),new Vector3(3.7f,1.55f,2.8f));rig.returnPose=Pose("回归 · 湖岸门",root,new Vector3(6.8f,3.5f,-.5f),new Vector3(1.8f,1.45f,5));rig.SetView(WorkshopView.Title,true);
  }
  static Transform Pose(string name,Transform root,Vector3 position,Vector3 look){var t=Node(name,root,position).transform;t.LookAt(look);return t;}
  static void BuildSpecimen(){
   game.specimenPrefabs=new WorkshopSpecimen[4];for(int i=0;i<4;i++)CreateSpecimen(i);
  }
  static void CreateSpecimen(int family){
   string[] files={"Specimen_Reef","Specimen_Armor","Specimen_Venom","Specimen_Crystal"};string[] names={"鳃鳞鲈","锈甲魟","灯囊鳗","棘晶鳐"};string[] clues={"稳定鱼肉","厚甲 / 需要激光","毒囊 / 隔离操作","脆晶 / 保持纯度"};
   string profilePath=Root+"/Settings/"+files[family]+".asset";var profile=AssetDatabase.LoadAssetAtPath<SpecimenProfile>(profilePath);if(!profile){profile=ScriptableObject.CreateInstance<SpecimenProfile>();profile.displayName=names[family];profile.handlingClue=clues[family];profile.family=family;profile.corePrice=new[]{85,170,230,350}[family];profile.meatYield=family==0?2:1;profile.boneHits=family==1?4:family==3?2:3;profile.toxicOrgan=family==2?1:-1;profile.machinePurityLoss=family==2?18:family==3?10:0;profile.requiredOrders=family<2?0:family-1;AssetDatabase.CreateAsset(profile,profilePath);}
   var root=Node(names[family]+" · 分层标本",null,game.specimenAnchor.position);var model=Model(Root+"/Models/"+files[family]+".fbx",root.transform,Vector3.zero);
   var specimen=root.AddComponent<WorkshopSpecimen>();specimen.profile=profile;if(family==0)game.specimen=specimen;var parts=new List<WorkshopPart>();
   string[] layers={"Shell","Skin","Flesh","Bone","Organ","Crystal"};
   foreach(var t in model.GetComponentsInChildren<Transform>().ToArray()){
    int layer=Array.FindIndex(layers,p=>t.name.StartsWith(p+"_")||t.name==p);if(layer<0)continue;
    var renderers=t.GetComponentsInChildren<Renderer>();if(renderers.Length==0)continue;Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
    var wrapper=Node(t.name+" · 可操作",root.transform);wrapper.transform.position=bounds.center;t.SetParent(wrapper.transform,true);
    var part=wrapper.AddComponent<WorkshopPart>();part.specimen=specimen;part.layer=(TissueLayer)layer;var box=wrapper.AddComponent<BoxCollider>();box.size=bounds.size+Vector3.one*.025f;part.hitbox=box;
    if(layer==1||layer==2){var points=t.GetComponentsInChildren<Transform>().Where(child=>child.name.StartsWith("Cut")).OrderBy(child=>child.name).ToList();if(points.Count!=5)throw new Exception("Missing sculpted incision points: "+t.name);part.cutPath=points.ToArray();
     var line=Node("切线",wrapper.transform).AddComponent<LineRenderer>();line.sharedMaterial=mats["Gold"];line.startWidth=line.endWidth=.015f;line.useWorldSpace=true;line.positionCount=5;for(int j=0;j<5;j++)line.SetPosition(j,points[j].position);part.cutGuide=line;part.cutCursor=Shape("切口起点",wrapper.transform,Vector3.zero,Vector3.one*.045f,"Glow").transform;part.cutCursor.position=points[0].position;
    }
    parts.Add(part);wrapper.SetActive(layer==1);
   }
   specimen.parts=parts.ToArray();specimen.game=game;
   specimen.tentacles=model.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Tentacles").gameObject;specimen.tentacles.SetActive(false);specimen.goldenCrown=model.GetComponentsInChildren<Transform>(true).First(t=>t.name=="GoldenCrown").gameObject;specimen.goldenCrown.SetActive(false);
   specimen.tail=model.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Tail");
   game.specimenPrefabs[family]=PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/"+files[family]+".prefab").GetComponent<WorkshopSpecimen>();if(family==0)game.specimenPrefab=game.specimenPrefabs[0];else Object.DestroyImmediate(root);
  }
  static void BuildTools(){
   var root=Node("04 · 工具 / 选择与光束").transform;game.cursorTool=Node("光标工具",root);game.toolVisuals=new Transform[4];
   for(int i=0;i<4;i++){var tool=Tool((WorkshopTool)i,game.cursorTool.transform,Vector3.zero);tool.SetActive(i==0);game.toolVisuals[i]=tool.transform;
    var parked=Tool((WorkshopTool)i,root,new Vector3(-1.35f+i*.73f,1.22f,-1.53f));parked.transform.rotation=Quaternion.Euler(0,0,70);Hot(parked,(WorkshopAction)((int)WorkshopAction.Scalpel+i),"",new Vector3(.48f,.40f,.6f));if(i==3)parked.SetActive(false);
   }
   game.cursorTool.SetActive(false);var beam=Node("激光切割光束",root).AddComponent<LineRenderer>();beam.sharedMaterial=mats["Core"];beam.startWidth=.025f;beam.endWidth=.008f;beam.positionCount=2;beam.enabled=false;game.laserBeam=beam;
  }
  static GameObject Tool(WorkshopTool kind,Transform parent,Vector3 p){
   var go=Node(kind.ToString(),parent,p);var t=go.transform;
   if(kind==WorkshopTool.Scalpel){Rod("刀柄",t,new Vector3(0,.20f,0),new Vector3(.1f,.64f,0),.03f,"Metal");var blade=Shape("刀刃",t,new Vector3(-.025f,.06f,0),new Vector3(.025f,.19f,.05f),"Pearl");}
   if(kind==WorkshopTool.Hammer){Rod("锤柄",t,new Vector3(0,.10f,0),new Vector3(.05f,.66f,0),.04f,"Leather");Box("锤头",t,new Vector3(0,.07f,0),new Vector3(.36f,.17f,.20f),"Metal");}
   if(kind==WorkshopTool.Forceps){for(int side=-1;side<=1;side+=2)Rod("镊片",t,new Vector3(side*.025f,0,0),new Vector3(side*.08f,.65f,0),.016f,"Metal");}
   if(kind==WorkshopTool.Laser){Rod("激光笔",t,new Vector3(0,.14f,0),new Vector3(.07f,.64f,0),.065f,"Dark");Shape("激光头",t,new Vector3(0,.08f,0),new Vector3(.05f,.09f,.05f),"Core");}
   go.transform.localRotation=Quaternion.Euler(-25,0,-25);return go;
  }
  static void BuildUI(){
   var canvasGO=new GameObject("05 · 界面 / 仅必要信息",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=canvasGO.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
   var hud=canvasGO.AddComponent<WorkshopHud>();game.hud=hud;hud.game=game;Color text=C("E9DEC5"),muted=C("A8B9B8");
   hud.titleGroup=UIGroup("标题",canvasGO.transform);hud.playGroup=UIGroup("操作",canvasGO.transform);hud.tradeGroup=UIGroup("交易",canvasGO.transform);hud.tradeGroup.transform.SetSiblingIndex(1);
   string veilPath=Root+"/Settings/TradeVeil.asset";var veilTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(veilPath);if(!veilTexture){veilTexture=new Texture2D(128,1,TextureFormat.RGBA32,false);veilTexture.wrapMode=TextureWrapMode.Clamp;for(int x=0;x<128;x++)veilTexture.SetPixel(x,0,new Color(.025f,.045f,.06f,Mathf.Pow(1-x/127f,1.1f)*.94f));veilTexture.Apply();AssetDatabase.CreateAsset(veilTexture,veilPath);}
   var veil=UIGroup("交易文字后的渐隐",hud.tradeGroup.transform);var veilRect=veil.GetComponent<RectTransform>();veilRect.anchorMin=new Vector2(0,0);veilRect.anchorMax=new Vector2(0,1);veilRect.pivot=new Vector2(0,.5f);veilRect.sizeDelta=new Vector2(850,0);var veilImage=veil.AddComponent<RawImage>();veilImage.texture=veilTexture;veilImage.raycastTarget=false;
   Text("标题",hud.titleGroup.transform,"归 潮",new Vector2(100,-285),new Vector2(450,130),82,text);
   hud.startButton=Button("开始旅途",hud.titleGroup.transform,new Vector2(110,-470),new Vector2(280,60),28,game.Begin);hud.startLabel=hud.startButton.GetComponent<Text>();
   hud.money=Text("钱 · 首次交易后显示",hud.playGroup.transform,"",new Vector2(52,-37),new Vector2(380,40),24,text);
   hud.inventory=Text("收集到才出现",hud.playGroup.transform,"",new Vector2(53,-82),new Vector2(800,32),18,muted);
   hud.context=Text("鼠标悬停提示",hud.playGroup.transform,"",new Vector2(-460,150),new Vector2(920,42),22,text,new Vector2(.5f,0));hud.context.alignment=TextAnchor.MiddleCenter;
   hud.toolRow=Text("工具",hud.playGroup.transform,"",new Vector2(-530,65),new Vector2(1060,36),21,muted,new Vector2(.5f,0));hud.toolRow.alignment=TextAnchor.MiddleCenter;
   hud.tradeButton=Button("TAB  鱼贩子",hud.playGroup.transform,new Vector2(-270,-40),new Vector2(225,40),20,()=>{} ,new Vector2(1,1));hud.tradeToggle=hud.tradeButton.GetComponent<Text>();hud.tradeToggle.alignment=TextAnchor.MiddleRight;
   hud.nextButton=Button("下一条",hud.playGroup.transform,new Vector2(-200,100),new Vector2(160,40),22,()=>{},new Vector2(1,0));hud.nextLabel=hud.nextButton.GetComponent<Text>();
   hud.toast=Text("短暂反馈",canvasGO.transform,"",new Vector2(-350,-150),new Vector2(700,40),22,text,new Vector2(.5f,1));hud.toast.alignment=TextAnchor.MiddleCenter;
   hud.pause=Text("暂停",canvasGO.transform,"暂停",new Vector2(-150,35),new Vector2(300,60),36,text,new Vector2(.5f,.5f));hud.pause.alignment=TextAnchor.MiddleCenter;hud.pause.gameObject.SetActive(false);
   WorkshopAction[] actions={WorkshopAction.DeliverOrder,WorkshopAction.SellGoods,WorkshopAction.SellParts,WorkshopAction.BuyKnife,WorkshopAction.BuyScanner,WorkshopAction.BuyPeeler,WorkshopAction.BuyLaser,WorkshopAction.Return};hud.tradeActions=actions;hud.tradeLabels=new Text[actions.Length];hud.tradeButtons=new Button[actions.Length];
   for(int i=0;i<actions.Length;i++){var button=Button("交易 "+i,hud.tradeGroup.transform,new Vector2(65,-190-i*65),new Vector2(650,48),22,()=>{});hud.tradeButtons[i]=button;hud.tradeLabels[i]=button.GetComponent<Text>();}
   hud.condition=Text("当前完整度",hud.playGroup.transform,"",new Vector2(53,-119),new Vector2(300,30),18,muted);
   hud.intakeGroup=UIGroup("来货选择",hud.playGroup.transform);hud.intakeLabels=new Text[2];hud.intakeButtons=new Button[3];
   for(int i=0;i<3;i++){var button=Button(i==2?"取消":"",hud.intakeGroup.transform,new Vector2(-320,260-i*55),new Vector2(640,42),22,()=>{},new Vector2(.5f,0));hud.intakeButtons[i]=button;if(i<2)hud.intakeLabels[i]=button.GetComponent<Text>();}hud.intakeGroup.SetActive(false);
   hud.playGroup.SetActive(false);hud.tradeGroup.SetActive(false);
   var events=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
  }
  static GameObject UIGroup(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return go;}
  static Text Text(string name,Transform parent,string value,Vector2 p,Vector2 size,int fontSize,Color color,Vector2? anchor=null){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor??new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=p;r.sizeDelta=size;var t=go.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fontSize;t.color=color;t.text=value;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Overflow;var sh=go.AddComponent<Shadow>();sh.effectColor=new Color(.025f,.055f,.08f,.9f);sh.effectDistance=new Vector2(1,-2);return t;}
  static Button Button(string name,Transform parent,Vector2 p,Vector2 size,int fontSize,UnityEngine.Events.UnityAction callback,Vector2? anchor=null){var text=Text(name,parent,name,p,size,fontSize,C("E9DEC5"),anchor);text.raycastTarget=true;var b=text.gameObject.AddComponent<Button>();b.targetGraphic=text;var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=C("FFE0A0");colors.pressedColor=C("91D9BC");b.colors=colors;return b;}
  static GameObject Node(string name,Transform parent=null,Vector3 p=default){var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=p;return go;}
  static GameObject Box(string name,Transform parent,Vector3 p,Vector3 s,string material,bool collide=false){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=s;go.GetComponent<Renderer>().sharedMaterial=mats[material];if(!collide)Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
  static GameObject Shape(string name,Transform parent,Vector3 p,Vector3 s,string material){var go=Node(name,parent,p);go.transform.localScale=s;go.AddComponent<MeshFilter>().sharedMesh=stone;go.AddComponent<MeshRenderer>().sharedMaterial=mats[material];return go;}
  static GameObject Cylinder(string name,Transform parent,string material){var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.GetComponent<Renderer>().sharedMaterial=mats[material];Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
  static void Rod(string name,Transform parent,Vector3 a,Vector3 b,float r,string material){var go=Cylinder(name,parent,material);go.transform.localPosition=(a+b)/2;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);go.transform.localScale=new Vector3(r*2,(a-b).magnitude/2,r*2);}
  static void Jar(Transform p,Vector3 pos,float radius,float height,string material){Shape("标本瓶",p,pos,new Vector3(radius,height,radius),material);Shape("瓶塞",p,pos+Vector3.up*height,new Vector3(radius*.7f,.065f,radius*.7f),"Copper");}
  static void Point(string name,Transform root,Vector3 p,Color c,float intensity,float range){var l=Node(name,root,p).AddComponent<Light>();l.type=LightType.Point;l.color=c;l.intensity=intensity;l.range=range;}
  static void Spot(string name,Transform root,Vector3 p,Vector3 look,Color c,float intensity,float angle){var l=Node(name,root,p).AddComponent<Light>();l.type=LightType.Spot;l.color=c;l.intensity=intensity;l.range=10;l.spotAngle=angle;l.innerSpotAngle=angle*.7f;l.shadows=LightShadows.Soft;l.transform.LookAt(look);}
  static GameObject Model(string path,Transform parent,Vector3 p){var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing model "+path);var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent,false);go.transform.localPosition=p;PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);foreach(var renderer in go.GetComponentsInChildren<Renderer>(true)){var array=renderer.sharedMaterials;for(int i=0;i<array.Length;i++)if(array[i]&&mats.TryGetValue(array[i].name,out var material))array[i]=material;renderer.sharedMaterials=array;}return go;}
  static void Hot(GameObject go,WorkshopAction action,string label,Vector3 size){var h=go.AddComponent<WorkshopHotspot>();h.action=action;h.label=label;var c=go.AddComponent<BoxCollider>();c.size=size;}
 }
}







