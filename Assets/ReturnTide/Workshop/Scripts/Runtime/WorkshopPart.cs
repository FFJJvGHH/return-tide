using System.Collections;
using UnityEngine;
namespace ReturnTide.Workshop {
 public enum TissueLayer { Shell,Skin,Flesh,Bone,Organ,Core }
 public enum WorkshopTool { Scalpel,Hammer,Forceps,Laser }
 public class WorkshopPart:MonoBehaviour {
  public TissueLayer layer;
  public Transform[] cutPath;
  public LineRenderer cutGuide;
  public Transform cutCursor;
  public Collider hitbox;
  public WorkshopSpecimen specimen;
  [Header("可调交互")][Min(.05f)]public float dragRadius=.7f;
  public bool containsCrystal;
  public bool toxic;
  public int CutIndex{get;private set;}public int Hits{get;private set;}
  public bool Removed{get;private set;}public bool Broken=>Hits>=(specimen.profile?specimen.profile.boneHits:specimen.game.balance.boneHits);
  float laserProgress;Vector3 home;Quaternion rest;Renderer[] renderers;MaterialPropertyBlock properties;
  void Awake(){home=transform.position;rest=transform.rotation;renderers=GetComponentsInChildren<Renderer>();properties=new MaterialPropertyBlock();}
  void OnEnable(){if(!Removed){home=transform.position;rest=transform.rotation;}}
  public string Hint(WorkshopTool tool){
   if(layer==TissueLayer.Shell)return specimen.game.state.laser?"激光 · 按住切割":"外骨骼 · 需要激光";
   if(layer==TissueLayer.Skin||layer==TissueLayer.Flesh)return tool==WorkshopTool.Scalpel?"从亮点沿线划开":"需要解剖刀";
   if(layer==TissueLayer.Bone&&!Broken)return tool==WorkshopTool.Hammer?"敲碎骨片":"需要骨锤";
   if(tool!=WorkshopTool.Forceps)return "需要镊子";
   return toxic?"毒囊 → 左侧隔离盘":layer==TissueLayer.Core?"夹取晶核 → 右侧托盘":layer==TissueLayer.Organ?"移开器官 → 右侧托盘":"取出碎骨 → 右侧托盘";
  }
  public bool Trace(Vector3 point){
   if(Removed||cutPath==null||cutPath.Length==0)return false;
   float tolerance=specimen.game.balance.pathTolerance*(specimen.game.state.knife?1.5f:1);
   if(Vector3.Distance(point,cutPath[CutIndex].position)>tolerance)return false;
   CutIndex++;specimen.game.Pulse(WorkshopTool.Scalpel,point);
   if(CutIndex>=cutPath.Length){Complete();return true;}RefreshGuide(true);return false;
  }
  public void ResetTrace(){if(!Removed){CutIndex=0;RefreshGuide(specimen.game.Tool==WorkshopTool.Scalpel);}}
  public void Laser(float delta){if(Removed||!specimen.game.state.laser)return;laserProgress+=delta;specimen.game.Pulse(WorkshopTool.Laser,transform.position);Highlight(true);if(laserProgress>=specimen.game.balance.laserSeconds)Complete();}
  public bool Strike(){if(Removed||layer!=TissueLayer.Bone||Broken)return false;Hits++;specimen.game.Pulse(WorkshopTool.Hammer,transform.position);StartCoroutine(Jolt());if(Broken)transform.rotation=rest*Quaternion.Euler(0,12,12);return true;}
  IEnumerator Jolt(){Vector3 p=transform.localPosition;for(float t=0;t<.13f;t+=Time.deltaTime){transform.localPosition=p+Vector3.up*Mathf.Sin(t/.13f*Mathf.PI)*.1f;yield return null;}transform.localPosition=p;}
  public bool CanDrag(WorkshopTool tool)=>!Removed&&tool==WorkshopTool.Forceps&&(layer==TissueLayer.Core||layer==TissueLayer.Organ||layer==TissueLayer.Bone&&Broken);
  public void MoveDragged(Vector3 world){transform.position=world;transform.rotation=rest*Quaternion.Euler(0,Mathf.Sin(Time.time*4)*8,12);}
  public void Restore(){transform.position=home;transform.rotation=rest;}
  public void Complete(){if(Removed)return;Removed=true;hitbox.enabled=false;if(cutGuide)cutGuide.enabled=false;if(cutCursor)cutCursor.gameObject.SetActive(false);specimen.PartRemoved(this);specimen.game.Record(this);StartCoroutine(Depart());}
  public void RestoreRemoved(){Removed=true;hitbox.enabled=false;specimen.PartRemoved(this);gameObject.SetActive(false);}
  IEnumerator Depart(){Vector3 p=transform.position;Vector3 target=(toxic?specimen.game.isolationTray:specimen.game.outputTray).position+Vector3.up*.1f;Vector3 scale=transform.localScale;for(float t=0;t<.35f;t+=Time.deltaTime){float f=t/.35f;transform.position=Vector3.Lerp(p,target,f)+Vector3.up*Mathf.Sin(f*Mathf.PI)*.6f;transform.localScale=scale*(1-f*.75f);yield return null;}gameObject.SetActive(false);}
  public void Highlight(bool enabled){if(renderers==null||layer==TissueLayer.Core)return;foreach(var r in renderers)if(r&&r!=cutGuide){if(!enabled){r.SetPropertyBlock(null);continue;}properties.Clear();properties.SetColor("_BaseColor",Color.Lerp(r.sharedMaterial.GetColor("_BaseColor"),new Color(.65f,1,.85f),.28f));r.SetPropertyBlock(properties);}}
  public void RefreshGuide(bool show){if(!cutGuide||Removed)return;cutGuide.enabled=show;if(cutCursor){cutCursor.gameObject.SetActive(show);if(show)cutCursor.position=cutPath[CutIndex].position;}if(!show)return;int count=cutPath.Length-CutIndex;cutGuide.positionCount=count;for(int i=0;i<count;i++)cutGuide.SetPosition(i,cutPath[CutIndex+i].position);}
  void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;if(cutPath!=null)foreach(var p in cutPath)if(p)Gizmos.DrawWireSphere(p.position,.2f);}
 }
}


