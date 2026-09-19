using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ReturnTide.Workshop {
 [System.Serializable]public class RewardToast {
  public RectTransform root;public CanvasGroup group;public WorkshopIcon icon;public Text count;
  [System.NonSerialized]public float born=-100;[System.NonSerialized]public int quantity;
 }
 public class FeedbackOverlay:MonoBehaviour {
  public WorkshopGame game;public Canvas canvas;
  public RectTransform[] toolSlots;public WorkshopIcon[] toolIcons;public Button[] toolButtons;
  public GameObject ledger;public GameObject[] inventorySlots;public Text[] inventoryCounts;
  public RewardToast[] rewards;public RectTransform flyingTemplate;
  public GameObject purityGroup;public WorkshopIcon purity;public Text purityNumber;
  public RectTransform contextGlyphRoot;public WorkshopIcon contextGlyph;
  public WorkshopIcon tradeIcon,nextIcon;public Text tradeKey;
  float purityUntil;int flights;public int GainEvents{get;private set;}
  void Start(){for(int i=0;i<4;i++){int n=i;toolButtons[i].onClick.AddListener(()=>game.SelectTool((WorkshopTool)n));}foreach(var t in rewards)t.group.alpha=0;flyingTemplate.gameObject.SetActive(false);}
  void Update(){
   bool play=game.Playing&&!game.returning;bool working=play&&!game.Trading&&!game.Paused&&!game.hud.intakeGroup.activeSelf;
   int count=Enumerable.Range(0,4).Count(i=>game.IsToolAvailable((WorkshopTool)i));int k=0;
   for(int i=0;i<4;i++){bool visible=working&&game.IsToolAvailable((WorkshopTool)i);toolSlots[i].gameObject.SetActive(visible);if(!visible)continue;toolSlots[i].anchoredPosition=new Vector2((k++-(count-1)*.5f)*74,55);toolIcons[i].color=game.Tool==(WorkshopTool)i?game.feedback.settings.gold:new Color(.70f,.79f,.76f,.65f);float target=game.Tool==(WorkshopTool)i?1.10f:1;toolSlots[i].localScale=Vector3.Lerp(toolSlots[i].localScale,Vector3.one*target,Time.unscaledDeltaTime*12);}
   bool inventory=play&&(game.Trading||Input.GetKey(KeyCode.I));ledger.SetActive(inventory);
   int[] totals={game.state.coins,game.stockMeat,game.stockCores,game.state.parts};int slot=0;for(int i=0;i<4;i++){inventorySlots[i].SetActive(totals[i]>0||i==0&&game.state.everSold);if(inventorySlots[i].activeSelf){inventorySlots[i].GetComponent<RectTransform>().anchoredPosition=new Vector2(54+slot++*126,-48);inventoryCounts[i].text=totals[i].ToString();}}
   purityGroup.SetActive(play&&(inventory||Time.unscaledTime<purityUntil));purity.fill=game.state.purity/100f;purity.color=game.state.purity>=85?game.feedback.settings.mint:game.feedback.settings.warning;purity.SetVerticesDirty();purityNumber.text=inventory?game.state.purity+"%":"";
   tradeIcon.Set(game.Trading?TideGlyph.Scalpel:TideGlyph.Bell);tradeKey.text="TAB";
   nextIcon.Set(game.specimen.IsArmored&&!game.state.laser?TideGlyph.Arrow:TideGlyph.Fish);
   foreach(var t in rewards){float age=Time.unscaledTime-t.born,duration=game.feedback.settings.popupSeconds;t.root.gameObject.SetActive(play&&age<duration);if(age>=duration)continue;t.group.alpha=Mathf.Min(1,age/.12f)*Mathf.Clamp01((duration-age)/.45f);float bounce=1+Mathf.Sin(Mathf.Clamp01(age/.4f)*Mathf.PI)*.18f;t.root.localScale=Vector3.one*bounce;}
   bool cue=working&&game.feedback.CueVisible;contextGlyphRoot.gameObject.SetActive(cue);if(cue){contextGlyph.Set(game.feedback.CueGlyph);contextGlyph.color=game.feedback.CueGlyph==TideGlyph.Warning?game.feedback.settings.warning:game.feedback.settings.gold;contextGlyphRoot.anchoredPosition=Project(game.feedback.CuePosition)+Vector2.up*25;float pulse=1+Mathf.Sin(Time.unscaledTime*4)*.06f;contextGlyphRoot.localScale=Vector3.one*pulse;}
  }
  public Vector2 Project(Vector3 world){Vector2 screen=RectTransformUtility.WorldToScreenPoint(game.cameraRig.GetComponent<Camera>(),world);RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform,screen,canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out var local);return local;}
  public void Gain(TideGlyph glyph,int amount,Vector3 source){if(amount==0)return;GainEvents++;var slot=rewards.FirstOrDefault(s=>s.icon.glyph==glyph&&Time.unscaledTime-s.born<game.feedback.settings.popupSeconds);if(slot==null){slot=rewards.OrderBy(s=>s.born).First();slot.quantity=0;}slot.quantity+=amount;slot.born=Time.unscaledTime;slot.icon.Set(glyph);slot.icon.color=ColorFor(glyph);slot.count.text=(slot.quantity>0?"+":"")+slot.quantity;slot.group.alpha=0;slot.root.gameObject.SetActive(true);if(flights<5)StartCoroutine(Fly(glyph,source,slot));}
  IEnumerator Fly(TideGlyph glyph,Vector3 source,RewardToast target){flights++;var item=Instantiate(flyingTemplate,flyingTemplate.parent);item.gameObject.SetActive(true);var icon=item.GetComponent<WorkshopIcon>();icon.Set(glyph);icon.color=ColorFor(glyph);Vector2 start=Project(source);Vector2 end=((RectTransform)target.root.parent).InverseTransformPoint(target.icon.transform.position);for(float t=0;t<.5f;t+=Time.unscaledDeltaTime){float f=t/.5f;item.anchoredPosition=Vector2.Lerp(start,end,f*f)+Vector2.up*Mathf.Sin(f*Mathf.PI)*55;item.localScale=Vector3.one*Mathf.Lerp(1,.7f,f);yield return null;}Destroy(item.gameObject);flights--;}
  public void ShowPurity(){purityUntil=Time.unscaledTime+2.5f;}
  Color ColorFor(TideGlyph glyph){return glyph==TideGlyph.Coin?game.feedback.settings.gold:glyph==TideGlyph.Meat?new Color(.95f,.64f,.58f):glyph==TideGlyph.Skin?new Color(.73f,.79f,.66f):glyph==TideGlyph.Bone?new Color(.92f,.86f,.69f):glyph==TideGlyph.Organ?new Color(.73f,.66f,.91f):game.feedback.settings.mint;}
 }
}
