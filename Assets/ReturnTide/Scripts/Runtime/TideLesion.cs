using UnityEngine;
namespace ReturnTide {
    public class TideLesion : TideInteractable {
        public bool isCore;
        public GameObject infectedVisual,healthyVisual,samplePrefab;
        public Collider barrier;
        public float cutDuration=1.8f;
        [Range(0,1)] public float progress;
        public bool removed;
        float lastHeld;
        public override bool Available => base.Available&&!removed;
        public override string Prompt => isCore&&!TideGame.Instance.HasStabilizer?"核心不稳定 · 先找鱼贩子":"长按 E  ·  切除";
        void Update() {
            if(removed)return;
            if(Time.time-lastHeld>.12f)progress=Mathf.MoveTowards(progress,0,Time.deltaTime*.8f);
            if(infectedVisual) infectedVisual.transform.localScale=Vector3.one*(1+Mathf.Sin(Time.time*2.5f)*.035f);
        }
        public override void Interact(TideGame game,bool held) {
            if(removed||!held||(isCore&&!game.HasStabilizer))return;
            lastHeld=Time.time; progress+=Time.deltaTime/cutDuration;
            if(progress>=1) Complete(game);
        }
        public void Complete(TideGame game) {
            if(removed)return;removed=true;progress=1;
            infectedVisual.SetActive(false);healthyVisual.SetActive(true);if(barrier)barrier.enabled=false;
            game.Sound(isCore?880:660,.3f);
            if(isCore)game.CureCore();
            else { game.CutCount++; if(samplePrefab) { var sample=Instantiate(samplePrefab,transform.position+Vector3.up*1.1f,Quaternion.identity);sample.name="可收集 · 净化样本"; } game.Notify("感染已切除"); }
        }
    }
}
