using UnityEngine;
using UnityEngine.UI;
namespace ReturnTide.Workshop {
 public class WorkshopHud:MonoBehaviour {
  public WorkshopGame game;public GameObject titleGroup,playGroup,tradeGroup;
  public Text startLabel,money,inventory,context,toast,pause,toolRow,tradeToggle,nextLabel;
  public Button startButton,nextButton,tradeButton;
  public Text[] tradeLabels;public WorkshopAction[] tradeActions;
  public Button[] tradeButtons;
  public GameObject intakeGroup;public Text[] intakeLabels;public Button[] intakeButtons;public Text condition;
  Font font;float toastUntil;
  void Awake(){font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},32);foreach(var t in GetComponentsInChildren<Text>(true))t.font=font;}
  void Start(){startButton.onClick.AddListener(game.Begin);nextButton.onClick.AddListener(ToggleIntake);tradeButton.onClick.AddListener(()=>game.SetTrade(!game.Trading));for(int i=0;i<tradeButtons.Length;i++){var action=tradeActions[i];tradeButtons[i].onClick.AddListener(()=>game.Act(action));}intakeButtons[0].onClick.AddListener(()=>Receive(0));intakeButtons[1].onClick.AddListener(()=>Receive(RiskyFamily));intakeButtons[2].onClick.AddListener(()=>intakeGroup.SetActive(false));}
  int RiskyFamily=>game.state.orders==0?1:game.state.orders==1?2:3;
  public void ToggleIntake(){if(!game.specimenComplete&&!(game.specimen.IsArmored&&!game.state.laser)){Toast("先取出晶核");return;}intakeGroup.SetActive(!intakeGroup.activeSelf);if(intakeGroup.activeSelf){intakeLabels[0].text=game.specimenPrefabs[0].profile.displayName+"  ·  稳定鱼肉";var p=game.specimenPrefabs[RiskyFamily].profile;intakeLabels[1].text=p.displayName+"  ·  "+p.handlingClue;}}
  void Receive(int family){if(game.NextSpecimen(family))intakeGroup.SetActive(false);}
  void Update(){
   money.text=game.state.everSold?"银币  "+game.state.coins:"";
   string stock="";if(game.stockMeat>0)stock+="鱼肉 "+game.stockMeat+"   ";if(game.stockCores>0)stock+="晶核 "+game.stockCores+"   ";if(game.state.parts>0)stock+="异变组织 "+game.state.parts;inventory.text=stock;
   context.text=game.Paused?"":game.Hint;
   condition.text=game.state.removed.Count>0?"纯度  "+game.state.purity+"%":"";
   string[] labels={"1  解剖刀","2  骨锤","3  镊子","4  激光"};string row="";for(int i=0;i<(game.state.laser?4:3);i++){if(i>0)row+="      ";row+=(int)game.Tool==i?"<color=#E3BF7F>"+labels[i]+"</color>":labels[i];}if(game.state.scanner)row+="      Q  探针";toolRow.text=row;
   toolRow.gameObject.SetActive(game.Playing&&!game.Trading&&!game.returning);
   tradeToggle.text=game.Trading?"TAB  解剖台":"TAB  鱼贩子";
   nextButton.gameObject.SetActive(!game.Trading&&!intakeGroup.activeSelf&&(game.specimenComplete||game.specimen.IsArmored&&!game.state.laser));nextLabel.text=game.specimen.IsArmored&&!game.state.laser?"退回 / 换货":"选择来货";
   for(int i=0;i<tradeLabels.Length;i++)tradeLabels[i].text=game.ActionLabel(tradeActions[i]);
   if(Time.unscaledTime>toastUntil)toast.text="";
  }
  public void SetMode(bool playing,bool trading){titleGroup.SetActive(!playing);playGroup.SetActive(playing);tradeGroup.SetActive(playing&&trading);context.text="";pause.gameObject.SetActive(false);}
  public void Toast(string value){toast.text=value;toastUntil=Time.unscaledTime+1.8f;}
 }
}
