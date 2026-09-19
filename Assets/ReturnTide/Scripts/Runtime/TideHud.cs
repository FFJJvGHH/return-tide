using UnityEngine;
using UnityEngine.UI;
namespace ReturnTide {
    public class TideHud:MonoBehaviour {
        public Text health,objective,samples,prompt,toast,region,controls,subtitle,title,eyebrow,tagline,startLabel,pauseText,endingText;
        public GameObject menuGroup,playGroup,endingGroup;
        public Image cutFill,dashFill,damageFlash;
        public TideIris iris;
        float toastUntil,flashUntil;
        Font chineseFont;
        void Awake(){chineseFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},32);foreach(var t in GetComponentsInChildren<Text>(true))t.font=chineseFont;}
        void Update(){
            var g=TideGame.Instance;if(!g)return;
            if(g.Playing){
                health.text=new string('●',g.player.Health)+new string('○',Mathf.Max(0,g.settings.maxHealth-g.player.Health));
                objective.text=g.CoreCured?"回归湖岸":g.HasStabilizer?"切除陨星核心":g.CutCount>=3?"把样本带给鱼贩子":"切除感染  "+g.CutCount+" / 3";
                samples.text=g.EverCollected?(g.HasStabilizer?"◇  稳态针已装配":"◇  净化样本  "+g.Samples):"";
                prompt.text=g.Nearest?g.Nearest.Prompt:"";
                var lesion=g.Nearest as TideLesion;cutFill.transform.parent.gameObject.SetActive(lesion&&lesion.progress>0);
                if(lesion)cutFill.fillAmount=lesion.progress;
                dashFill.fillAmount=g.player.DashReady;
                float z=g.player.transform.position.z;
                region.text=z<9?"01   /   鳃光浅滩":z<29?"02   /   珊瑚胃庭":"03   /   陨星心室";
            }
            if(Time.unscaledTime>toastUntil)toast.text="";
            if(damageFlash)damageFlash.color=new Color(1,.3f,.25f,Mathf.Max(0,(flashUntil-Time.unscaledTime)*.65f));
        }
        public void Notify(string message,float seconds){toast.text=message;toastUntil=Time.unscaledTime+seconds;}
        public void Flash(){flashUntil=Time.unscaledTime+.45f;}
        public void ShowMenu(){menuGroup.SetActive(true);playGroup.SetActive(false);endingGroup.SetActive(false);subtitle.text="";}
        public void ShowPlay(){menuGroup.SetActive(false);playGroup.SetActive(true);endingGroup.SetActive(false);subtitle.text="";}
        public void ShowEnding(){menuGroup.SetActive(false);playGroup.SetActive(false);endingGroup.SetActive(true);subtitle.text="";toast.text="";}
    }
}

