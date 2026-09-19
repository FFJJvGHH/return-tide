using UnityEngine;
namespace ReturnTide.Workshop {
 [CreateAssetMenu(menuName="Return Tide/Feedback Settings")]
 public class FeedbackSettings:ScriptableObject {
  [Header("引导")][Min(.1f)]public float idleCueDelay=2.5f,cueRepeatSeconds=4.5f;
  [Header("动作时序")][Min(.1f)]public float peelDuration=.82f,pluckDuration=.62f,popupSeconds=2.0f;
  [Header("镜头与小动作")][Range(0,.15f)]public float impact=.035f;[Range(0,4)]public float coreFocus=1.7f;public bool reducedMotion;
  public Color mint=new Color(.55f,.95f,.78f),gold=new Color(.95f,.74f,.40f),warning=new Color(1,.48f,.30f);
  [Header("声画同步")]
  public AudioClip slice,peel,pluck,crack,ceramic,coin,core,deny,unlock,flutter;
 }
}
