using UnityEngine;
namespace ReturnTide.Workshop {
 [CreateAssetMenu(menuName="Return Tide/Specimen Profile")]
 public class SpecimenProfile:ScriptableObject {
  public string displayName;
  [TextArea]public string handlingClue;
  [Range(0,3)]public int family;
  [Min(1)]public int corePrice=85,meatYield=1,boneHits=3;
  [Range(-1,2)]public int toxicOrgan=-1;
  [Range(0,30)]public int machinePurityLoss;
  [Min(0)]public int requiredOrders;
 }
}
