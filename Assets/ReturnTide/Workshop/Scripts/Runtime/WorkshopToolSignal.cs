using UnityEngine;
namespace ReturnTide.Workshop {
 public class WorkshopToolSignal:MonoBehaviour {
  public WorkshopTool tool;public Transform visual;public LineRenderer halo;
  Vector3 home;Quaternion rotation;Renderer[] pieces;MaterialPropertyBlock block;
  void Awake(){home=visual.localPosition;rotation=visual.localRotation;pieces=visual.GetComponentsInChildren<Renderer>();block=new MaterialPropertyBlock();}
  public void Animate(bool wanted,bool selected,bool reduced){
   float pulse=wanted?Mathf.Sin(Time.unscaledTime*5)*.5f+.5f:0;
   visual.localPosition=home+transform.InverseTransformVector(Vector3.up)*(wanted&&!reduced?pulse*.055f:0);
   visual.localRotation=rotation*Quaternion.Euler(0,0,wanted&&!reduced?Mathf.Sin(Time.unscaledTime*7)*7:0);
   halo.enabled=wanted||selected;halo.startColor=halo.endColor=wanted?new Color(.95f,.78f,.43f,.85f):new Color(.6f,.9f,.8f,.35f);
   foreach(var r in pieces){block.Clear();if(wanted)block.SetColor("_BaseColor",Color.Lerp(r.sharedMaterial.GetColor("_BaseColor"),new Color(1,.88f,.55f),pulse*.55f));r.SetPropertyBlock(block);}
  }
 }
}
