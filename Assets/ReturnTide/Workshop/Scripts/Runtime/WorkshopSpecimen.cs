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
  public bool IsArmored=>variant==1;
  public bool Finished{get;private set;}
  public TissueLayer CurrentLayer{get;private set;}
  Vector3 tailScale;
  public void Configure(WorkshopGame owner,int specimenNumber){
   game=owner;variant=profile?profile.family:(specimenNumber%6==1?1:specimenNumber%6==3?2:specimenNumber%6==5?3:0);
   if(tail)tailScale=tail.localScale;
   tentacles.SetActive(variant==2);goldenCrown.SetActive(variant==3);
   var organs=parts.Where(p=>p.layer==TissueLayer.Organ).ToArray();int hidden=specimenNumber%organs.Length;
   for(int i=0;i<organs.Length;i++){organs[i].containsCrystal=i==hidden;organs[i].toxic=profile&&i==profile.toxicOrgan;}
   var crystal=parts.First(p=>p.layer==TissueLayer.Core);Vector3 cp=crystal.transform.position;cp.x=organs[hidden].transform.position.x;crystal.transform.position=cp;
   foreach(var p in parts){p.specimen=this;p.gameObject.SetActive(false);}
   CurrentLayer=IsArmored?TissueLayer.Shell:TissueLayer.Skin;RevealLayer(CurrentLayer);
  }
  void Update(){if(tail&&game&&game.Playing){float twitch=Mathf.Pow(Mathf.Max(0,Mathf.Sin(Time.time*.8f+variant)),24);tail.localScale=tailScale+new Vector3(0,0,twitch*.025f);}}
  public void RevealLayer(TissueLayer layer){CurrentLayer=layer;foreach(var p in parts.Where(p=>p.layer==layer&&!p.Removed)){p.gameObject.SetActive(true);p.RefreshGuide(game.Tool==WorkshopTool.Scalpel);}if(layer==TissueLayer.Skin&&game.state.peeler&&game.state.peelerEnabled)StartCoroutine(AutoPeel());}
  System.Collections.IEnumerator AutoPeel(){while(!game.Playing)yield return null;if(!game.state.machineApplied&&profile&&profile.machinePurityLoss>0&&parts.Any(p=>p.layer==TissueLayer.Skin&&!p.Removed)){game.state.machineApplied=true;game.DamagePurity(profile.machinePurityLoss,"机械剥皮损伤了组织");}foreach(var p in parts.Where(p=>p.layer==TissueLayer.Skin).ToArray()){yield return new WaitForSeconds(.45f);if(!game.state.peelerEnabled)yield break;p.Complete();}}
  public void PartRemoved(WorkshopPart part){
   switch(part.layer){case TissueLayer.Skin:game.stockSkin++;break;case TissueLayer.Flesh:game.stockMeat+=profile?profile.meatYield:1;break;case TissueLayer.Bone:game.stockBone++;break;case TissueLayer.Organ:game.stockMutation++;if(part.containsCrystal){var core=parts.First(p=>p.layer==TissueLayer.Core);core.gameObject.SetActive(true);game.Chime(880,.22f);}break;case TissueLayer.Core:game.HarvestCore(profile?profile.corePrice:game.balance.coreValue);Finished=true;game.specimenComplete=true;game.Chime(1080,.3f);break;}
   if(part.layer<=TissueLayer.Bone&&parts.Where(p=>p.layer==part.layer).All(p=>p.Removed))RevealLayer(part.layer+1);
  }
  public void ShowGuides(WorkshopTool tool){foreach(var p in parts)if(p.gameObject.activeSelf)p.RefreshGuide(tool==WorkshopTool.Scalpel&&(p.layer==TissueLayer.Skin||p.layer==TissueLayer.Flesh));}
 }
}



