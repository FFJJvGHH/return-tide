using UnityEngine;
namespace ReturnTide {
    public class TideExit : TideInteractable {
        public override bool Available=>base.Available&&TideGame.Instance.CoreCured;
        public override string Prompt=>"E  ·  回归湖岸";
        public override void Interact(TideGame game,bool held){if(Input.GetKeyDown(KeyCode.E))game.ReturnHome();}
    }
}
