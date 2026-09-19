using UnityEngine;
using UnityEngine.UI;
namespace ReturnTide {
    public class TideIris:MaskableGraphic {
        [Range(0,1)] public float openness=1;
        protected override void OnPopulateMesh(VertexHelper vh){
            vh.Clear();if(openness>=.999f)return;
            Rect r=rectTransform.rect;Vector2 center=r.center;float outer=r.size.magnitude;float inner=openness*outer*.55f;
            for(int i=0;i<=96;i++){float a=i*Mathf.PI*2/96;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));vh.AddVert(center+d*inner,color,Vector2.zero);vh.AddVert(center+d*outer,color,Vector2.zero);if(i>0){int k=i*2;vh.AddTriangle(k-2,k-1,k);vh.AddTriangle(k,k-1,k+1);}}
        }
        public void Set(float v){openness=v;SetVerticesDirty();}
    }
}
