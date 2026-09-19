import bpy, math, os
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(ROOT,'Assets','ReturnTide','Workshop','Models');os.makedirs(OUT,exist_ok=True)
P={'FishSkin':'427E80','FishDark':'183F49','Pearl':'ABC8BD','Flesh':'AB666D','Organ':'753F62','Bone':'DBC7A2','Core':'6CFFC9','Gold':'CCAB69','Metal':'414F62'}
def mat(n):
 m=bpy.data.materials.get(n) or bpy.data.materials.new(n);h=P[n];m.diffuse_color=(*[int(h[i:i+2],16)/255 for i in (0,2,4)],1);return m
def uv(n,p,s,m,seg=16,rings=10):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=p);o=bpy.context.object;o.name=n;o.scale=s;o.data.materials.append(mat(m));return o
def ico(n,p,s,m):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=p);o=bpy.context.object;o.name=n;o.scale=s;o.data.materials.append(mat(m));return o
def rod(n,a,b,r,m):
 a,b=Vector(a),Vector(b);bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=r,radius2=r*.75,depth=(b-a).length,location=(a+b)/2);o=bpy.context.object;o.name=n;o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();o.data.materials.append(mat(m));return o
def group(n):
 o=bpy.data.objects.new(n,None);bpy.context.collection.objects.link(o);return o
def parent(o,p):o.parent=p;return o
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
base=group('Permanent')
parent(uv('Belly',(-.05,0,.12),(1.85,.79,.24),'FishDark'),base)
parent(uv('Head',(1.61,0,.27),(.62,.69,.49),'FishSkin'),base)
for y in [-.52,.52]:
 parent(uv('EyeSocket',(1.8,y,.5),(.21,.15,.2),'Gold'),base)
 parent(uv('Eye',(1.85,y*1.16,.54),(.10,.045,.13),'FishDark'),base)
 parent(uv('EyeGlint',(1.82,y*1.2,.6),(.025,.025,.032),'Pearl'),base)
 parent(ico('PectoralFin',(.7,y*1.45,.14),(.58,.6,.07),'FishSkin'),base)
 for i in range(3):parent(rod('Gill',(1.26-i*.12,y*1.2,.26),(1.26-i*.12,y*1.11,.5),.024,'FishDark'),base)
parent(ico('Tail',(-2.15,0,.20),(.76,1.0,.10),'FishSkin'),base)
for i in [-1,0,1]:parent(rod('TailVein',(-1.7,0,.24),(-2.55,i*.7,.26),.025,'Gold'),base)
for i in range(6):
 x=-1.4+i*.5
 for side in [-1,1]:parent(uv('ScaleRim',(x,side*.68,.28),(.31,.15,.18),'Pearl',10,6),base)
for i,(left,right) in enumerate([(-1.5,-.55),(-.55,.4),(.4,1.25)]):
 g=group('Skin_'+str(i));verts=[];faces=[]
 for ix in range(4):
  x=left+(right-left)*ix/3
  for k in range(9):
   a=k*math.pi/8;verts.append((x,math.cos(a)*.72,.27+math.sin(a)*(.52-.06*abs(x))))
 for ix in range(3):
  for k in range(8):
   a=ix*9+k;faces.append((a,a+1,a+10,a+9))
 mesh=bpy.data.meshes.new('PeelMesh');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('SkinPanel',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mat('FishSkin'));o.parent=g
 for ix in range(3):
  x=left+.15+ix*.25
  parent(uv('SkinMark',(x,-.25,.73-abs(x)*.06),(.07,.12,.015),'Pearl',8,6),g)
for i,x in enumerate([-.8,.45]):
 g=group('Flesh_'+str(i));parent(uv('Muscle',(x,0,.36),(.68,.6,.25),'Flesh'),g)
 for j in range(4):parent(rod('MuscleFibre',(x-.35+j*.2,-.48,.47),(x-.48+j*.2,.48,.47),.018,'Pearl'),g)
for i,x in enumerate([-.95,-.1,.75]):
 g=group('Bone_'+str(i));
 for side in [-1,1]:
  points=[(x,0,.51),(x,side*.25,.55),(x+.08,side*.5,.42),(x+.18,side*.58,.26)]
  for j in range(3):parent(rod('Rib',points[j],points[j+1],.055,'Bone'),g)
 parent(rod('Vertebra',(x-.33,0,.51),(x+.33,0,.51),.075,'Bone'),g)
for i,x in enumerate([-1.0,-.1,.8]):
 g=group('Organ_'+str(i));parent(uv('Lobe',(x,0,.26),(.38,.48,.18),'Organ'),g)
 for side in [-1,1]:parent(rod('Vein',(x-.25,0,.44),(x+.12,side*.24,.38),.014,'Flesh'),g)
g=group('Crystal');parent(ico('CrystalHeart',(0,0,.28),(.24,.23,.42),'Core'),g)
for j in [-1,1]:parent(ico('CrystalShard',(j*.18,0,.2),(.12,.13,.24),'Core'),g)
for i,x in enumerate([-1.15,-.35,.45]):
 g=group('Shell_'+str(i));parent(ico('ExoskeletonPlate',(x,0,.77),(.59,.82,.22),'Metal'),g)
 for j in [-1,1]:parent(ico('ShellSpine',(x,j*.40,.92),(.11,.10,.33),'Gold'),g)
g=group('Tentacles')
for i in range(5):
 a=i*math.pi*2/5;points=[]
 for j in range(8):
  t=j/7;points.append((1.8+math.cos(a)*t*.9,math.sin(a)*t*.9,.2+math.sin(t*5+i)*.12))
 for j in range(7):parent(rod('Tentacle',points[j],points[j+1],.085*(1-j/9),'Organ'),g)
g=group('GoldenCrown')
for i in range(5):parent(ico('CrownTip',(1.45+(i-2)*.15,0,.88),(.10,.15,.3),'Gold'),g)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource','Workshop_Specimen.blend'))
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Workshop_Specimen.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
print('WORKSHOP_ANATOMY_READY')
