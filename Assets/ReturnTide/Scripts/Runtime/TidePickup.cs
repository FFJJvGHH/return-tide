using UnityEngine;
namespace ReturnTide {
    public class TidePickup : MonoBehaviour {
        public float collectRadius=2.1f;
        public int amount=1;
        Vector3 origin;
        void Start(){origin=transform.position;}
        void Update(){
            transform.position=origin+Vector3.up*Mathf.Sin(Time.time*2.4f)*.12f;transform.Rotate(0,45*Time.deltaTime,0,Space.World);
            var g=TideGame.Instance;if(!g||!g.Playing)return;
            if(Vector3.Distance(g.player.transform.position,transform.position)<collectRadius){g.AddSample(amount);Destroy(gameObject);}
        }
        void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(transform.position,collectRadius);}
    }
}
