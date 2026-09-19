using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ReturnTide.Workshop {
 public class WorkshopFeedback:MonoBehaviour {
  public WorkshopGame game;public FeedbackSettings settings;public FeedbackOverlay overlay;public WorkshopToolSignal[] toolSignals;
  public LineRenderer rightRing,leftRing,discoveryRing;public GameObject sparkPrefab;
  public Transform trophyRoot;public AudioSource audioSource;
  public bool CueVisible{get;private set;}public TideGlyph CueGlyph{get;private set;}public Vector3 CuePosition{get;private set;}
  public int WantedTool{get;private set;}public int CutEvents,CrackEvents,LandingEvents,RevealEvents,SurpriseEvents;
  public bool Busy=>Time.time<busyUntil;public bool Restoring;
  float lastAction,flareUntil,rightPulse,leftPulse,busyUntil,lastTickSound,lastError;Vector3 flarePoint;WorkshopPart dragged;
  int token,receiptSlot;System.Random random=new System.Random(2719);
  class Bit {public GameObject obj;public Renderer renderer;public Vector3 velocity;public float born,life,size;public Color color;}
  readonly List<Bit> bits=new List<Bit>();readonly List<GameObject> trophies=new List<GameObject>();MaterialPropertyBlock property;
  void Awake(){property=new MaterialPropertyBlock();for(int i=0;i<48;i++){var go=Instantiate(sparkPrefab,transform);go.SetActive(false);bits.Add(new Bit{obj=go,renderer=go.GetComponent<Renderer>()});}lastAction=Time.time;}
  public void Activity(){lastAction=Time.time;}
  public void Learn(WorkshopTool tool){int flag=1<<(int)tool;if((game.state.learnedTools&flag)!=0)return;game.state.learnedTools|=flag;if(!Restoring&&game.Playing){Play(settings.unlock,.40f);Emit(toolSignals[(int)tool].transform.position,settings.gold,7,.5f,.025f);} }
  public void Stage(TissueLayer layer){if(layer==TissueLayer.Bone)Learn(WorkshopTool.Hammer);if(layer==TissueLayer.Organ)Learn(WorkshopTool.Forceps);Activity();}
  public void Begin(){Activity();game.state.learnedTools|=1;}
  void Update(){
   bool work=game.Playing&&!game.Trading&&!game.Paused&&!game.returning&&!game.hud.intakeGroup.activeSelf;
   if(Input.GetKeyDown(KeyCode.F8))settings.reducedMotion=!settings.reducedMotion;
   WantedTool=DesiredTool();float idle=Time.time-lastAction;bool pulseWindow=idle>settings.idleCueDelay&&(idle-settings.idleCueDelay)%settings.cueRepeatSeconds<1.6f;
   for(int i=0;i<toolSignals.Length;i++){bool available=game.IsToolAvailable((WorkshopTool)i);toolSignals[i].gameObject.SetActive(available);if(available)toolSignals[i].Animate(work&&!game.PointerBusy&&pulseWindow&&WantedTool==i,work&&game.Tool==(WorkshopTool)i,settings.reducedMotion);}
   CueVisible=work&&(!game.PointerBusy&&pulseWindow||dragged);CueGlyph=dragged?(dragged.toxic?TideGlyph.Warning:TideGlyph.Tray):WantedTool<0?TideGlyph.Lock:(TideGlyph)WantedTool;
   CuePosition=dragged?(dragged.toxic?game.isolationTray:game.outputTray).position:WantedTool<0?game.specimen.transform.position+Vector3.up:toolSignals[Mathf.Clamp(WantedTool,0,3)].transform.position+Vector3.up*.55f;
   Ring(rightRing,game.outputTray.position+Vector3.up*.17f,.72f,1.02f,work&&((dragged&&!dragged.toxic)||Time.time<rightPulse),settings.mint);
   Ring(leftRing,game.isolationTray.position+Vector3.up*.12f,.54f,.76f,work&&((dragged&&dragged.toxic)||Time.time<leftPulse),dragged&&dragged.toxic?settings.gold:settings.warning);
   Ring(discoveryRing,flarePoint,.42f+(flareUntil-Time.time)*.1f,.42f,work&&Time.time<flareUntil,settings.mint);
   foreach(var b in bits){if(!b.obj.activeSelf)continue;float age=Time.time-b.born;if(age>b.life){b.obj.SetActive(false);continue;}b.velocity+=Vector3.down*Time.deltaTime*.8f;b.obj.transform.position+=b.velocity*Time.deltaTime;b.obj.transform.Rotate(0,90*Time.deltaTime,55*Time.deltaTime);b.obj.transform.localScale=Vector3.one*b.size*Mathf.Sin(Mathf.PI*Mathf.Clamp01(age/b.life));}
  }
  int DesiredTool(){if(!game.specimen)return 0;var part=game.Hovered;if(part){if(part.layer==TissueLayer.Shell)return game.state.laser?3:-1;if(part.layer==TissueLayer.Skin||part.layer==TissueLayer.Flesh)return 0;if(part.layer==TissueLayer.Bone&&!part.Broken)return 1;return 2;}switch(game.specimen.CurrentLayer){case TissueLayer.Shell:return game.state.laser?3:-1;case TissueLayer.Skin:case TissueLayer.Flesh:return 0;case TissueLayer.Bone:return game.specimen.parts.Any(p=>p.gameObject.activeSelf&&p.Broken&&!p.Removed)?2:1;default:return 2;}}
  void Ring(LineRenderer line,Vector3 center,float x,float z,bool show,Color color){line.enabled=show;if(!show)return;line.positionCount=49;for(int i=0;i<=48;i++){float a=i*Mathf.PI/24;line.SetPosition(i,center+new Vector3(Mathf.Cos(a)*x,0,Mathf.Sin(a)*z));}line.startWidth=line.endWidth=.014f+(Mathf.Sin(Time.time*6)*.5f+.5f)*.01f;line.startColor=line.endColor=color;}
  public void Drag(WorkshopPart part){dragged=part;Activity();if(part)Play(settings.pluck,.22f);}
  public void Cut(WorkshopPart part,Vector3 position){Activity();CutEvents++;Emit(position,settings.mint,3,.25f,.012f);if(Time.time>lastTickSound){lastTickSound=Time.time+.07f;Play(settings.slice,.25f,.94f+part.CutIndex*.035f);}game.specimen.Reflex(.14f);}
  public void Hit(WorkshopPart part){Activity();bool broken=part.Broken;Play(settings.crack,broken?.6f:.27f,broken?.85f:1.15f);Emit(part.transform.position+Vector3.up*.14f,settings.gold,broken?11:4,broken?1.2f:.45f,broken?.055f:.025f);game.specimen.Reflex(broken?1:.35f);if(broken){CrackEvents++;Learn(WorkshopTool.Forceps);game.cameraRig.Impact(settings.impact);if(game.specimen.variant==1)Surprise(part.transform.position);}}
  public void Completed(WorkshopPart part){if(Restoring)return;Activity();float duration=part.layer==TissueLayer.Skin?settings.peelDuration:settings.pluckDuration;busyUntil=Mathf.Max(busyUntil,Time.time+duration+.12f);Play(part.layer==TissueLayer.Skin?settings.peel:settings.pluck,.45f);Emit(part.transform.position,part.layer==TissueLayer.Flesh?new Color(.9f,.65f,.58f):settings.mint,part.layer==TissueLayer.Core?15:5,.65f,.035f);StartCoroutine(Receipt(part.layer,part.toxic,part.specimen.profile?part.specimen.profile.meatYield:1,duration));}
  IEnumerator Receipt(TissueLayer layer,bool toxic,int yield,float delay){yield return new WaitForSeconds(delay);LandingEvents++;Transform tray=toxic?game.isolationTray:game.outputTray;Play(settings.ceramic,.35f,layer==TissueLayer.Core?1.3f:1);if(toxic)leftPulse=Time.time+.65f;else rightPulse=Time.time+.65f;Emit(tray.position+Vector3.up*.2f,toxic?settings.gold:settings.mint,6,.55f,.025f);
   TideGlyph glyph=layer==TissueLayer.Skin?TideGlyph.Skin:layer==TissueLayer.Flesh?TideGlyph.Meat:layer==TissueLayer.Bone?TideGlyph.Bone:layer==TissueLayer.Organ?TideGlyph.Organ:TideGlyph.Crystal;
   if(layer!=TissueLayer.Shell){overlay.Gain(glyph,layer==TissueLayer.Flesh?yield:1,tray.position+Vector3.up*.3f);DepositToken(layer,toxic);}if(layer==TissueLayer.Core){Play(settings.core,.45f);game.researcher.Nod();}
  }
  void DepositToken(TissueLayer layer,bool toxic){if(trophies.Count>=9){Destroy(trophies[0]);trophies.RemoveAt(0);}var go=Instantiate(sparkPrefab,trophyRoot);go.name="盘中收获 · "+layer;go.SetActive(true);Transform tray=toxic?game.isolationTray:game.outputTray;int i=receiptSlot++;go.transform.position=tray.position+new Vector3(((i%3)-1)*.19f,.10f,((i/3)%3-1)*.26f);go.transform.localScale=layer==TissueLayer.Flesh?new Vector3(.26f,.065f,.18f):layer==TissueLayer.Core?new Vector3(.10f,.22f,.10f):new Vector3(.16f,.06f,.10f);Tint(go.GetComponent<Renderer>(),layer==TissueLayer.Flesh?new Color(.78f,.42f,.43f):layer==TissueLayer.Core?settings.mint:toxic?new Color(.42f,.65f,.3f):settings.gold);trophies.Add(go);}
  public void Sold(int amount,Vector3 from){Activity();Play(settings.coin,.5f);overlay.Gain(TideGlyph.Coin,amount,from);Emit(from,settings.gold,16,1.1f,.035f);foreach(var go in trophies)if(go)Destroy(go);trophies.Clear();}
  public void RevealCore(Vector3 point){if(Restoring)return;RevealEvents++;flarePoint=point;flareUntil=Time.time+1.2f;Emit(point,settings.mint,16,.65f,.03f);Play(settings.core,.6f);game.cameraRig.Focus(settings.coreFocus);game.specimen.Reflex(.75f);if(game.specimen.variant==2||game.state.specimenIndex%3==2)Surprise(point);}
  public void Negative(Vector3 point){if(Time.time<lastError)return;lastError=Time.time+.3f;Play(settings.deny,.3f);Emit(point,settings.warning,5,.40f,.025f);overlay.ShowPurity();leftPulse=Time.time+.4f;Activity();}
  public void Surprise(Vector3 point){if(game.specimen.Surprised)return;game.specimen.Surprised=true;SurpriseEvents++;Emit(point+Vector3.up*.3f,settings.mint,9,1.4f,.045f);game.specimen.Reflex(1.8f);game.researcher.Startle();Play(settings.flutter,.45f);}
  public void Emit(Vector3 p,Color color,int number,float speed,float size){for(int i=0;i<number;i++){var b=bits.FirstOrDefault(item=>!item.obj.activeSelf);if(b==null)return;b.born=Time.time;b.life=.45f+(float)random.NextDouble()*.4f;b.size=size;b.velocity=new Vector3((float)random.NextDouble()-.5f,.4f+(float)random.NextDouble(),(float)random.NextDouble()-.5f)*speed;b.obj.transform.position=p;b.obj.SetActive(true);Tint(b.renderer,color);}}
  void Tint(Renderer renderer,Color tint){property.Clear();property.SetColor("_BaseColor",tint);property.SetColor("_Color",tint);renderer.SetPropertyBlock(property);}
  public void Play(AudioClip clip,float volume=1,float pitch=1){if(!clip||Restoring)return;audioSource.pitch=pitch;audioSource.PlayOneShot(clip,volume);}
 }
}
