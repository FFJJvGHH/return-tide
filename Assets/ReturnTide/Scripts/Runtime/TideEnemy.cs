using UnityEngine;
namespace ReturnTide {
    public class TideEnemy : MonoBehaviour {
        public Transform visual;
        public int health=2;
        public float detectionRadius=7,moveSpeed=1.8f,leashRadius=6;
        Vector3 home;float nextHit;
        void Start(){home=transform.position;}
        void Update(){
            var g=TideGame.Instance;if(!g||!g.Playing)return;
            Vector3 delta=g.player.transform.position-transform.position;delta.y=0;
            Vector3 goal=delta.magnitude<detectionRadius&&Vector3.Distance(home,g.player.transform.position)<leashRadius?g.player.transform.position:home;
            goal.y=transform.position.y;transform.position=Vector3.MoveTowards(transform.position,goal,moveSpeed*Time.deltaTime);
            if(visual){visual.localPosition=Vector3.up*(.5f+Mathf.Sin(Time.time*5)*.13f);visual.Rotate(0,25*Time.deltaTime,0);}
            if(delta.magnitude<1.25f&&Time.time>nextHit){nextHit=Time.time+1.2f;g.player.Damage(1);}
        }
        public void Hit(Vector3 direction){health--;transform.position+=direction*.7f;TideGame.Instance.Sound(210,.08f);if(health<=0)Destroy(gameObject);}
        void OnDrawGizmosSelected(){Gizmos.color=new Color(1,.4f,.3f,.7f);Gizmos.DrawWireSphere(transform.position,detectionRadius);}
    }
}
