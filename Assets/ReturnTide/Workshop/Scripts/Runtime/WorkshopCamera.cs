using UnityEngine;
namespace ReturnTide.Workshop {
 public enum WorkshopView { Title, Bench, Trade, Return }
 public class WorkshopCamera:MonoBehaviour {
  public Transform titlePose,benchPose,tradePose,returnPose;
  public float transitionSeconds=.85f;public float titleFov=43,benchFov=40,tradeFov=44;
  public WorkshopView view;Vector3 fromPosition;Quaternion fromRotation;float fromFov,elapsed;Camera cam;
  public WorkshopGame game;float impulse,focus,focusTime;
  void Awake(){cam=GetComponent<Camera>();SetView(WorkshopView.Title,true);}
  public void SetView(WorkshopView next,bool instant=false){
   if(!cam)cam=GetComponent<Camera>();view=next;fromPosition=transform.position;fromRotation=transform.rotation;fromFov=cam.fieldOfView;elapsed=instant?transitionSeconds:0;Apply();
  }
  void LateUpdate(){if(elapsed<transitionSeconds){elapsed+=Time.unscaledDeltaTime;Apply();return;}if(!game||view!=WorkshopView.Bench)return;
   transform.SetPositionAndRotation(benchPose.position,benchPose.rotation);cam.fieldOfView=benchFov;
   bool allowed=game.feedback&&!game.feedback.settings.reducedMotion&&!game.PointerBusy&&!game.Paused;
   if(allowed){transform.position+=transform.up*Mathf.Sin(Time.time*75)*impulse;float age=Time.time-focusTime;if(age>=0&&age<.85f)cam.fieldOfView-=Mathf.Sin(age/.85f*Mathf.PI)*focus;}
   impulse=Mathf.MoveTowards(impulse,0,Time.deltaTime*.35f);
  }
  public void Impact(float value){impulse=Mathf.Max(impulse,value);}
  public void Focus(float value){focus=value;focusTime=Time.time+.12f;}
  void Apply(){Transform pose=view==WorkshopView.Title?titlePose:view==WorkshopView.Bench?benchPose:view==WorkshopView.Trade?tradePose:returnPose;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/transitionSeconds));transform.position=Vector3.Lerp(fromPosition,pose.position,t);transform.rotation=Quaternion.Slerp(fromRotation,pose.rotation,t);cam.fieldOfView=Mathf.Lerp(fromFov,view==WorkshopView.Bench?benchFov:view==WorkshopView.Trade?tradeFov:titleFov,t);}
  public bool Settled=>elapsed>=transitionSeconds;
 }
}
