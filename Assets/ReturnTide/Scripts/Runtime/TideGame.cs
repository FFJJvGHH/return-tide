using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ReturnTide {
    public class TideGame:MonoBehaviour {
        public static TideGame Instance {get;private set;}
        public TideSettings settings;
        public TidePlayer player;
        public TideCamera cameraRig;
        public TideHud hud;
        public Transform entry, lakeSpawn, fishingFish, meteor, exitGlow;
        public GameObject interior,lake,fishingRod;
        public AudioSource audioSource;
        public bool replayIntroEveryTime;
        public int Samples,CutCount;
        public bool HasStabilizer,CoreCured,EverCollected;
        public Vector3 checkpoint;
        public bool Playing {get;private set;}
        public TideInteractable Nearest {get;private set;}
        bool menu=true,paused,ending,sequence;
        AudioClip[] tones;
        void Awake(){Instance=this;Time.timeScale=1;checkpoint=entry.position;tones=new AudioClip[5];}
        void Start(){player.Teleport(entry.position);player.visual.rotation=Quaternion.Euler(0,180,0);cameraRig.Snap();lake.SetActive(false);interior.SetActive(true);hud.ShowMenu();}
        void Update(){
            if(menu&&Input.GetKeyDown(KeyCode.Return))Begin();
            if(ending&&Input.GetKeyDown(KeyCode.Return))Restart();
            if(sequence&&Input.GetKeyDown(KeyCode.Escape)){StopAllCoroutines();EnterInterior();return;}
            if(!menu&&!ending&&!sequence&&Input.GetKeyDown(KeyCode.Escape)){paused=!paused;Time.timeScale=paused?0:1;hud.pauseText.text=paused?"已暂停\nESC  继续     /     R  重新开始":"";}
            if(paused&&Input.GetKeyDown(KeyCode.R))Restart();
            if(!Playing||paused)return;
            Nearest=null;float closest=float.MaxValue;
            foreach(var obj in FindObjectsOfType<TideInteractable>()){
                if(!obj.Available)continue;float dist=Vector3.Distance(player.transform.position,obj.transform.position);
                if(dist<obj.interactionRadius&&dist<closest){closest=dist;Nearest=obj;}
            }
            if(Nearest)Nearest.Interact(this,Input.GetKey(KeyCode.E));
        }
        public void Begin(){if(!menu)return;menu=false;hud.menuGroup.SetActive(false);if(replayIntroEveryTime||PlayerPrefs.GetInt("ReturnTide.IntroSeen",0)==0)StartCoroutine(Intro());else EnterInterior();}
        IEnumerator Intro(){
            sequence=true;Playing=false;player.controllable=false;lake.SetActive(true);interior.SetActive(false);fishingRod.SetActive(true);
            player.Teleport(lakeSpawn.position);player.visual.rotation=Quaternion.Euler(0,60,0);cameraRig.Snap();
            hud.subtitle.text="陨星坠落后的第七天，生化感染已经蔓延至湖边。";
            fishingFish.gameObject.SetActive(false);meteor.gameObject.SetActive(true);
            Vector3 from=meteor.position;for(float t=0;t<2.8f;t+=Time.deltaTime){meteor.position=from+new Vector3(-t*4,-t*2,0);yield return null;}
            hud.subtitle.text="湖水很安静。直到——";yield return new WaitForSeconds(1);
            fishingFish.gameObject.SetActive(true);Vector3 a=lakeSpawn.position+new Vector3(5,-2,5),b=lakeSpawn.position+Vector3.up*.9f;
            Sound(110,.5f);
            for(float t=0;t<2.1f;t+=Time.deltaTime){float p=t/2.1f;fishingFish.position=Vector3.Lerp(a,b,p)+Vector3.up*Mathf.Sin(p*Mathf.PI)*4;fishingFish.localScale=Vector3.one*Mathf.Lerp(.65f,2.4f,p);fishingFish.LookAt(player.transform.position+Vector3.up);yield return null;}
            for(float t=0;t<.9f;t+=Time.deltaTime){hud.iris.Set(1-t/.9f);yield return null;}
            hud.iris.Set(0);yield return new WaitForSeconds(.35f);EnterInterior();
        }
        public void EnterInterior(){
            sequence=false;menu=false;lake.SetActive(false);interior.SetActive(true);fishingRod.SetActive(false);player.Teleport(entry.position);cameraRig.follow=true;cameraRig.Snap();
            Playing=true;player.controllable=true;hud.ShowPlay();PlayerPrefs.SetInt("ReturnTide.IntroSeen",1);PlayerPrefs.Save();
            StartCoroutine(OpenIris());Notify("居然……还活着。",3);
        }
        IEnumerator OpenIris(){for(float t=0;t<.7f;t+=Time.deltaTime){hud.iris.Set(t/.7f);yield return null;}hud.iris.Set(1);}
        public void AddSample(int amount){Samples+=amount;EverCollected=true;Sound(990,.15f);Notify("获得  ·  净化样本",2.5f);}
        public void CureCore(){CoreCured=true;exitGlow.gameObject.SetActive(true);Notify("心跳平稳了。该回家了。",5);}
        public void Respawn(){player.Heal();player.Teleport(checkpoint);Notify("稳态装置将你带回安全处",3);}
        public void ReturnHome(){if(CoreCured&&!ending)StartCoroutine(Ending());}
        IEnumerator Ending(){
            Playing=false;player.controllable=false;ending=true;
            for(float t=0;t<.65f;t+=Time.deltaTime){hud.iris.Set(1-t/.65f);yield return null;}
            interior.SetActive(false);lake.SetActive(true);meteor.gameObject.SetActive(false);fishingFish.gameObject.SetActive(false);fishingRod.SetActive(false);
            player.Teleport(lakeSpawn.position);cameraRig.Snap();hud.ShowEnding();yield return OpenIris();
        }
        public void Notify(string message,float seconds=3){hud.Notify(message,seconds);}
        public void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
        public void Sound(float frequency,float duration){
            if(!audioSource)return;
            int n=Mathf.RoundToInt(22050*duration);float[] data=new float[n];
            for(int i=0;i<n;i++){float t=i/22050f;float env=Mathf.Sin(Mathf.PI*i/n);data[i]=(Mathf.Sin(t*frequency*6.28318f)+.2f*Mathf.Sin(t*frequency*12.566f))*env*.13f;}
            var clip=AudioClip.Create("Tide chime",n,1,22050,false);clip.SetData(data,0);audioSource.PlayOneShot(clip);Destroy(clip,duration+.1f);
        }
        void OnDestroy(){Time.timeScale=1;if(Instance==this)Instance=null;}
    }
}

