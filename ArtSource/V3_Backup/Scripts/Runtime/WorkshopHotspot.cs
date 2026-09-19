using UnityEngine;
namespace ReturnTide.Workshop {
 public enum WorkshopAction{Trade,Next,SellGoods,SellParts,BuyLaser,BuyPeeler,BuyScanner,BuyKnife,Return,Back,Scalpel,Hammer,Forceps,Laser,DeliverOrder}
 public class WorkshopHotspot:MonoBehaviour {
  public WorkshopAction action;
  public string label;
  public bool tradeOnly;
  void OnDrawGizmosSelected(){Gizmos.color=new Color(.6f,1,.85f);Gizmos.DrawWireSphere(transform.position,.35f);}
 }
}

