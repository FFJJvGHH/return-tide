using UnityEngine;
namespace ReturnTide {
    public class TideFloat:MonoBehaviour {
        public float height=.15f,speed=1,rotation=0;Vector3 origin;float phase;
        void Start(){origin=transform.localPosition;phase=transform.position.x*2;}
        void Update(){transform.localPosition=origin+Vector3.up*Mathf.Sin(Time.time*speed+phase)*height;transform.Rotate(0,rotation*Time.deltaTime,0);}
    }
}
