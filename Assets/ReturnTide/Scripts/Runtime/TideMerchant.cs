using UnityEngine;
namespace ReturnTide {
    public class TideMerchant : TideInteractable {
        public Transform checkpoint;
        public override string Prompt => TideGame.Instance.HasStabilizer?"E  ·  鱼贩子阿鲤":"E  ·  与鱼贩子交谈";
        public override void Interact(TideGame g,bool held) {
            if(!Input.GetKeyDown(KeyCode.E))return;
            Trade(g);
        }
        public void Trade(TideGame g) {
            g.checkpoint=checkpoint.position;g.player.Heal();
            if(g.HasStabilizer)g.Notify("阿鲤：切掉深处的陨星核。咱们一起回岸上。",5);
            else if(g.Samples>=g.settings.requiredSamples){g.Samples-=g.settings.requiredSamples;g.HasStabilizer=true;g.Sound(740,.35f);g.Notify("阿鲤：稳态针拿好。鱼还有救，我们也是。",5);}
            else g.Notify("阿鲤：我卖鱼一辈子，头回被鱼收了。带来 "+g.settings.requiredSamples+" 份样本，我给你稳态针。",6);
        }
    }
}
