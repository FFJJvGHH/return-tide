using UnityEngine;
using UnityEngine.UI;
namespace ReturnTide.Workshop {
 public enum TideGlyph { Scalpel,Hammer,Forceps,Laser,Meat,Bone,Organ,Crystal,Coin,Skin,Bell,Fish,Arrow,Warning,Purity,Scanner,Check,Lock,Mouse,Order,Tray }
 [AddComponentMenu("Return Tide/Vector Icon")]
 [RequireComponent(typeof(CanvasRenderer))]
 public class WorkshopIcon:MaskableGraphic {
  public TideGlyph glyph;
  [Range(0,1)]public float fill=1;
  public float stroke=2.2f;
  public int GeneratedVertices{get;private set;}
  protected WorkshopIcon(){useLegacyMeshGeneration=false;}
  VertexHelper mesh;Vector2 origin;float unit;
  protected override void OnPopulateMesh(VertexHelper vh){mesh=vh;vh.Clear();Rect r=rectTransform.rect;origin=r.center;unit=Mathf.Min(r.width,r.height)/32f;
   switch(glyph){
    case TideGlyph.Scalpel:Line(-9,-11,5,5,3);Poly(new[]{new Vector2(3,4),new Vector2(11,13),new Vector2(8,3)});break;
    case TideGlyph.Hammer:Line(-6,-12,2,5,3);Poly(new[]{new Vector2(-6,9),new Vector2(-3,14),new Vector2(11,7),new Vector2(8,2)});break;
    case TideGlyph.Forceps:Line(-7,-12,3,12);Line(-1,-12,3,12);Line(-7,-12,-4,-10);Line(-1,-12,-4,-10);break;
    case TideGlyph.Laser:Line(-5,-4,4,10,5);Line(-7,-7,-12,-13,1.4f);Line(-3,-6,-8,-11,1.4f);Circle(4,10,4);break;
    case TideGlyph.Meat:Ellipse(0,0,12,9);Ellipse(2,1,6,4);Line(-7,2,-4,5,1.2f);Line(-7,-3,-4,0,1.2f);break;
    case TideGlyph.Bone:Line(-7,-6,7,6,4);Disk(-10,-7,3);Disk(-7,-10,3);Disk(10,7,3);Disk(7,10,3);break;
    case TideGlyph.Organ:Poly(new[]{new Vector2(-9,9),new Vector2(-12,0),new Vector2(-5,-10),new Vector2(4,-11),new Vector2(12,-3),new Vector2(10,7),new Vector2(2,11)});Line(-3,6,1,-5,1.5f,new Color(.07f,.14f,.15f,color.a));break;
    case TideGlyph.Crystal:case TideGlyph.Purity:Poly(new[]{new Vector2(0,14),new Vector2(10,4),new Vector2(7,-7),new Vector2(0,-14),new Vector2(-7,-7),new Vector2(-10,4)});Line(0,12,-3,2,1.3f,new Color(.08f,.25f,.23f,color.a));Line(-3,2,0,-11,1.3f,new Color(.08f,.25f,.23f,color.a));if(glyph==TideGlyph.Purity&&fill<.86f){Line(-8,6,3,0,2,new Color(.1f,.15f,.16f,color.a));Line(3,0,-3,-6,2,new Color(.1f,.15f,.16f,color.a));}break;
    case TideGlyph.Coin:Circle(0,0,12);Circle(0,0,8);Line(-2,-5,2,5,2.5f);break;
    case TideGlyph.Skin:Poly(new[]{new Vector2(-10,-9),new Vector2(8,-12),new Vector2(12,9),new Vector2(-7,12)});Line(-5,-5,0,7,1.2f,new Color(.14f,.12f,.1f,color.a));break;
    case TideGlyph.Bell:Line(-11,-6,11,-6);Line(-8,-5,-7,5);Line(8,-5,7,5);Arc(0,5,7,0,180);Disk(0,-10,2);Line(0,12,0,15);break;
    case TideGlyph.Fish:Ellipse(-1,0,10,7);Poly(new[]{new Vector2(8,0),new Vector2(15,8),new Vector2(15,-8)});Disk(-6,1,1.5f);break;
    case TideGlyph.Arrow:Line(-12,0,10,0);Line(4,7,11,0);Line(4,-7,11,0);break;
    case TideGlyph.Warning:Line(0,13,-13,-10);Line(-13,-10,13,-10);Line(13,-10,0,13);Line(0,5,0,-2,3);Disk(0,-6,1.6f);break;
    case TideGlyph.Scanner:Circle(-2,3,8);Line(4,-4,12,-12,3);Line(-6,3,2,3,1);Line(-2,-1,-2,7,1);break;
    case TideGlyph.Check:Line(-10,0,-3,-7,3);Line(-3,-7,11,10,3);break;
    case TideGlyph.Lock:Arc(0,5,6,0,180);Line(-6,5,-6,0);Line(6,5,6,0);Poly(new[]{new Vector2(-10,1),new Vector2(10,1),new Vector2(10,-12),new Vector2(-10,-12)});Disk(0,-5,2,new Color(.08f,.15f,.17f,color.a));break;
    case TideGlyph.Mouse:Ellipse(0,0,8,12);Line(0,11,0,2);Line(-7,2,7,2);Poly(new[]{new Vector2(-6,3),new Vector2(-5,8),new Vector2(-1,10),new Vector2(-1,3)});break;
    case TideGlyph.Order:Line(-9,-13,-9,12);Line(-9,12,9,12);Line(9,12,9,-13);Line(9,-13,-9,-13);Line(-4,6,5,6,1.4f);Line(-4,1,5,1,1.4f);Line(-4,-5,1,-5,1.4f);break;
    case TideGlyph.Tray:Arc(0,0,12,180,360);Line(-12,0,12,0);Line(-8,-12,8,-12);break;
   }GeneratedVertices=vh.currentVertCount;
  }
  void V(Vector2 p,Color c){mesh.AddVert(origin+p*unit,c,Vector2.zero);}
  void Line(float ax,float ay,float bx,float by,float width=-1,Color? tint=null){Vector2 a=new Vector2(ax,ay),b=new Vector2(bx,by);Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*(width<0?stroke:width)*.5f;int k=mesh.currentVertCount;Color c=tint??color;V(a-n,c);V(a+n,c);V(b+n,c);V(b-n,c);mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);}
  void Poly(Vector2[] points){int k=mesh.currentVertCount;foreach(var p in points)V(p,color);for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(k,k+i,k+i+1);}
  void Circle(float x,float y,float radius){Arc(x,y,radius,0,360);}
  void Arc(float x,float y,float radius,float start,float end){int n=Mathf.CeilToInt((end-start)/12);for(int i=0;i<n;i++){float a=Mathf.Lerp(start,end,i/(float)n)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(i+1f)/n)*Mathf.Deg2Rad;Line(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius);}}
  void Ellipse(float x,float y,float rx,float ry){for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Line(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry);}}
  void Disk(float x,float y,float r,Color? tint=null){int k=mesh.currentVertCount;Color c=tint??color;V(new Vector2(x,y),c);for(int i=0;i<=20;i++){float a=i*Mathf.PI/10;V(new Vector2(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r),c);if(i>0)mesh.AddTriangle(k,k+i,k+i+1);}}
  public void Set(TideGlyph value){glyph=value;SetVerticesDirty();}
 }
}
