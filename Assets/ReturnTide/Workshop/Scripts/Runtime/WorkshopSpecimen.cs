using System.Linq;
using UnityEngine;
namespace ReturnTide.Workshop {
 public class WorkshopSpecimen:MonoBehaviour {
  public WorkshopGame game;
  public WorkshopPart[] parts;
  public SpecimenProfile profile;
  public GameObject tentacles,goldenCrown;
  public Transform tail;
  public int variant;
  public Transform breathingBody;public Transform[] finVisuals;
  public bool Surprised;
  public bool IsArmored=>variant==1;
  public bool Finished{get;private set;}
  public TissueLayer CurrentLayer{get;private set;}
  Vector3 tailScale,bodyScale;Quaternion tailRest;Quaternion[] finRest;float recoil;
  public void Configure(WorkshopGame owner,int specimenNumber){
   game=owner;variant=profile?profile.family:(specimenNumber%6==1?1:specimenNumber%6==3?2:specimenNumber%6==5?3:0);
   if(tail){tailScale=tail.localScale;tailRest=tail.localRotation;}if(breathingBody)bodyScale=breathingBody.localScale;if(finVisuals==null)finVisuals=new Transform[0];finRest=new Quaternion[finVisuals.Length];for(int i=0;i<finRest.Length;i++)finRest[i]=finVisuals[i].localRotation;
   tentacles.SetActive(variant==2);goldenCrown.SetActive(variant==3);
   var organs=parts.Where(p=>p.layer==TissueLayer.Organ).ToArray();int hidden=specimenNumber%organs.Length;
   for(int i=0;i<organs.Length;i++){organs[i].containsCrystal=i==hidden;organs[i].toxic=profile&&i==profile.toxicOrgan;}
   var crystal=parts.First(p=>p.layer==TissueLayer.Core);Vector3 cp=crystal.transform.position;cp.x=organs[hidden].transform.position.x;crystal.transform.position=cp;
   foreach(var p in parts){p.specimen=this;p.gameObject.SetActive(false);}
   CurrentLayer=IsArmored?TissueLayer.Shell:TissueLayer.Skin;RevealLayer(CurrentLayer);
  }
  void Update(){if(!game||!game.Playing||game.Paused)return;float calm=Finished ? .15f : 1;recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*2.8f);if(breathingBody)breathingBody.localScale=Vector3.Scale(bodyScale,new Vector3(1,1+Mathf.Sin(Time.time*2.1f+variant)*.013f*calm,1));if(tail){tail.localScale=tailScale;tail.localRotation=tailRest*Quaternion.Euler(0,(Mathf.Sin(Time.time*2.6f)*3*calm+Mathf.Sin(Time.time*21)*recoil*8),0);}for(int i=0;i<finVisuals.Length;i++)finVisuals[i].localRotation=finRest[i]*Quaternion.Euler(Mathf.Sin(Time.time*1.8f+i*.04f)*(2*calm+recoil*5),0,0);}
  public void Reflex(float amount){recoil=Mathf.Max(recoil,amount);}
  public void RevealLayer(TissueLayer layer){CurrentLayer=layer;foreach(var p in parts.Where(p=>!p.Removed&&(p.layer==layer||layer<TissueLayer.Organ&&p.layer==layer+1))){p.gameObject.SetActive(true);p.SetReady(p.layer==layer);}ShowGuides(game.Tool);if(game.feedback)game.feedback.Stage(layer);if(layer==TissueLayer.Skin&&game.state.peeler&&game.state.peelerEnabled)StartCoroutine(AutoPeel());}
  System.Collections.IEnumerator AutoPeel(){while(!game.Playing)yield return null;if(!game.state.machineApplied&&profile&&profile.machinePurityLoss>0&&parts.Any(p=>p.layer==TissueLayer.Skin&&!p.Removed)){game.state.machineApplied=true;game.DamagePurity(profile.machinePurityLoss,"机械剥皮损伤了组织");}foreach(var p in parts.Where(p=>p.layer==TissueLayer.Skin).ToArray()){yield return new WaitForSeconds(.45f);if(!game.state.peelerEnabled)yield break;p.Complete();}}
  public void PartRemoved(WorkshopPart part){
   switch(part.layer){case TissueLayer.Skin:game.stockSkin++;break;case TissueLayer.Flesh:game.stockMeat+=profile?profile.meatYield:1;break;case TissueLayer.Bone:game.stockBone++;break;case TissueLayer.Organ:game.stockMutation++;if(part.containsCrystal){var core=parts.First(p=>p.layer==TissueLayer.Core);core.gameObject.SetActive(true);core.SetReady(true);if(game.feedback)game.feedback.RevealCore(core.transform.position);else game.Chime(880,.22f);}break;case TissueLayer.Core:game.HarvestCore(profile?profile.corePrice:game.balance.coreValue);Finished=true;game.specimenComplete=true;if(!game.feedback)game.Chime(1080,.3f);break;}
   if(part.layer<=TissueLayer.Bone&&parts.Where(p=>p.layer==part.layer).All(p=>p.Removed))RevealLayer(part.layer+1);
  }
  public void ShowGuides(WorkshopTool tool){bool first=true;foreach(var p in parts)if(p.gameObject.activeSelf&&!p.Removed){bool eligible=p.Ready&&p.layer==CurrentLayer&&tool==WorkshopTool.Scalpel&&(p.layer==TissueLayer.Skin||p.layer==TissueLayer.Flesh);p.RefreshGuide(eligible&&first);if(eligible)first=false;}}
 }
}






