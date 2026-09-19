using UnityEngine;
namespace ReturnTide.Workshop {
 public class WorkshopAnimator:MonoBehaviour {
  public Transform model,headPivot,leftShoulder,rightShoulder,leftUpper,leftForearm,leftHand,rightUpper,rightForearm,rightHand;
  public Transform[] eyes;
  public Vector3 target;
  public float breathing=.012f,reach=1.15f;
  Vector3 rootScale;Vector3[] eyeScales;Quaternion headRest;float actionUntil,nodUntil,startleUntil;WorkshopTool action;bool gripping;
  void Awake(){rootScale=model.localScale;headRest=headPivot.localRotation;eyeScales=new Vector3[eyes.Length];for(int i=0;i<eyes.Length;i++)eyeScales[i]=eyes[i].localScale;target=transform.position+transform.forward;}
  void Update(){
   model.localScale=new Vector3(rootScale.x,rootScale.y*(1+Mathf.Sin(Time.time*1.6f)*breathing),rootScale.z);
   float blink=Time.time%4.7f;for(int i=0;i<eyes.Length;i++)eyes[i].localScale=new Vector3(eyeScales[i].x,eyeScales[i].y*(blink<.13f?.1f:1),eyeScales[i].z);
   Vector3 local=transform.InverseTransformPoint(target);float nod=Time.time<nodUntil?Mathf.Sin((nodUntil-Time.time)*12)*8:0;
   headPivot.localRotation=Quaternion.Slerp(headPivot.localRotation,headRest*Quaternion.Euler(Mathf.Clamp(local.z*-4,-10,10)+nod,Mathf.Clamp(local.x*6,-18,18),0),Time.deltaTime*5);
   Vector3 leftTarget=transform.position+transform.forward*.78f-transform.right*.30f+Vector3.up*1.25f;
   Vector3 rightTarget=rightShoulder.position+Vector3.ClampMagnitude(target-rightShoulder.position,reach);
   rightTarget.y=Mathf.Max(transform.position.y+1.16f,rightTarget.y);
   if(Time.time<startleUntil){float flinch=Mathf.Sin(Mathf.Clamp01((startleUntil-Time.time)/.55f)*Mathf.PI);leftTarget-=transform.forward*flinch*.18f;rightTarget-=transform.forward*flinch*.22f;rightTarget+=Vector3.up*flinch*.1f;}
   float remaining=Mathf.Clamp01((actionUntil-Time.time)/.3f);float stroke=Mathf.Sin(remaining*Mathf.PI);
   if(action==WorkshopTool.Hammer)rightTarget+=Vector3.up*stroke*.5f;else rightTarget+=transform.forward*stroke*.12f;
   PoseArm(leftShoulder,leftUpper,leftForearm,leftHand,leftTarget,-1);PoseArm(rightShoulder,rightUpper,rightForearm,rightHand,rightTarget,1);
   rightHand.localScale=Vector3.one*(gripping?.12f:.14f);
  }
  void PoseArm(Transform shoulder,Transform upper,Transform lower,Transform hand,Vector3 goal,float side){
   Vector3 handPoint=Vector3.Lerp(hand.position,goal,1-Mathf.Exp(-Time.deltaTime*14));Vector3 elbow=Vector3.Lerp(shoulder.position,handPoint,.5f)+transform.right*side*.15f+Vector3.down*.16f;
   Segment(upper,shoulder.position,elbow,.12f);Segment(lower,elbow,handPoint,.105f);hand.position=handPoint;
  }
  void Segment(Transform t,Vector3 a,Vector3 b,float radius){t.position=(a+b)/2;t.rotation=Quaternion.FromToRotation(Vector3.up,b-a);t.localScale=new Vector3(radius*2,Vector3.Distance(a,b)/2,radius*2);}
  public void Action(WorkshopTool tool){action=tool;actionUntil=Time.time+.3f;}
  public void Grip(bool value){gripping=value;}
  public void Nod(){nodUntil=Time.time+.8f;}
  public void Startle(){startleUntil=Time.time+.55f;nodUntil=Time.time+.25f;}
 }
}
