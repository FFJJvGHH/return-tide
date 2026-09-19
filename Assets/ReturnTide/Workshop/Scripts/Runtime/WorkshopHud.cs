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
  public void ToggleIntake(){if(game.feedback&&game.feedback.Busy)return;if(!game.specimenComplete&&!(game.specimen.IsArmored&&!game.state.laser)){Toast("先取出晶核");return;}intakeGroup.SetActive(!intakeGroup.activeSelf);if(intakeGroup.activeSelf){intakeLabels[0].text=game.specimenPrefabs[0].profile.displayName+"  ·  稳定鱼肉";var p=game.specimenPrefabs[RiskyFamily].profile;intakeLabels[1].text=p.displayName+"  ·  "+p.handlingClue;}}
  void Receive(int family){if(game.NextSpecimen(family))intakeGroup.SetActive(false);}
  void Update(){
   money.text=inventory.text=condition.text=toolRow.text="";
   context.text=(game.Trading||Input.GetKey(KeyCode.LeftAlt))&&!game.Paused?game.Hint:"";
   toolRow.gameObject.SetActive(false);
   tradeToggle.text="";nextLabel.text="";
   tradeButton.gameObject.SetActive(game.Trading||game.state.everSold||game.stockMeat>0||game.stockCores>0||game.state.parts>0);
   nextButton.gameObject.SetActive(!game.Trading&&!intakeGroup.activeSelf&&(!game.feedback||!game.feedback.Busy)&&(game.specimenComplete||game.specimen.IsArmored&&!game.state.laser));   for(int i=0;i<tradeLabels.Length;i++)tradeLabels[i].text=game.ActionLabel(tradeActions[i]);
   if(Time.unscaledTime>toastUntil)toast.text="";
  }
  public void SetMode(bool playing,bool trading){titleGroup.SetActive(!playing);playGroup.SetActive(playing);tradeGroup.SetActive(playing&&trading);context.text="";pause.gameObject.SetActive(false);}
  public void Toast(string value){if(game.Playing&&!game.Trading&&!intakeGroup.activeSelf&&game.feedback){toast.text="";game.feedback.Negative(game.specimen.transform.position+Vector3.up);return;}toast.text=value;toastUntil=Time.unscaledTime+1.8f;}
 }
}

