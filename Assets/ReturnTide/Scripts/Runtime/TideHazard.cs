using UnityEngine;
namespace ReturnTide {
    public class TideHazard : MonoBehaviour {
        public int damage=1;
        public bool returnToCheckpoint;
        void OnTriggerStay(Collider other){var p=other.GetComponent<TidePlayer>();if(!p)return;p.Damage(damage);if(returnToCheckpoint&&p.controllable)p.Teleport(TideGame.Instance.checkpoint);}
        void OnDrawGizmosSelected(){Gizmos.color=new Color(1,.2f,.2f,.3f);var b=GetComponent<BoxCollider>();if(b){Gizmos.matrix=transform.localToWorldMatrix;Gizmos.DrawCube(b.center,b.size);}}
    }
}
