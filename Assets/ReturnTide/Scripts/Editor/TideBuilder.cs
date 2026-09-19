using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace ReturnTide.Editor {
 public static class TideBuilder {
    const string Root="Assets/ReturnTide";
    static readonly Dictionary<string,Material> mats=new Dictionary<string,Material>();
    static readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
    static System.Random rng;
    static TideSettings balance;
    static GameObject pickup;
    static Transform world;
    [MenuItem("Tools/归潮 Return Tide/一键生成可编辑关卡",false,0)]
    public static void Generate(){ReturnTide.Workshop.Editor.WorkshopBuilder.Generate();}
    public static void Build(){
        rng=new System.Random(1937);mats.Clear();meshes.Clear();
        foreach(string folder in new[]{"Materials","Meshes","Prefabs","Scenes","Settings"})Directory.CreateDirectory(Root+"/"+folder);
        AssetDatabase.Refresh();CreatePalette();
        balance=AssetDatabase.LoadAssetAtPath<TideSettings>(Root+"/Settings/GameBalance.asset");
        if(!balance){balance=ScriptableObject.CreateInstance<TideSettings>();AssetDatabase.CreateAsset(balance,Root+"/Settings/GameBalance.asset");}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var environment=Node("01 · 鱼腹生态 / EDITABLE ENVIRONMENT");world=environment.transform;
        var lake=Node("02 · 湖岸 / OPENING & RETURN");
        var actors=Node("03 · 角色与系统 / ACTORS");
        var game=Node("Game Director · 流程与引用",actors.transform).AddComponent<TideGame>();game.settings=balance;game.interior=environment;game.lake=lake;
        game.audioSource=game.gameObject.AddComponent<AudioSource>();game.audioSource.spatialBlend=0;
        var ambience=Node("鱼腹声景 · 可替换音频",actors.transform).AddComponent<AudioSource>();ambience.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/Belly_Ambience.wav");ambience.loop=true;ambience.playOnAwake=true;ambience.volume=.5f;
        CreateLighting();CreatePickup();CreateInterior(game);CreateLake(lake.transform,game);
        var playerGO=Node("Mio · 科研员",actors.transform,new Vector3(0,.6f,-3));
        var controller=playerGO.AddComponent<CharacterController>();controller.height=2.2f;controller.radius=.36f;controller.center=new Vector3(0,1.1f,0);controller.stepOffset=.4f;
        var player=playerGO.AddComponent<TidePlayer>();player.settings=balance;player.visual=Model("Mio_Researcher",playerGO.transform,Vector3.zero).transform;
        var slash=Node("Scalpel · 切除弧光",playerGO.transform,new Vector3(0,1,0));
        var arc=MeshObj("弧刃",slash.transform,ArcMesh("Slash",1.8f,2.0f,-65,65),"Glow");arc.transform.localRotation=Quaternion.Euler(90,0,0);player.slash=slash.transform;slash.SetActive(false);
        var trailGO=Node("闪避尾迹",playerGO.transform,new Vector3(0,1,0));var trail=trailGO.AddComponent<TrailRenderer>();trail.sharedMaterial=mats["Glow"];trail.time=.25f;trail.startWidth=.65f;trail.endWidth=0;trail.emitting=false;player.dashTrail=trail;
        game.player=player;
        var camGO=Node("Main Camera · 斜俯视镜头");camGO.tag="MainCamera";var camera=camGO.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=9.5f;camera.nearClipPlane=.1f;camera.farClipPlane=170;camera.backgroundColor=Hex("183C48");camera.clearFlags=CameraClearFlags.SolidColor;
        var extra=camGO.AddComponent<UniversalAdditionalCameraData>();extra.renderPostProcessing=true;extra.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        camGO.AddComponent<AudioListener>();game.cameraRig=camGO.AddComponent<TideCamera>();game.cameraRig.target=playerGO.transform;game.cameraRig.Snap();
        game.hud=CreateUI();game.hud.ShowMenu();
        lake.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(playerGO,Root+"/Prefabs/Mio_Player.prefab");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,Root+"/Scenes/ReturnTide.unity");
        var scenes=new List<EditorBuildSettingsScene>{new EditorBuildSettingsScene(Root+"/Scenes/ReturnTide.unity",true)};
        foreach(var s in EditorBuildSettings.scenes)if(s.path!=Root+"/Scenes/ReturnTide.unity")scenes.Add(s);
        EditorBuildSettings.scenes=scenes.ToArray();
        PlayerSettings.productName="归潮 · Return Tide";PlayerSettings.companyName="Tideglass Studio";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        AssetDatabase.SaveAssets();Selection.activeGameObject=playerGO;
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0,0,10),Quaternion.Euler(48,0,0),28);
        Debug.Log("RETURN_TIDE_BUILD_OK: editable scene, models, prefabs and settings saved.");
    }
    static void CreatePalette(){
        string[] names={"Ivory","Ink","Teal","Coral","Gold","Skin","Hair","Glow","Dark","Leather","Ground","GroundLight","Flesh","Water","Rose","WhiteGlow"};
        string[] colors={"EAE4CF","203D49","3DAFA4","DF837D","EEBC69","F1C4AD","DDE4E1","92F1CA","283E54","936B58","617E7B","97A697","8C535E","204F5A","BC6573","FFF0C6"};
        for(int i=0;i<names.Length;i++){
            string path=Root+"/Materials/"+names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",Hex(colors[i]));m.SetFloat("_Smoothness",names[i]=="Water"?.72f:.12f);
            if(names[i]=="Glow"||names[i]=="WhiteGlow"){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",Hex(colors[i])*4.5f);}
            mats[names[i]]=m;
        }
        var waterShader=Shader.Find("Return Tide/Luminous Water");if(waterShader){mats["Water"].shader=waterShader;mats["Water"].SetColor("_BaseColor",Hex("163F4A"));mats["Water"].SetColor("_RippleColor",Hex("48847C"));}
    }
    static void CreateLighting(){
        var root=Node("00 · 光影 / LIGHT & ATMOSPHERE");
        var sun=Node("暖色主光 · Soft shadows",root.transform).AddComponent<Light>();sun.type=LightType.Directional;sun.color=Hex("FFE7C8");sun.intensity=1.35f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.7f;sun.shadowBias=.03f;sun.transform.rotation=Quaternion.Euler(48,-32,0);
        var fill=Node("冷色轮廓光",root.transform).AddComponent<Light>();fill.type=LightType.Directional;fill.color=Hex("87DFD6");fill.intensity=.4f;fill.transform.rotation=Quaternion.Euler(20,140,0);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Hex("668A99");RenderSettings.ambientEquatorColor=Hex("466E73");RenderSettings.ambientGroundColor=Hex("3C354E");RenderSettings.fog=true;RenderSettings.fogColor=Hex("204853");RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.014f;RenderSettings.sun=sun;
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/URP-HighFidelity.asset");if(pipeline){QualitySettings.renderPipeline=pipeline;pipeline.shadowDistance=85;}
        QualitySettings.shadows=UnityEngine.ShadowQuality.All;QualitySettings.shadowResolution=UnityEngine.ShadowResolution.High;QualitySettings.antiAliasing=4;
        string path=Root+"/Settings/TideAtmosphere.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.28f);bloom.threshold.Override(1.1f);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);var color=profile.Add<ColorAdjustments>(true);color.postExposure.Override(.25f);color.saturation.Override(9);var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.23f);vignette.smoothness.Override(.7f);foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);}
        var volume=Node("Bloom · 色彩 · 暗角",root.transform).AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
    }
    static void CreatePickup(){
        var go=Node("净化样本 · Pickup");var body=Shape("玻璃样本",go.transform,new Vector3(0,0,0),new Vector3(.18f,.42f,.18f),"Glow");
        Shape("铜盖",go.transform,new Vector3(0,.4f,0),new Vector3(.22f,.09f,.22f),"Gold");go.AddComponent<TidePickup>();
        pickup=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/PurifiedSample.prefab");Object.DestroyImmediate(go);
    }
    static void CreateInterior(TideGame game){
        var floor=Node("01 · 岛屿与通路",world).transform;
        Platform(floor,new Vector3(0,0,0),7.6f,"Ground");Platform(floor,new Vector3(0,0,17),10,"Ground");Platform(floor,new Vector3(0,0,37),10.5f,"Ground");
        Box("骨桥 A",floor,new Vector3(0,.065f,8),new Vector3(5.5f,.8f,6),"GroundLight",true);
        Box("骨桥 B",floor,new Vector3(0,.065f,27),new Vector3(4.5f,.8f,7),"GroundLight",true);
        for(int i=0;i<7;i++){Box("桥面骨节",floor,new Vector3(0,.44f,24.4f+i*.8f),new Vector3(4.6f,.10f,.28f),"Ivory");}
        var sea=Box("胃液 · 下方危险区",world,new Vector3(0,-1.7f,21),new Vector3(110,.25f,110),"Water");
        var hazard=Node("坠落判定 · 可编辑 Box Trigger",world,new Vector3(0,-2.8f,20));var hc=hazard.AddComponent<BoxCollider>();hc.size=new Vector3(110,1.5f,110);hc.isTrigger=true;var h=hazard.AddComponent<TideHazard>();h.returnToCheckpoint=true;
        var art=Node("02 · 骨拱 / 珊瑚 / 发光植物",world).transform;
        for(int i=0;i<8;i++){
            float z=-6+i*7.3f;
            for(int side=-1;side<=1;side+=2){
                var rib=Node("肋骨拱 "+i+" / "+side,art,new Vector3(side*10.5f,-.3f,z));
                for(int s=0;s<7;s++){
                    float a=s*.16f,b=(s+1)*.16f;Vector3 p=new Vector3(-side*(1-Mathf.Cos(a))*7,Mathf.Sin(a)*10,0),q=new Vector3(-side*(1-Mathf.Cos(b))*7,Mathf.Sin(b)*10,0);
                    Rod("骨节",rib.transform,p,q,.43f-s*.035f,"Ivory");
                }
            }
        }
        // Low-poly organic walls are separate editable mesh objects.
        for(int i=0;i<36;i++){
            float z=-8+i*1.7f;
            foreach(int side in new[]{-1,1}){
                float x=side*(13+Rand(0,3));Shape("胃壁褶皱",art,new Vector3(x,1.0f,z),new Vector3(Rand(2,3),Rand(1.8f,3.6f),Rand(2,4)),i%3==0?"Rose":"Flesh");
                if(i%2==0)Plant(art,new Vector3(side*Rand(6.8f,9.4f),.4f,z),Rand(.8f,1.6f));
            }
        }
        for(int i=0;i<60;i++){
            float z=Rand(-6,45),x=Rand(-9,9);if(Mathf.Abs(x)<3.2f)continue;
            Shape("漂流卵石",art,new Vector3(x,.4f,z),new Vector3(Rand(.15f,.6f),Rand(.12f,.38f),Rand(.2f,.8f)),i%3==0?"Coral":"GroundLight");
        }
        for(int i=0;i<38;i++){
            float z=Rand(-5,45),side=i%2==0?-1:1,x=side*Rand(5.2f,8.2f);
            var kelp=Node("海扇 · 叶片簇",art,new Vector3(x,.4f,z));
            for(int j=0;j<5;j++){
                var leaf=Shape("扇叶",kelp.transform,new Vector3((j-2)*.12f,.55f+Mathf.Sin(j)*.2f,0),new Vector3(.12f,Rand(.6f,1.2f),.045f),i%3==0?"Coral":"Teal");leaf.transform.localRotation=Quaternion.Euler(Rand(-25,25),i*39,(j-2)*-22);
            }
        }
        foreach(float center in new[]{0f,17f,37f}){
            for(int i=0;i<12;i++){
                float a=i*Mathf.PI*2/12;float radius=center==0?6.6f:8.8f;
                var fold=Shape("岛缘骨质褶纹",art,new Vector3(Mathf.Cos(a)*radius,.43f,center+Mathf.Sin(a)*radius),new Vector3(.12f,.045f,.8f),"GroundLight");fold.transform.localRotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0);
            }
            for(int i=0;i<8;i++)Shape("地表矿物细斑",art,new Vector3(Rand(-5,5),.41f,center+Rand(-4,4)),new Vector3(Rand(.25f,.7f),.015f,Rand(.15f,.45f)),"GroundLight");
        }
        for(int i=0;i<16;i++){
            float z=-4+i*3;var mark=Shape("引路荧光",art,new Vector3(Mathf.Sin(i*.7f)*.65f,.46f,z),new Vector3(.07f,.045f,.25f),"Glow");
        }
        for(int i=0;i<18;i++){
            var mote=Shape("悬浮孢子",art,new Vector3(Rand(-10,10),Rand(1,5),Rand(-6,48)),Vector3.one*Rand(.04f,.10f),"WhiteGlow");var f=mote.AddComponent<TideFloat>();f.height=.5f;f.speed=Rand(.4f,1.3f);
        }
        for(int i=0;i<3;i++)LightPoint(art,new Vector3(i==1?-5:4,3,3+i*17),Hex("63FFD4"),4.5f,12);
        game.entry=Node("出生点 · 鳃光浅滩",world,new Vector3(0,.65f,-3)).transform;
        var gameplay=Node("03 · 可编辑交互与判定",world).transform;
        MakeLesion(gameplay,new Vector3(-4.6f,.5f,13),false,"感染 01 · 鳃膜");
        MakeLesion(gameplay,new Vector3(5,.5f,19),false,"感染 02 · 胃腺");
        MakeLesion(gameplay,new Vector3(-3,.5f,32),false,"感染 03 · 心瓣");
        MakeLesion(gameplay,new Vector3(0,.5f,40),true,"陨星核心 · 需要稳态针");
        Enemy(gameplay,new Vector3(-2,.6f,12));Enemy(gameplay,new Vector3(5,.6f,16));Enemy(gameplay,new Vector3(-3,.6f,30));Enemy(gameplay,new Vector3(3,.6f,35));
        var stall=Node("鱼贩子阿鲤 · 交易与检查点",gameplay,new Vector3(-4.5f,.5f,3.5f));
        Model("Fishmonger",stall.transform,new Vector3(0,0,.7f));
        Box("摊面",stall.transform,new Vector3(0,.9f,-.6f),new Vector3(2.8f,.15f,1.1f),"Leather");
        for(int i=-1;i<=1;i+=2)Rod("木支柱",stall.transform,new Vector3(i*1.35f,0,-.1f),new Vector3(i*1.35f,3.1f,-.1f),.075f,"Leather");
        var awning=Box("青色篷布",stall.transform,new Vector3(0,2.9f,-.3f),new Vector3(3,.12f,1.8f),"Teal");awning.transform.localRotation=Quaternion.Euler(-9,0,0);
        for(int i=-1;i<=1;i++)Shape("摊上小鱼",stall.transform,new Vector3(i*.7f,1.1f,-.6f),new Vector3(.3f,.13f,.14f),"Ivory");
        var lantern=Shape("摊位灯",stall.transform,new Vector3(1.2f,2.3f,-.2f),new Vector3(.2f,.30f,.2f),"WhiteGlow");LightPoint(stall.transform,new Vector3(1,2,-.5f),Hex("FFC87A"),2,5);
        var merchant=stall.AddComponent<TideMerchant>();merchant.interactionRadius=3.4f;merchant.checkpoint=Node("复苏点",stall.transform,new Vector3(2,.1f,-1)).transform;
        PrefabUtility.SaveAsPrefabAsset(stall,Root+"/Prefabs/Fishmonger_Stall.prefab");
        var exit=Node("回归之门",gameplay,new Vector3(0,.5f,45));exit.AddComponent<TideExit>();
        var glow=Node("净化后开启",exit.transform);game.exitGlow=glow.transform;
        var ring=MeshObj("回归光环",glow.transform,ArcMesh("Portal",1.65f,1.80f,0,360),"Glow");ring.transform.localPosition=Vector3.up*2;
        LightPoint(glow.transform,new Vector3(0,2,0),Hex("A6FFE0"),5,8);glow.SetActive(false);
    }
    static void CreateLake(Transform root,TideGame game){
        root.position=new Vector3(140,0,0);
        Platform(root,new Vector3(-3,0,0),8,"GroundLight");Box("静水",root,new Vector3(4,-.45f,5),new Vector3(75,.2f,75),"Water");
        for(int i=0;i<9;i++){
            var stone=Shape("岸边岩",root,new Vector3(Rand(-9,-4),Rand(0,.5f),Rand(-7,8)),new Vector3(Rand(1,2),Rand(.5f,1.7f),Rand(1,2)),"Ground");
        }
        for(int i=0;i<7;i++){
            float x=Rand(-12,-6),z=Rand(-7,12);Rod("树干",root,new Vector3(x,0,z),new Vector3(x,4,z),.25f,"Leather");
            Shape("树冠",root,new Vector3(x,4.4f,z),new Vector3(2.3f,2.8f,2.3f),"Teal");
        }
        Box("钓鱼木栈桥",root,new Vector3(1,.4f,1),new Vector3(5,.3f,2.8f),"Leather",true);
        for(int i=0;i<10;i++)Box("木板",root,new Vector3(-1.2f+i*.48f,.60f,1),new Vector3(.42f,.12f,2.8f),"Gold");
        game.lakeSpawn=Node("湖岸出生点",root,new Vector3(.8f,.75f,.6f)).transform;
        game.fishingRod=Node("钓竿与鱼线",root,new Vector3(1,.5f,1));Rod("钓竿",game.fishingRod.transform,new Vector3(0,1,0),new Vector3(2.7f,3,1),.025f,"Leather");Rod("鱼线",game.fishingRod.transform,new Vector3(2.7f,3,1),new Vector3(3.5f,-.5f,2.1f),.006f,"Ivory");Shape("浮漂",game.fishingRod.transform,new Vector3(3.5f,-.45f,2.1f),new Vector3(.12f,.12f,.12f),"Coral");
        game.fishingFish=Model("Radiant_Fish",root,new Vector3(7,-2,5)).transform;
        game.meteor=Shape("坠落的陨星",root,new Vector3(13,13,12),new Vector3(.4f,.7f,.4f),"WhiteGlow").transform;
        Rod("陨星光尾",game.meteor,new Vector3(0,0,0),new Vector3(6,3,0),.12f,"Gold");
    }
    static void MakeLesion(Transform parent,Vector3 pos,bool core,string name){
        var go=Node(name,parent,pos);var lesion=go.AddComponent<TideLesion>();lesion.isCore=core;lesion.cutDuration=core?3.0f:balance.cutSeconds;lesion.interactionRadius=core?3.6f:3;lesion.samplePrefab=pickup;
        var infected=Node("感染态 · 可替换模型",go.transform);lesion.infectedVisual=infected;
        Shape("病灶",infected.transform,new Vector3(0,.65f,0),new Vector3(core?1.6f:1.0f,core?1.8f:1.0f,core?1.6f:1.0f),"Rose");
        for(int j=0;j<7;j++){float a=j*6.283f/7;Vector3 p=new Vector3(Mathf.Cos(a),.5f,Mathf.Sin(a));var spike=Shape("辐射晶簇",infected.transform,p,new Vector3(.22f,core?1.7f:.85f,.22f),"Glow");spike.transform.localRotation=Quaternion.Euler(Mathf.Sin(a)*30,0,-Mathf.Cos(a)*30);}
        Shape("发光核心",infected.transform,new Vector3(0,1.1f,0),Vector3.one*.47f,"Gold");
        var healthy=Node("净化态",go.transform);lesion.healthyVisual=healthy;Plant(healthy.transform,Vector3.zero,1.1f);healthy.SetActive(false);
        var hit=go.AddComponent<SphereCollider>();hit.radius=core?1.3f:.8f;hit.center=new Vector3(0,.7f,0);lesion.barrier=hit;
        var ring=MeshObj("切除边界",go.transform,ArcMesh(core?"CoreRing":"LesionRing",core?2:1.4f,core?2.04f:1.44f,0,360),"Gold");ring.transform.localRotation=Quaternion.Euler(90,0,0);ring.transform.localPosition=Vector3.up*.05f;
        PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+(core?"MeteorCore":"InfectedOrgan")+".prefab");
    }
    static void Enemy(Transform parent,Vector3 pos){
        var go=Node("游走病原 · Patrol",parent,pos);var enemy=go.AddComponent<TideEnemy>();var v=Node("病原模型",go.transform,Vector3.up*.5f);enemy.visual=v.transform;
        Shape("壳",v.transform,Vector3.zero,new Vector3(.65f,.48f,.65f),"Coral");Shape("核",v.transform,new Vector3(0,.27f,-.35f),new Vector3(.22f,.22f,.22f),"Glow");
        for(int i=0;i<5;i++){float a=i*6.283f/5;Rod("触须",v.transform,new Vector3(Mathf.Cos(a)*.2f,0,Mathf.Sin(a)*.2f),new Vector3(Mathf.Cos(a)*.9f,-.35f,Mathf.Sin(a)*.9f),.07f,"Rose");}
        PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/Pathogen.prefab");
    }
    static void Plant(Transform parent,Vector3 p,float scale){
        var go=Node("生物荧光花",parent,p);go.transform.localScale=Vector3.one*scale;
        for(int j=0;j<4;j++){
            float a=j*2.4f;Vector3 top=new Vector3(Mathf.Cos(a)*.35f,.8f+j*.30f,Mathf.Sin(a)*.35f);Rod("花茎",go.transform,Vector3.zero,top,.04f,"Teal");
            Shape("荧光花芯",go.transform,top,new Vector3(.13f,.23f,.13f),"Glow");
            for(int k=0;k<3;k++){float angle=k*2.094f+a;var petal=Shape("花瓣",go.transform,top+new Vector3(Mathf.Cos(angle)*.2f,-.08f,Mathf.Sin(angle)*.2f),new Vector3(.25f,.09f,.13f),j%2==0?"Ivory":"Gold");petal.transform.localRotation=Quaternion.Euler(0,-angle*Mathf.Rad2Deg,-18);}
        }
    }
    static void Platform(Transform parent,Vector3 p,float radius,string material){
        var go=MeshObj("浮岛 · 独立地形",parent,DiskMesh(),material);go.transform.localPosition=p;go.transform.localScale=new Vector3(radius,.8f,radius);go.AddComponent<MeshCollider>();
        var underside=Shape("岛屿下缘",parent,p+Vector3.down*.95f,new Vector3(radius*.96f,1.2f,radius*.95f),"Flesh");
    }
    static GameObject Model(string name,Transform parent,Vector3 p){
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+".fbx");if(!model)throw new Exception("Missing generated Blender model: "+name);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(model);go.transform.SetParent(parent,false);go.transform.localPosition=p;
        foreach(var renderer in go.GetComponentsInChildren<Renderer>()){
            var materials=renderer.sharedMaterials;for(int i=0;i<materials.Length;i++)if(materials[i]&&mats.TryGetValue(materials[i].name,out var m))materials[i]=m;renderer.sharedMaterials=materials;
        }
        return go;
    }
    static GameObject Node(string name,Transform parent=null,Vector3 p=default){var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=p;return go;}
    static GameObject Box(string name,Transform parent,Vector3 p,Vector3 scale,string material,bool collide=false){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mats[material];if(!collide)Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
    static GameObject Shape(string name,Transform parent,Vector3 p,Vector3 scale,string material){var go=MeshObj(name,parent,StoneMesh(),material);go.transform.localPosition=p;go.transform.localScale=scale;return go;}
    static GameObject MeshObj(string name,Transform parent,Mesh mesh,string material){var go=Node(name,parent);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mats[material];return go;}
    static void Rod(string name,Transform parent,Vector3 a,Vector3 b,float radius,string material){var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=(a+b)*.5f;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);go.transform.localScale=new Vector3(radius*2,(b-a).magnitude*.5f,radius*2);go.GetComponent<Renderer>().sharedMaterial=mats[material];Object.DestroyImmediate(go.GetComponent<Collider>());}
    static void LightPoint(Transform parent,Vector3 pos,Color color,float intensity,float range){var l=Node("生物光源",parent,pos).AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;}
    static float Rand(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
    static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
    static Mesh SaveMesh(string name,List<Vector3> verts,List<int> tris){
        // Split vertices for deliberate faceted normals.
        var v=new Vector3[tris.Count];var t=new int[tris.Count];for(int i=0;i<t.Length;i++){v[i]=verts[tris[i]];t[i]=i;}
        var mesh=new Mesh{name=name};mesh.vertices=v;mesh.triangles=t;mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path=Root+"/Meshes/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);meshes[name]=mesh;return mesh;
    }
    static Mesh StoneMesh(){
        if(meshes.TryGetValue("FacetedStone",out var m))return m;var v=new List<Vector3>();var t=new List<int>();int sides=10,rings=6;
        for(int y=0;y<=rings;y++){float a=y*Mathf.PI/rings;for(int x=0;x<=sides;x++){float b=x*2*Mathf.PI/sides;v.Add(new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b)));}}
        for(int y=0;y<rings;y++)for(int x=0;x<sides;x++){int k=y*(sides+1)+x;t.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1});}
        return SaveMesh("FacetedStone",v,t);
    }
    static Mesh DiskMesh(){
        if(meshes.TryGetValue("Island",out var m))return m;var v=new List<Vector3>{new Vector3(0,.5f,0),new Vector3(0,-.5f,0)};var t=new List<int>();int n=32;
        for(int i=0;i<n;i++){float a=i*2*Mathf.PI/n;float r=1+Mathf.Sin(i*1.77f)*.035f;v.Add(new Vector3(Mathf.Cos(a)*r,.5f,Mathf.Sin(a)*r));v.Add(new Vector3(Mathf.Cos(a)*r*.95f,-.5f,Mathf.Sin(a)*r*.95f));}
        for(int i=0;i<n;i++){int a=2+i*2,b=2+((i+1)%n)*2;t.AddRange(new[]{0,b,a,1,a+1,b+1,a,b,a+1,b,b+1,a+1});}return SaveMesh("Island",v,t);
    }
    static Mesh ArcMesh(string name,float inner,float outer,float start,float end){
        if(meshes.TryGetValue(name,out var m))return m;var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<=64;i++){float a=Mathf.Lerp(start,end,i/64f)*Mathf.Deg2Rad;v.Add(new Vector3(Mathf.Cos(a)*inner,Mathf.Sin(a)*inner,0));v.Add(new Vector3(Mathf.Cos(a)*outer,Mathf.Sin(a)*outer,0));if(i>0){int k=i*2;t.AddRange(new[]{k-2,k-1,k,k,k-1,k+1,k,k-1,k-2,k+1,k-1,k});}}return SaveMesh(name,v,t);
    }
    static TideHud CreateUI(){
        var go=Node("04 · 界面 / 每个文本均可编辑");var canvas=go.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=go.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var hud=go.AddComponent<TideHud>();Color ivory=Hex("F2EBD8"),mint=Hex("AEF4D2"),soft=Hex("B6D5CF");
        hud.menuGroup=UINode("标题页",go.transform);hud.playGroup=UINode("游戏 HUD",go.transform);hud.endingGroup=UINode("结局",go.transform);
        var menu=hud.menuGroup.transform;
        string gradientPath=Root+"/Settings/MenuVeil.asset";var gradient=AssetDatabase.LoadAssetAtPath<Texture2D>(gradientPath);
        if(!gradient){gradient=new Texture2D(128,1,TextureFormat.RGBA32,false);gradient.name="Soft title veil";gradient.wrapMode=TextureWrapMode.Clamp;for(int x=0;x<128;x++)gradient.SetPixel(x,0,new Color(.025f,.08f,.10f,Mathf.Pow(1-x/127f,1.6f)*.82f));gradient.Apply();AssetDatabase.CreateAsset(gradient,gradientPath);}
        var veil=UINode("柔和渐隐背景",menu);Place(veil.GetComponent<RectTransform>(),new Vector2(0,1),Vector2.zero,new Vector2(750,900));var veilImage=veil.AddComponent<RawImage>();veilImage.texture=gradient;veilImage.raycastTarget=false;
        hud.eyebrow=Label("序号",menu,"T I D E G L A S S     /     0 0 1",new Vector2(85,-170),new Vector2(600,40),18,mint);
        hud.title=Label("游戏标题",menu,"归 潮",new Vector2(78,-223),new Vector2(580,125),86,ivory);
        Label("英文副标题",menu,"R E T U R N   T O   T H E   T I D E",new Vector2(88,-354),new Vector2(650,38),19,ivory);
        Label("细分割线",menu,"────────────",new Vector2(88,-407),new Vector2(350,32),16,mint);
        hud.tagline=Label("短句",menu,"在巨鱼心跳之间，找一条回家的路。",new Vector2(88,-455),new Vector2(650,42),24,ivory);
        hud.startLabel=Label("开始提示",menu,"↵   开始旅程",new Vector2(88,-578),new Vector2(500,50),28,mint);
        Label("版本",menu,"一段关于切除与回归的小小冒险   /   第一章",new Vector2(88,-810),new Vector2(800,32),17,soft);
        var play=hud.playGroup.transform;
        hud.health=Label("生命",play,"●●●●●",new Vector2(54,-40),new Vector2(320,42),25,ivory);
        hud.objective=Label("当前目标",play,"",new Vector2(56,-93),new Vector2(450,38),22,ivory);
        hud.samples=Label("收集后才显示",play,"",new Vector2(57,-135),new Vector2(450,32),18,mint);
        hud.region=Label("区域名",play,"",new Vector2(-445,-47),new Vector2(390,34),18,soft,new Vector2(1,1));hud.region.alignment=TextAnchor.MiddleRight;
        hud.controls=Label("操作提示",play,"W A S D   移动     /     鼠标左键   挥刀     /     SPACE   闪避",new Vector2(55,37),new Vector2(1000,36),16,soft,new Vector2(0,0));
        hud.prompt=Label("附近交互",play,"",new Vector2(-350,142),new Vector2(700,46),24,ivory,new Vector2(.5f,0));hud.prompt.alignment=TextAnchor.MiddleCenter;
        var progress=UINode("切除进度",play);Place(progress.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(-100,125),new Vector2(200,3));var back=progress.AddComponent<Image>();back.color=new Color(.8f,.9f,.8f,.18f);
        hud.cutFill=Bar("进度",progress.transform,new Vector2(0,0),new Vector2(200,3),mint);hud.cutFill.type=Image.Type.Filled;hud.cutFill.fillMethod=Image.FillMethod.Horizontal;progress.SetActive(false);
        var dash=UINode("闪避恢复",play);Place(dash.GetComponent<RectTransform>(),new Vector2(0,0),new Vector2(58,83),new Vector2(80,3));hud.dashFill=Bar("恢复",dash.transform,Vector2.zero,new Vector2(80,3),mint);hud.dashFill.type=Image.Type.Filled;hud.dashFill.fillMethod=Image.FillMethod.Horizontal;
        hud.toast=Label("临时信息",go.transform,"",new Vector2(-590,205),new Vector2(1180,50),23,ivory,new Vector2(.5f,0));hud.toast.alignment=TextAnchor.MiddleCenter;
        hud.subtitle=Label("开场字幕",go.transform,"",new Vector2(-620,90),new Vector2(1240,55),26,ivory,new Vector2(.5f,0));hud.subtitle.alignment=TextAnchor.MiddleCenter;
        hud.pauseText=Label("暂停",go.transform,"",new Vector2(-400,65),new Vector2(800,150),30,ivory,new Vector2(.5f,.5f));hud.pauseText.alignment=TextAnchor.MiddleCenter;
        var end=hud.endingGroup.transform;
        Label("结局小标题",end,"E P I L O G U E   /   湖 岸",new Vector2(85,-210),new Vector2(700,44),20,mint);
        Label("结局标题",end,"回 归",new Vector2(78,-275),new Vector2(600,120),80,ivory);
        hud.endingText=Label("结局短句",end,"切除的是感染，留下的是生命。\n\n阿鲤说，明天请你喝鱼汤。",new Vector2(88,-430),new Vector2(900,150),27,ivory);
        Label("重玩",end,"↵   再次启程",new Vector2(88,-660),new Vector2(600,50),25,mint);
        var flash=UINode("受伤闪烁",go.transform);Stretch(flash.GetComponent<RectTransform>());hud.damageFlash=flash.AddComponent<Image>();hud.damageFlash.color=Color.clear;hud.damageFlash.raycastTarget=false;
        var iris=UINode("圆形收幕 · Iris",go.transform);Stretch(iris.GetComponent<RectTransform>());hud.iris=iris.AddComponent<TideIris>();hud.iris.color=Color.black;hud.iris.raycastTarget=false;
        hud.endingGroup.SetActive(false);return hud;
    }
    static GameObject UINode(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);Stretch(go.GetComponent<RectTransform>());return go;}
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    static void Place(RectTransform r,Vector2 anchor,Vector2 pos,Vector2 size){r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(0,1);r.sizeDelta=size;r.anchoredPosition=pos;}
    static Text Label(string name,Transform parent,string value,Vector2 pos,Vector2 size,int fontSize,Color color,Vector2? anchor=null){var go=UINode(name,parent);Place(go.GetComponent<RectTransform>(),anchor??new Vector2(0,1),pos,size);var t=go.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=fontSize;t.color=color;t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;var shadow=go.AddComponent<Shadow>();shadow.effectColor=new Color(.04f,.12f,.15f,.7f);shadow.effectDistance=new Vector2(1,-2);return t;}
    static Image Bar(string name,Transform parent,Vector2 pos,Vector2 size,Color color){var go=UINode(name,parent);Place(go.GetComponent<RectTransform>(),new Vector2(0,1),pos,size);var im=go.AddComponent<Image>();im.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");im.color=color;return im;}
    [MenuItem("Tools/归潮 Return Tide/重播首次开场",false,20)]static void ReplayIntro(){PlayerPrefs.DeleteKey("ReturnTide.IntroSeen");Debug.Log("Next journey will play the opening.");}
    [MenuItem("Tools/归潮 Return Tide/打开可编辑关卡",false,10)]static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ReturnTide.Workshop.Editor.WorkshopBuilder.ScenePath);}
 }
}



