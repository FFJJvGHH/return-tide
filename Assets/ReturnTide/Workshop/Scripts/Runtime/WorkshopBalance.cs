using UnityEngine;
namespace ReturnTide.Workshop {
 [CreateAssetMenu(menuName="Return Tide/Workshop Balance")]
 public class WorkshopBalance:ScriptableObject {
  [Header("交易")][Min(0)]public int meatValue=12,skinValue=5,boneValue=4,coreValue=85,mutationValue=28;
  [Header("工具解锁")][Min(0)]public int laserPrice=180,laserParts=2,peelerPrice=130,peelerParts=1,scannerPrice=90,knifePrice=65;
  [Header("回归")][Min(0)]public int returnPrice=650,returnParts=3;
  [Header("操作")][Min(.05f)]public float pathTolerance=.25f,laserSeconds=1.2f,dragHeight=.65f;
  [Min(1)]public int boneHits=3;
  [Header("委托 / 纯度")][Min(0)]public int meatOrderReward=110,coreOrderReward=160,researchOrderReward=140;
  [Range(0,100)]public int intactThreshold=85;
  [Min(0)]public int returnOrders=3;
 }
 [System.Serializable] public class WorkshopSave {
  public int version=2,coins,parts,deliveries,specimenIndex;public bool laser,peeler,scanner,knife,everSold;
  public int meat,skin,bone,cores,coreValue;public System.Collections.Generic.List<string> removed=new System.Collections.Generic.List<string>();
  public int learnedTools=1;public bool peelerEnabled=true,machineApplied;public int orders,familyIndex=-1,purity=100;public System.Collections.Generic.List<int> coreQualities=new System.Collections.Generic.List<int>();public System.Collections.Generic.List<int> corePrices=new System.Collections.Generic.List<int>();
 }
}



