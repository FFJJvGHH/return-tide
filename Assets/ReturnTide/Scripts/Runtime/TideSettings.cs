using UnityEngine;
namespace ReturnTide {
    [CreateAssetMenu(menuName="Return Tide/Game Balance")]
    public class TideSettings : ScriptableObject {
        [Header("科研员")][Min(1)] public int maxHealth=5;
        [Min(.1f)] public float moveSpeed=5.8f, dashSpeed=15, dashDuration=.2f, dashCooldown=.85f;
        [Header("切除与战斗")][Min(.1f)] public float cutSeconds=1.8f, interactionRange=2.8f, attackRadius=2.8f, attackCooldown=.42f;
        [Header("关卡")][Min(1)] public int requiredSamples=3;
        public Color mint=new Color(.55f,1,.83f), coral=new Color(1,.45f,.4f);
    }
}
