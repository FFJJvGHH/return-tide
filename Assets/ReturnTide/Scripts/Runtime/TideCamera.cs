using UnityEngine;
namespace ReturnTide {
    public class TideCamera : MonoBehaviour {
        public Transform target;
        public Vector3 offset=new Vector3(0,15,-18);
        public float followSpeed=5;
        public bool follow=true;
        void LateUpdate() { if(follow&&target) { transform.position=Vector3.Lerp(transform.position,target.position+offset,1-Mathf.Exp(-followSpeed*Time.deltaTime));transform.rotation=Quaternion.LookRotation(-offset); } }
        public void Snap() { transform.position=target.position+offset;transform.rotation=Quaternion.LookRotation(-offset); }
    }
}

