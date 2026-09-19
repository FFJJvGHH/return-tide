using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace ReturnTide.Workshop.Editor {
 public static partial class WorkshopBuilder {
  static Material FeedbackMaterial(){string path=Root+"/Materials/Feedback.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));material.SetColor("_BaseColor",Color.white);material.SetFloat("_Cull",0);AssetDatabase.CreateAsset(material,path);}return material;}
  static LineRenderer FeedbackLine(string name,Transform parent,float width){var l=Node(name,parent).AddComponent<LineRenderer>();l.sharedMaterial=FeedbackMaterial();l.useWorldSpace=true;l.startWidth=l.endWidth=width;l.numCapVertices=3;l.startColor=l.endColor=new Color(.62f,1,.80f);l.positionCount=0;l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return l;}
  static void BuildFeedback(){
   var feedback=Node("06 · 反馈导演 / 时序与表现").AddComponent<WorkshopFeedback>();game.feedback=feedback;feedback.game=game;
   string path=Root+"/Settings/Feedback.asset";var settings=AssetDatabase.LoadAssetAtPath<FeedbackSettings>(path);if(!settings){settings=ScriptableObject.CreateInstance<FeedbackSettings>();AssetDatabase.CreateAsset(settings,path);}feedback.settings=settings;
   settings.slice=Clip("slice");settings.peel=Clip("peel");settings.pluck=Clip("pluck");settings.crack=Clip("crack");settings.ceramic=Clip("ceramic");settings.coin=Clip("coin");settings.core=Clip("core");settings.deny=Clip("deny");settings.unlock=Clip("unlock");settings.flutter=Clip("flutter");EditorUtility.SetDirty(settings);
   feedback.audioSource=feedback.gameObject.AddComponent<AudioSource>();feedback.audioSource.playOnAwake=false;
   var spark=Shape("反馈粒子 · 编辑此预制体",null,Vector3.zero,Vector3.one,"Core");spark.GetComponent<Renderer>().sharedMaterial=FeedbackMaterial();spark.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;feedback.sparkPrefab=PrefabUtility.SaveAsPrefabAsset(spark,Root+"/Prefabs/FX_Chip.prefab");Object.DestroyImmediate(spark);
   feedback.trophyMaterial=mats["Flesh"];feedback.trophyRoot=Node("盘中物料",feedback.transform).transform;
   feedback.rightRing=FeedbackLine("回收盘呼吸环",feedback.transform,.02f);feedback.leftRing=FeedbackLine("隔离盘呼吸环",feedback.transform,.02f);feedback.discoveryRing=FeedbackLine("晶核发现环",feedback.transform,.02f);
   feedback.toolSignals=new WorkshopToolSignal[4];foreach(var hot in Object.FindObjectsOfType<WorkshopHotspot>(true)){int index=(int)hot.action-(int)WorkshopAction.Scalpel;if(index<0||index>3)continue;var hitbox=hot.GetComponent<BoxCollider>();hitbox.center=new Vector3(0,.32f,0);hitbox.size=new Vector3(.28f,.72f,.38f);var children=hot.transform.Cast<Transform>().ToArray();var visual=Node("可动模型 · 判定保持固定",hot.transform).transform;foreach(var t in children)t.SetParent(visual,true);var signal=hot.gameObject.AddComponent<WorkshopToolSignal>();signal.tool=(WorkshopTool)index;signal.visual=visual;signal.halo=FeedbackLine("工具提示环",hot.transform,.015f);signal.halo.positionCount=33;for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;signal.halo.SetPosition(i,hot.transform.position+new Vector3(Mathf.Cos(a)*.26f,.04f,Mathf.Sin(a)*.23f));}feedback.toolSignals[index]=signal;hot.gameObject.SetActive(index==0);}
   var overlay=game.hud.gameObject.AddComponent<FeedbackOverlay>();overlay.game=game;overlay.canvas=game.hud.GetComponent<Canvas>();feedback.overlay=overlay;
   var ui=game.hud.playGroup.transform;var root=UIGroup("图标与获得反馈",ui).transform;
   overlay.toolSlots=new RectTransform[4];overlay.toolIcons=new WorkshopIcon[4];overlay.toolButtons=new Button[4];
   for(int i=0;i<4;i++){var slot=Rect("工具 "+i,root,new Vector2(.5f,0),new Vector2(0,55),new Vector2(54,54));overlay.toolSlots[i]=slot;var icon=Icon("工具图形",slot,(TideGlyph)i,Vector2.zero,42);overlay.toolIcons[i]=icon;icon.raycastTarget=true;var button=icon.gameObject.AddComponent<Button>();button.targetGraphic=icon;overlay.toolButtons[i]=button;var key=Text("键位",slot,(i+1).ToString(),new Vector2(-4,34),new Vector2(22,24),13,C("9AAFAE"),new Vector2(.5f,.5f));key.alignment=TextAnchor.MiddleCenter;}
   overlay.ledger=UIGroup("库存 · 交易或按住 I",root);overlay.inventorySlots=new GameObject[4];overlay.inventoryCounts=new Text[4];TideGlyph[] types={TideGlyph.Coin,TideGlyph.Meat,TideGlyph.Crystal,TideGlyph.Organ};
   for(int i=0;i<4;i++){var slot=Rect("库存 "+types[i],overlay.ledger.transform,new Vector2(0,1),new Vector2(54+i*126,-48),new Vector2(116,40));overlay.inventorySlots[i]=slot.gameObject;Icon("物品图形",slot,types[i],Vector2.zero,30);overlay.inventoryCounts[i]=Text("数量",slot,"",new Vector2(23,18),new Vector2(78,36),23,C("E9DEC5"),new Vector2(.5f,.5f));}
   overlay.rewards=new RewardToast[3];for(int i=0;i<3;i++){var slot=Rect("获得浮标 "+i,root,new Vector2(0,1),new Vector2(60,-62-i*55),new Vector2(126,40));var item=new RewardToast{root=slot,group=slot.gameObject.AddComponent<CanvasGroup>(),icon=Icon("获得图标",slot,TideGlyph.Meat,Vector2.zero,32),count=Text("增量",slot,"",new Vector2(25,18),new Vector2(85,40),26,C("E9DEC5"),new Vector2(.5f,.5f))};item.group.blocksRaycasts=false;item.group.alpha=0;overlay.rewards[i]=item;}
   overlay.flyingTemplate=Rect("飞入库存的图标 · 模板",root,new Vector2(.5f,.5f),Vector2.zero,new Vector2(28,28));var flying=overlay.flyingTemplate.gameObject.AddComponent<WorkshopIcon>();flying.raycastTarget=false;overlay.flyingTemplate.gameObject.SetActive(false);
   var purity=Rect("完整度 · 按需出现",root,new Vector2(1,1),new Vector2(-85,-115),new Vector2(100,50));overlay.purityGroup=purity.gameObject;overlay.purity=Icon("晶核裂纹",purity,TideGlyph.Purity,Vector2.zero,32);overlay.purityNumber=Text("完整度数值",purity,"",new Vector2(-70,13),new Vector2(58,30),18,C("E9DEC5"),new Vector2(.5f,.5f));
   overlay.contextGlyphRoot=Rect("场景位置引导",root,new Vector2(.5f,.5f),Vector2.zero,new Vector2(38,38));overlay.contextGlyph=overlay.contextGlyphRoot.gameObject.AddComponent<WorkshopIcon>();overlay.contextGlyph.raycastTarget=false;
   var tradeRect=game.hud.tradeButton.GetComponent<RectTransform>();tradeRect.sizeDelta=new Vector2(76,58);tradeRect.anchoredPosition=new Vector2(-103,-36);overlay.tradeIcon=Icon("交易铃图标",tradeRect,TideGlyph.Bell,Vector2.zero,32);overlay.tradeIcon.raycastTarget=true;game.hud.tradeButton.targetGraphic=overlay.tradeIcon;overlay.tradeKey=Text("交易键位",tradeRect,"TAB",new Vector2(-12,-20),new Vector2(40,22),11,C("A6B7AD"),new Vector2(.5f,.5f));
   var nextRect=game.hud.nextButton.GetComponent<RectTransform>();nextRect.sizeDelta=new Vector2(72,52);nextRect.anchoredPosition=new Vector2(-112,108);overlay.nextIcon=Icon("来货图标",nextRect,TideGlyph.Fish,Vector2.zero,36);overlay.nextIcon.raycastTarget=true;game.hud.nextButton.targetGraphic=overlay.nextIcon;
   Icon("下一批箭头",nextRect,TideGlyph.Arrow,new Vector2(30,0),17);
   foreach(var old in new[]{game.hud.money,game.hud.inventory,game.hud.condition,game.hud.toolRow}){old.text="";old.gameObject.SetActive(false);}
  }
  static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/"+name+".wav");
  static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size){var t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();t.SetParent(parent,false);t.anchorMin=t.anchorMax=anchor;t.pivot=new Vector2(.5f,.5f);t.sizeDelta=size;t.anchoredPosition=pos;return t;}
  static WorkshopIcon Icon(string name,Transform parent,TideGlyph glyph,Vector2 pos,float size){var t=Rect(name,parent,new Vector2(.5f,.5f),pos,new Vector2(size,size));var icon=t.gameObject.AddComponent<WorkshopIcon>();icon.glyph=glyph;icon.color=C("E9DEC5");icon.raycastTarget=false;return icon;}
 }
}


