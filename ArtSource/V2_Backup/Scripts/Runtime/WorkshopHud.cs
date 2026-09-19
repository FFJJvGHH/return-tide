using UnityEngine;
using UnityEngine.UI;
namespace ReturnTide.Workshop {
 public class WorkshopHud:MonoBehaviour {
  public WorkshopGame game;public GameObject titleGroup,playGroup,tradeGroup;
  public Text startLabel,money,inventory,context,toast,pause,toolRow,tradeToggle,nextLabel;
  public Button startButton,nextButton,tradeButton;
  public Text[] tradeLabels;public WorkshopAction[] tradeActions;
  public Button[] tradeButtons;
  Font font;float toastUntil;
  void Awake(){font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},32);foreach(var t in GetComponentsInChildren<Text>(true))t.font=font;}
  void Start(){startButton.onClick.AddListener(game.Begin);nextButton.onClick.AddListener(()=>game.NextSpecimen());tradeButton.onClick.AddListener(()=>game.SetTrade(!game.Trading));for(int i=0;i<tradeButtons.Length;i++){var action=tradeActions[i];tradeButtons[i].onClick.AddListener(()=>game.Act(action));}}
  void Update(){
   money.text=game.state.everSold?"银币  "+game.state.coins:"";
   string stock="";if(game.stockMeat>0)stock+="鱼肉 "+game.stockMeat+"   ";if(game.stockCores>0)stock+="晶核 "+game.stockCores+"   ";if(game.state.parts>0)stock+="异变组织 "+game.state.parts;inventory.text=stock;
   context.text=game.Paused?"":game.Hint;
   string[] labels={"1  解剖刀","2  骨锤","3  镊子","4  激光"};string row="";for(int i=0;i<(game.state.laser?4:3);i++){if(i>0)row+="      ";row+=(int)game.Tool==i?"<color=#E3BF7F>"+labels[i]+"</color>":labels[i];}if(game.state.scanner)row+="      Q  探针";toolRow.text=row;
   toolRow.gameObject.SetActive(game.Playing&&!game.Trading&&!game.returning);
   tradeToggle.text=game.Trading?"TAB  解剖台":"TAB  鱼贩子";
   nextButton.gameObject.SetActive(!game.Trading&&(game.specimenComplete||game.specimen.IsArmored&&!game.state.laser));nextLabel.text=game.specimen.IsArmored&&!game.state.laser?"退回整鱼  +12":"下一条";
   for(int i=0;i<tradeLabels.Length;i++)tradeLabels[i].text=game.ActionLabel(tradeActions[i]);
   if(Time.unscaledTime>toastUntil)toast.text="";
  }
  public void SetMode(bool playing,bool trading){titleGroup.SetActive(!playing);playGroup.SetActive(playing);tradeGroup.SetActive(playing&&trading);context.text="";pause.gameObject.SetActive(false);}
  public void Toast(string value){toast.text=value;toastUntil=Time.unscaledTime+1.8f;}
 }
}
