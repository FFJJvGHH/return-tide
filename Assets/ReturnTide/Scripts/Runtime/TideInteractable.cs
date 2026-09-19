using UnityEngine;
namespace ReturnTide {
    public abstract class TideInteractable : MonoBehaviour {
        [Min(.1f)] public float interactionRadius=2.8f;
        public abstract string Prompt { get; }
        public virtual bool Available => isActiveAndEnabled;
        public abstract void Interact(TideGame game, bool held);
        protected virtual void OnDrawGizmosSelected() { Gizmos.color=new Color(.5f,1,.8f,.7f); Gizmos.DrawWireSphere(transform.position,interactionRadius); }
    }
}
