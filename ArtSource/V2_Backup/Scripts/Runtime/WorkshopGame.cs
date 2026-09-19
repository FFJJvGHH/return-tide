using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ReturnTide.Workshop {
 public class WorkshopGame:MonoBehaviour {
  public const string SaveKey="ReturnTide.Workshop.v2";
  public WorkshopBalance balance;public WorkshopCamera cameraRig;public WorkshopHud hud;public WorkshopAnimator researcher;
  public WorkshopSpecimen specimenPrefab,specimen;public Transform specimenAnchor,outputTray,exitDoor,laserDisplay,peelerDisplay;
  public GameObject cursorTool;public Transform[] toolVisuals;public LineRenderer laserBeam;
  public AudioSource audioSource;public bool saveEnabled=true;
  public WorkshopSave state=new WorkshopSave();
  public WorkshopTool Tool{get;private set;}
  public bool Playing{get;private set;}public bool Trading{get;private set;}public bool Paused{get;private set;}
  public bool specimenComplete,returning;
  public int stockSkin,stockMeat,stockBone,stockCores,stockCoreValue;
  public int stockMutation{get=>state.parts;set=>state.parts=value;}
  public WorkshopPart Hovered{get;private set;}public WorkshopHotspot Hotspot{get;private set;}
  public int GoodsValue=>stockSkin*balance.skinValue+stockMeat*balance.meatValue+stockBone*balance.boneValue+stockCoreValue;
  public string Hint{get;private set;}="";
  WorkshopPart cutting,dragging;Camera cam;Vector3 cursorPoint;float nextSound;bool restoring,finishedCampaign;
  void Awake(){Time.timeScale=1;cam=cameraRig.GetComponent<Camera>();if(saveEnabled&&PlayerPrefs.HasKey(SaveKey)){try{state=JsonUtility.FromJson<WorkshopSave>(PlayerPrefs.GetString(SaveKey))??new WorkshopSave();}catch{state=new WorkshopSave();}}}
  void Start(){InitializeSpecimen();hud.SetMode(false,false);RefreshEquipment();SelectTool(WorkshopTool.Scalpel);}
  public void Begin(){if(finishedCampaign){SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);return;}if(Playing)return;Playing=true;hud.SetMode(true,false);cameraRig.SetView(WorkshopView.Bench);}
  void Update(){
   if(!Playing){if(Input.GetKeyDown(KeyCode.Return))Begin();return;}
   if(returning)return;
   if(Input.GetKeyDown(KeyCode.Escape)){if(Trading){SetTrade(false);return;}Paused=!Paused;Time.timeScale=Paused?0:1;hud.pause.gameObject.SetActive(Paused);CancelPointer();}
   if(Paused)return;
   if(Input.GetKeyDown(KeyCode.Tab)){SetTrade(!Trading);return;}
   if(Input.GetKeyDown(KeyCode.Alpha1))SelectTool(WorkshopTool.Scalpel);
   if(Input.GetKeyDown(KeyCode.Alpha2))SelectTool(WorkshopTool.Hammer);
   if(Input.GetKeyDown(KeyCode.Alpha3))SelectTool(WorkshopTool.Forceps);
   if(Input.GetKeyDown(KeyCode.Alpha4)&&state.laser)SelectTool(WorkshopTool.Laser);
   if(Input.GetKeyDown(KeyCode.N))NextSpecimen();
   if(!cameraRig.Settled)return;
   if(UnityEngine.EventSystems.EventSystem.current&&UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()&&!dragging){Hint="";return;}
   ProcessPointer(Input.mousePosition,Input.GetMouseButtonDown(0),Input.GetMouseButton(0),Input.GetMouseButtonUp(0));
  }
  public void ProcessPointer(Vector2 position,bool down,bool held,bool up){
   Ray ray=cam.ScreenPointToRay(position);
   foreach(var tissue in specimen.parts)if(tissue.gameObject.activeInHierarchy)tissue.Highlight(false);Hovered=null;Hotspot=null;Hint="";
   if(Physics.Raycast(ray,out var hit,50)){
    Hovered=hit.collider.GetComponentInParent<WorkshopPart>();Hotspot=hit.collider.GetComponentInParent<WorkshopHotspot>();
    if(Hovered){if(Hovered.Removed||!Hovered.gameObject.activeInHierarchy)Hovered=null;else{Hovered.Highlight(true);Hint=Hovered.Hint(Tool);}}
    if(Hotspot){if(Hotspot.tradeOnly&&!Trading)Hotspot=null;else Hint=ActionLabel(Hotspot.action);}
   }
   if(Trading){cursorTool.SetActive(false);if(Hotspot&&down)Act(Hotspot.action);return;}
   var plane=new Plane(Vector3.up,new Vector3(0,specimenAnchor.position.y+.82f,0));if(plane.Raycast(ray,out float distance))cursorPoint=ray.GetPoint(distance);
   bool overTable=Mathf.Abs(cursorPoint.x)<3.15f&&Mathf.Abs(cursorPoint.z)<1.45f;cursorTool.SetActive(overTable&&!Hotspot);
   cursorTool.transform.position=cursorPoint+Vector3.up*.08f;researcher.target=cursorPoint;
   laserBeam.enabled=false;
   if(down){
    if(Hotspot){Act(Hotspot.action);return;}
    if(Hovered){
     if(Hovered.CanDrag(Tool)){dragging=Hovered;dragging.hitbox.enabled=false;researcher.Grip(true);}
     else if(Tool==WorkshopTool.Hammer&&Hovered.layer==TissueLayer.Bone)Hovered.Strike();
     else if(Tool==WorkshopTool.Scalpel&&(Hovered.layer==TissueLayer.Skin||Hovered.layer==TissueLayer.Flesh))cutting=Hovered;
    }
   }
   if(cutting&&held){
    var cutPlane=new Plane(Vector3.up,cutting.cutPath[0].position);if(cutPlane.Raycast(ray,out float d)){Vector3 point=ray.GetPoint(d);cursorTool.transform.position=point+Vector3.up*.04f;if(cutting.Trace(point))cutting=null;}
   }
   if(Tool==WorkshopTool.Laser&&Hovered&&Hovered.layer==TissueLayer.Shell&&held){
    Hovered.Laser(Time.deltaTime);laserBeam.enabled=true;laserBeam.SetPosition(0,cursorTool.transform.position+Vector3.up*.35f);laserBeam.SetPosition(1,hit.point);
   }
   if(dragging){Vector3 p=cursorPoint+Vector3.up*balance.dragHeight;dragging.MoveDragged(p);cursorTool.transform.position=p+Vector3.up*.12f;Hint="放入右侧托盘";}
   if(up){
    if(cutting){cutting.ResetTrace();cutting=null;}
    if(dragging){Vector2 a=new Vector2(cursorPoint.x,cursorPoint.z),b=new Vector2(outputTray.position.x,outputTray.position.z);if(Vector2.Distance(a,b)<1.1f)dragging.Complete();else{dragging.Restore();dragging.hitbox.enabled=true;}dragging=null;researcher.Grip(false);}
   }
   if(state.scanner&&Input.GetKey(KeyCode.Q)){foreach(var p in specimen.parts)if(p.gameObject.activeInHierarchy&&p.layer==TissueLayer.Organ&&p.containsCrystal){p.Highlight(true);Hint="晶核反应";}}
  }
  void InitializeSpecimen(){
   specimenComplete=false;specimen.Configure(this,state.specimenIndex);
   int storedParts=state.parts;restoring=true;
   foreach(var name in state.removed.ToArray()){var p=specimen.parts.FirstOrDefault(x=>x.name==name);if(p)p.RestoreRemoved();}
   restoring=false;state.parts=storedParts;stockSkin=state.skin;stockMeat=state.meat;stockBone=state.bone;stockCores=state.cores;stockCoreValue=state.coreValue;
  }
  public void Record(WorkshopPart p){if(!restoring&&!state.removed.Contains(p.name)){state.removed.Add(p.name);Save();}}
  public void Save(){state.skin=stockSkin;state.meat=stockMeat;state.bone=stockBone;state.cores=stockCores;state.coreValue=stockCoreValue;if(saveEnabled){PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(state));PlayerPrefs.Save();}}
  public void SetTrade(bool value){CancelPointer();Trading=value;hud.SetMode(true,value);cameraRig.SetView(value?WorkshopView.Trade:WorkshopView.Bench);laserBeam.enabled=false;}
  public void SelectTool(WorkshopTool tool){if(tool==WorkshopTool.Laser&&!state.laser)return;CancelPointer();Tool=tool;for(int i=0;i<toolVisuals.Length;i++)toolVisuals[i].gameObject.SetActive(i==(int)tool);if(specimen)specimen.ShowGuides(tool);}
  void CancelPointer(){if(cutting)cutting.ResetTrace();cutting=null;if(dragging){dragging.Restore();dragging.hitbox.enabled=true;}dragging=null;if(researcher)researcher.Grip(false);}
  public bool NextSpecimen(){if(returning)return false;CancelPointer();if(specimen.IsArmored&&!state.laser&&!specimenComplete){state.coins+=12;state.everSold=true;}else if(!specimenComplete){hud.Toast("先取出晶核");return false;}state.specimenIndex++;state.removed.Clear();specimen.gameObject.SetActive(false);Destroy(specimen.gameObject);specimen=Instantiate(specimenPrefab,specimenAnchor.position,Quaternion.identity,specimenAnchor.parent);InitializeNewSpecimen();Save();return true;}
  void InitializeNewSpecimen(){specimenComplete=false;specimen.Configure(this,state.specimenIndex);specimen.ShowGuides(Tool);}
  public bool SellGoods(){if(GoodsValue<=0)return false;state.coins+=GoodsValue;state.deliveries++;state.everSold=true;stockSkin=stockMeat=stockBone=stockCores=stockCoreValue=0;researcher.Nod();Chime(680,.2f);Save();return true;}
  public bool SellPart(){if(state.parts<=0)return false;state.parts--;state.coins+=balance.mutationValue;state.everSold=true;Save();Chime(550,.1f);return true;}
  public bool Buy(WorkshopAction action){
   int price=0,parts=0;bool owned=false;
   switch(action){case WorkshopAction.BuyLaser:price=balance.laserPrice;parts=balance.laserParts;owned=state.laser;break;case WorkshopAction.BuyPeeler:price=balance.peelerPrice;parts=balance.peelerParts;owned=state.peeler;break;case WorkshopAction.BuyScanner:price=balance.scannerPrice;owned=state.scanner;break;case WorkshopAction.BuyKnife:price=balance.knifePrice;owned=state.knife;break;default:return false;}
   if(owned)return false;if(state.coins<price||state.parts<parts){hud.Toast(state.coins<price?"钱不够":"异变组织不足");return false;}
   state.coins-=price;state.parts-=parts;
   switch(action){case WorkshopAction.BuyLaser:state.laser=true;break;case WorkshopAction.BuyPeeler:state.peeler=true;break;case WorkshopAction.BuyScanner:state.scanner=true;break;case WorkshopAction.BuyKnife:state.knife=true;break;}
   RefreshEquipment();Save();Chime(920,.3f);return true;
  }
  void RefreshEquipment(){laserDisplay.gameObject.SetActive(state.laser);peelerDisplay.gameObject.SetActive(state.peeler);foreach(var spot in FindObjectsOfType<WorkshopHotspot>(true))if(spot.action==WorkshopAction.Laser)spot.gameObject.SetActive(state.laser);}
  public string ActionLabel(WorkshopAction a){switch(a){case WorkshopAction.Trade:return "鱼贩子 · 交易";case WorkshopAction.Next:return specimen.IsArmored&&!state.laser?"退回整鱼  +12":specimenComplete?"接收下一条":"先取出晶核";case WorkshopAction.SellGoods:return GoodsValue>0?"出售鱼肉与晶核  "+GoodsValue:"暂无货物";case WorkshopAction.SellParts:return "卖出异变组织  "+balance.mutationValue;case WorkshopAction.BuyLaser:return state.laser?"激光已安装":"激光  "+balance.laserPrice+"  ·  组织 ×"+balance.laserParts;case WorkshopAction.BuyPeeler:return state.peeler?"剥皮机已安装":"剥皮机  "+balance.peelerPrice+"  ·  组织 ×"+balance.peelerParts;case WorkshopAction.BuyScanner:return state.scanner?"探针已安装":"探针  "+balance.scannerPrice;case WorkshopAction.BuyKnife:return state.knife?"解剖刀已升级":"升级解剖刀  "+balance.knifePrice;case WorkshopAction.Return:return "修复返航信标  "+balance.returnPrice+"  ·  组织 ×"+balance.returnParts;case WorkshopAction.Back:return "回到解剖台";default:return a==WorkshopAction.Scalpel?"解剖刀":a==WorkshopAction.Hammer?"骨锤":a==WorkshopAction.Forceps?"镊子":"激光";}}
  public void Act(WorkshopAction action){switch(action){case WorkshopAction.Trade:SetTrade(true);break;case WorkshopAction.Back:SetTrade(false);break;case WorkshopAction.Next:NextSpecimen();break;case WorkshopAction.SellGoods:SellGoods();break;case WorkshopAction.SellParts:SellPart();break;case WorkshopAction.Return:ReturnHome();break;case WorkshopAction.Scalpel:SelectTool(WorkshopTool.Scalpel);break;case WorkshopAction.Hammer:SelectTool(WorkshopTool.Hammer);break;case WorkshopAction.Forceps:SelectTool(WorkshopTool.Forceps);break;case WorkshopAction.Laser:SelectTool(WorkshopTool.Laser);break;default:Buy(action);break;}}
  public bool ReturnHome(){if(returning)return false;if(state.coins<balance.returnPrice||state.parts<balance.returnParts){hud.Toast(state.coins<balance.returnPrice?"钱不够":"异变组织不足");return false;}state.coins-=balance.returnPrice;state.parts-=balance.returnParts;Save();StartCoroutine(ReturnSequence());return true;}
  IEnumerator ReturnSequence(){returning=true;CancelPointer();hud.SetMode(false,false);hud.titleGroup.SetActive(false);cursorTool.SetActive(false);cameraRig.SetView(WorkshopView.Return);researcher.Nod();for(float t=0;t<2.5f;t+=Time.deltaTime){exitDoor.localRotation=Quaternion.Euler(0,Mathf.SmoothStep(0,-100,t/2.5f),0);yield return null;}yield return new WaitForSeconds(1);hud.titleGroup.SetActive(true);hud.startLabel.text="再次启程";Playing=false;returning=false;finishedCampaign=true;if(saveEnabled)PlayerPrefs.DeleteKey(SaveKey);state=new WorkshopSave();hud.startButton.onClick.RemoveAllListeners();hud.startButton.onClick.AddListener(()=>SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));}
  public void Pulse(WorkshopTool tool,Vector3 point){researcher.Action(tool);researcher.target=point;if(Time.time>nextSound){nextSound=Time.time+.15f;Chime(tool==WorkshopTool.Hammer?95:tool==WorkshopTool.Laser?360:650,.06f);}}
  public void Chime(float hz,float duration){if(restoring||!audioSource)return;int count=(int)(22050*duration);float[] data=new float[count];for(int i=0;i<count;i++)data[i]=Mathf.Sin(i/22050f*hz*6.283f)*Mathf.Sin(Mathf.PI*i/count)*.08f;var clip=AudioClip.Create("tool",count,1,22050,false);clip.SetData(data,0);audioSource.PlayOneShot(clip);Destroy(clip,duration+.1f);}
  void OnDestroy(){Time.timeScale=1;}
 }
}



