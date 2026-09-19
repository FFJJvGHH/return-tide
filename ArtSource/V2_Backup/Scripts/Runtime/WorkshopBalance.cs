using UnityEngine;
namespace ReturnTide.Workshop {
 [CreateAssetMenu(menuName="Return Tide/Workshop Balance")]
 public class WorkshopBalance:ScriptableObject {
  [Header("交易")][Min(0)]public int meatValue=12,skinValue=5,boneValue=4,coreValue=85,mutationValue=28;
  [Header("工具解锁")][Min(0)]public int laserPrice=180,laserParts=2,peelerPrice=130,peelerParts=1,scannerPrice=90,knifePrice=65;
  [Header("回归")][Min(0)]public int returnPrice=650,returnParts=3;
  [Header("操作")][Min(.05f)]public float pathTolerance=.25f,laserSeconds=1.2f,dragHeight=.65f;
  [Min(1)]public int boneHits=3;
 }
 [System.Serializable] public class WorkshopSave {
  public int version=2,coins,parts,deliveries,specimenIndex;public bool laser,peeler,scanner,knife,everSold;
  public int meat,skin,bone,cores,coreValue;public System.Collections.Generic.List<string> removed=new System.Collections.Generic.List<string>();
 }
}
