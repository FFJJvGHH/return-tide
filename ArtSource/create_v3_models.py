import bpy, math, os
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(ROOT,'Assets','ReturnTide','Workshop','Models')
P={'FishSkin':'487F80','FishDark':'1D3745','Pearl':'AAC8B6','Flesh':'A96F76','Organ':'71465F','Bone':'D7C7A3','Core':'6DF3C6','Gold':'C2A26D','Metal':'475A68','Ivory':'DEDAC6','Ink':'213240','Teal':'438D84','Coral':'B97E77','Skin':'ECC9AE','Hair':'9EBBB9','Dark':'253746','Leather':'735648','Glow':'88DAB8','Copper':'AA7951'}
def mat(n):
 m=bpy.data.materials.get(n) or bpy.data.materials.new(n);h=P[n];m.diffuse_color=(*[int(h[i:i+2],16)/255 for i in (0,2,4)],1);return m
def reset():bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def node(n,p=None,pos=(0,0,0)):
 o=bpy.data.objects.new(n,None);bpy.context.collection.objects.link(o);o.location=pos;o.parent=p;return o
def mesh(n,v,f,m,p=None):
 d=bpy.data.meshes.new(n);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.data.materials.append(mat(m));o.parent=p;return o
def uv(n,pos,s,m,p=None,smooth=False):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=pos);o=bpy.context.object;o.name=n;o.scale=s;o.data.materials.append(mat(m));o.parent=p
 if smooth:
  for face in o.data.polygons:face.use_smooth=True
 return o
def ico(n,pos,s,m,p=None):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,location=pos);o=bpy.context.object;o.name=n;o.scale=s;o.data.materials.append(mat(m));o.parent=p;return o
def tube(n,points,r,m,p=None,sides=8):
 v=[];f=[]
 for i,point in enumerate(points):
  tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)]);tangent.normalize();u=tangent.cross(Vector((0,0,1)))
  if u.length<.01:u=tangent.cross(Vector((0,1,0)))
  u.normalize();w=tangent.cross(u).normalized();radius=r[i] if isinstance(r,list) else r
  for j in range(sides):v.append(Vector(point)+radius*(u*math.cos(j*math.tau/sides)+w*math.sin(j*math.tau/sides)))
  if i:
   for j in range(sides):a=(i-1)*sides+j;b=(i-1)*sides+(j+1)%sides;f.append((a,b,b+sides,a+sides))
 f.append(tuple(reversed(range(sides))));f.append(tuple((len(points)-1)*sides+j for j in range(sides)))
 return mesh(n,v,f,m,p)
def box(n,pos,s,m,p=None,bevel=.035):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=n;o.dimensions=s;o.data.materials.append(mat(m));o.parent=p;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Edge bevel','BEVEL');mod.width=bevel;mod.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
def fin(n,base,edge,m,p):
 v=[base]+edge;faces=[]
 for i in range(len(edge)-1):faces.extend([(0,i+1,i+2),(0,i+2,i+1)])
 mesh(n,v,faces,m,p)
 for point in edge[::2]:tube(n+'Ray',[base,point],.012,'Gold',p,5)
def export(name):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource',name+'.blend'))
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)

def fish(kind,name):
 reset();base=node('Permanent');width=[.63,.81,.43,.62][kind];height=[.47,.59,.35,.40][kind]
 def shape(x):return .22+.78*max(0,math.sin(math.pi*max(0,min(1,(x+1.72)/3.22))))**.65
 def profile(x):return width*shape(x)
 # Closed lower envelope, separate from the removable dorsal panels.
 v=[];f=[];rings=19;around=18
 for i in range(rings):
  x=-1.80+i*3.55/(rings-1);taper=.45+.55*math.sin(math.pi*i/(rings-1))**.55
  for j in range(around):
   a=j*math.tau/around;v.append((x,math.cos(a)*width*taper,.17+math.sin(a)*.18))
 for i in range(rings-1):
  for j in range(around):a=i*around+j;b=i*around+(j+1)%around;f.append((a,b,b+around,a+around))
 mesh('BellyEnvelope',v,f,'FishDark',base)
 # Head, recessed jaws, gills, small scale fields and thin fins.
 uv('Head',(1.52,0,.26),([.59,.69,.58,.47][kind],width*(1.15 if kind==3 else .9),height*(.6 if kind==3 else .85)),'FishSkin',base)
 uv('Jaw',(1.68,0,.10),(.53,width*.75,.15),'Pearl',base)
 uv('Mouth',(1.98,0,.23),(.09,width*.52,.15),'FishDark',base)
 for side in [-1,1]:
  uv('EyeRim',(1.54,side*width*.82,.44),(.14,.085,.14),'Gold',base)
  uv('Eye',(1.57,side*width*.92,.46),(.075,.04,.09),'Ink',base,True)
  uv('EyeGlint',(1.55,side*width*.98,.49),(.015,.01,.022),'Pearl',base)
  for i in range(4):tube('Gill',[(1.21-i*.09,side*width*.91,.22),(1.18-i*.09,side*width*.98,.36),(1.10-i*.09,side*width*.82,.48)],.013,'FishDark',base)
  for i in range(8):
   x=-1.4+i*.31
   tube('ScaleEtching',[(x-.08,side*width*.92,.24),(x,side*width*.99,.27),(x+.10,side*width*.95,.31)],.009,'Pearl',base,5)
  span=1.35 if kind==3 else .7 if kind==1 else .42
  edge=[]
  for j in range(13):
   t=j/12;x=.92-t*2.1;extent=math.sin(math.pi*t)**.75*span;edge.append((x,side*(profile(x)*.92+extent),.15+math.sin(math.pi*t)*.09))
  fin('PectoralFin',(.20,side*width*.40,.20),edge,'FishSkin',base)
 tail=node('Tail',base)
 if kind==2:
  tube('EelTail',[(-1.5,0,.17),(-2.0,.08,.13),(-2.45,.36,.10),(-2.7,.53,.2)],[.20,.15,.085,.01],'FishSkin',tail)
 else:fin('ForkTail',(-1.65,0,.17),[(-2.3,-.62,.24),(-2.10,-.24,.2),(-2.25,0,.18),(-2.10,.24,.2),(-2.3,.62,.24)],'FishSkin',tail)
 for side in [-1,1]:
  tube('LateralLine',[(-1.55,side*width*.90,.23),(-.65,side*width,.24),(.25,side*width,.24),(.95,side*width*.95,.27)],.016,'Gold',base)
 for i,(left,right) in enumerate([(-1.5,-.55),(-.55,.4),(.4,1.22)]):
  g=node('Skin_'+str(i));v=[];f=[]
  for ix in range(6):
   x=left+(right-left)*ix/5;w=profile(x)
   for j in range(13):a=j*math.pi/12;v.append((x,math.cos(a)*w,.24+math.sin(a)*height*shape(x)))
  for ix in range(5):
   for j in range(12):a=ix*13+j;f.append((a,a+1,a+14,a+13))
  skin=mesh('DorsalSkin',v,f,'FishSkin',g);skin.data.materials.append(mat('FishDark'));skin.data.materials.append(mat('Pearl'))
  for face in skin.data.polygons:
   j=face.index%12;face.material_index=1 if 4<=j<=7 else 2 if j==0 or j==11 else 0
  for ix in range(4):
   x=left+.12+ix*(right-left-.2)/3
   for side in [-1,1]:tube('ScaleMark',[(x-.06,side*profile(x)*.55,.24+height*shape(x)*.84),(x,side*profile(x)*.6,.24+height*shape(x)*.80),(x+.06,side*profile(x)*.55,.24+height*shape(x)*.84)],.006,'Pearl',g,5)
  x=(left+right)/2
  for j in range(5):y=profile(x)*(.78-j*.39);z=.24+height*shape(x)*math.sqrt(max(0,1-(y/profile(x))**2))+.04;node('Cut'+str(j),g,(x,y,z))
 for i,x in enumerate([-.80,.45]):
  g=node('Flesh_'+str(i));uv('Muscle',(x,0,.33),(.68,width*.82,.19),'Flesh',g)
  for j in range(6):
   tube('Myomere',[(x-.44+j*.16,-width*.65,.40),(x-.50+j*.16,0,.52),(x-.44+j*.16,width*.65,.40)],.014,'Pearl',g,5)
  for j in range(5):y=width*(.58-j*.29);node('Cut'+str(j),g,(x,y,.35+.19*math.sqrt(1-(y/(width*.82))**2)+.04))
 for i,x in enumerate([-.97,-.12,.73]):
  g=node('Bone_'+str(i));tube('Spine',[(x-.35,0,.43),(x,0,.48),(x+.35,0,.43)],.065 if kind!=3 else .045,'Bone',g)
  for side in [-1,1]:tube('Rib',[(x,0,.47),(x,side*width*.35,.50),(x+.08,side*width*.72,.39),(x+.19,side*width*.78,.21)],.055 if kind==1 else .035,'Bone',g)
 for i,x in enumerate([-.99,-.12,.76]):
  g=node('Organ_'+str(i));toxic=kind==2 and i==1
  uv('ToxicSac' if toxic else 'LiverLobe',(x,0,.24),(.34,width*.60,.15),'Teal' if toxic else 'Organ',g)
  for side in [-1,1]:tube('Vein',[(x-.24,0,.34),(x,side*width*.22,.38),(x+.2,side*width*.40,.30)],.012,'Core' if toxic else 'Flesh',g)
 g=node('Crystal');ico('Prism',(0,0,.24),(.17,.15,.31),'Core',g)
 for j in [-1,1]:ico('Shard',(j*.14,0,.18),(.08,.08,.19),'Core',g)
 for i,x in enumerate([-1.1,-.25,.60]):
  g=node('Shell_'+str(i))
  verts=[];faces=[]
  for ix in range(4):
   xx=x-.39+ix*.25
   for j in range(9):a=j*math.pi/8;verts.append((xx+.055*math.sin(a),math.cos(a)*profile(xx)*1.08,.29+math.sin(a)*(height*shape(xx)+.14)))
  for ix in range(3):
   for j in range(8):a=ix*9+j;faces.append((a,a+1,a+10,a+9))
  mesh('Scute',verts,faces,'Metal',g)
  edge=[verts[j] for j in range(9)];tube('ScuteEdge',edge,.013,'Gold',g)
  ico('Spine',(x,0,.35+height*shape(x)+.13),(.075,.065,.15),'Bone',g)
 tent=node('Tentacles')
 for side in [-1,1]:
  tube('Barbel',[(1.92,side*.16,.15),(2.25,side*.32,.13),(2.42,side*.65,.21),(2.26,side*.85,.24)],[.065,.046,.022,.005],'Organ',tent)
 tube('LureStalk',[(1.36,0,.57),(1.22,0,.95),(1.55,0,1.17),(1.93,0,1.05)],[.038,.027,.022,.016],'FishDark',tent);uv('Lure',(1.93,0,1.03),(.085,.085,.13),'Core',tent)
 crown=node('GoldenCrown')
 for i in range(7):x=-1.4+i*.36;ico('DorsalCrystal',(x,(-1 if i%2 else 1)*.33,.65),(.12,.16,.23+(i%3)*.1),'Core',crown)
 export(name)

for k,n in enumerate(['Specimen_Reef','Specimen_Armor','Specimen_Venom','Specimen_Crystal']):fish(k,n)
# Refine the existing scientist into coherent face, hair masses and tailored clothing.
bpy.ops.wm.open_mainfile(filepath=os.path.join(ROOT,'ArtSource','Mio_Researcher.blend'))
for o in list(bpy.data.objects):
 if o.name.startswith(('Fringe','Hair','Head','Eye','Cheek','Mouth','Goggles','Coat_','Collar')):bpy.data.objects.remove(o,do_unlink=True)
uv('Head',(0,-.025,1.85),(.355,.295,.39),'Skin',smooth=True)
# Scalp with a front hairline, swept ribbons rather than floating pointed blobs.
uv('Hair_Back',(0,.085,1.9),(.385,.30,.43),'Hair',smooth=True)
v=[];f=[]
for j in range(13):
 a=math.pi*.1+j*math.pi*.8/12
 for i in range(9):
  b=-math.pi*.07+i*math.pi*1.14/8;x=math.cos(a)*.39;y=math.sin(b)*.31;z=1.96+math.sin(a)*math.cos(b)*.30
  v.append((x,y,z))
for j in range(12):
 for i in range(8):a=j*9+i;f.append((a,a+1,a+10,a+9))
mesh('Hair_Cap',v,f,'Hair')
v=[];f=[]
for j in range(5):
 t=j/4
 for i in range(25):
  x=-.33+i*.66/24;bottom=2.015+.025*math.cos(i*.65)+(.04 if x<-.05 else -.015)
  v.append((x-.025*t,-.16-.15*math.sin(t*math.pi*.5)+abs(x)*.12,2.22*(1-t)+bottom*t))
for j in range(4):
 for i in range(24):a=j*25+i;f.append((a,a+25,a+26,a+1))
fringe=mesh('Fringe',v,f,'Hair')
for face in fringe.data.polygons:face.use_smooth=True
for side in [-1,1]:
 tube('Hair_Side',[(side*.30,.02,2.10),(side*.37,-.01,1.91),(side*.36,.04,1.68),(side*.28,.08,1.57)],[.13,.12,.10,.02],'Hair',sides=10)
 x=side*.135
 uv('Eye',(x,-.300,1.87),(.080,.007,.097),'Ivory',smooth=True)
 uv('Eye',(x,-.307,1.866),(.041,.004,.070),'Teal',smooth=True)
 uv('Eye',(x,-.312,1.872),(.023,.003,.047),'Ink',smooth=True)
 uv('Eye',(x-.018,-.317,1.90),(.011,.002,.015),'Ivory',smooth=True)
 tube('Eye_Lid',[(x-.068,-.302,1.92),(x-.032,-.310,1.945),(x+.014,-.311,1.948),(x+.065,-.301,1.925)],.006,'Ink',sides=6)
 uv('Cheek',(side*.23,-.251,1.755),(.045,.003,.017),'Coral',smooth=True)
 # Segmented circular goggles with a separate glass insert.
 ring=[(x+math.cos(a*math.tau/24)*.12,-.20,2.20+math.sin(a*math.tau/24)*.10) for a in range(25)]
 tube('Goggles_Rim',ring,.021,'Copper',sides=8);uv('Goggles_Glass',(x,-.20,2.20),(.097,.025,.081),'Teal',smooth=True)
tube('Goggles_Bridge',[(-.045,-.2,2.2),(.045,-.2,2.2)],.014,'Copper');uv('Head_Nose',(0,-.319,1.77),(.027,.026,.027),'Skin',smooth=True)
tube('Mouth',[(-.031,-.304,1.701),(0,-.312,1.693),(.031,-.304,1.701)],.008,'Coral',sides=6)
# Coat profile with tailored waist, hem, trim and lapels.
v=[];f=[];levels=[(.60,.38,.24),(.69,.42,.26),(1.02,.29,.23),(1.29,.29,.22),(1.46,.28,.19)]
for z,rx,ry in levels:
 for i in range(16):a=i*math.tau/16;v.append((math.cos(a)*rx,math.sin(a)*ry,z))
for j in range(len(levels)-1):
 for i in range(16):a=j*16+i;b=j*16+(i+1)%16;f.append((a,b,b+16,a+16))
mesh('Coat_Tailored',v,f,'Ivory')
for side in [-1,1]:
 mesh('Collar_Lapel',[(side*.03,-.21,1.39),(side*.24,-.19,1.46),(side*.19,-.26,1.16)],[(0,1,2),(2,1,0)],'Teal')
 box('Coat_Pocket',(side*.225,-.233,.95),(.15,.028,.16),'Ivory',bevel=.018)
 tube('Coat_PocketSeam',[(side*.15,-.255,1.00),(side*.29,-.255,1.00)],.009,'Gold')
for z in [.79,.94,1.09]:uv('Coat_Button',(0,-.25,z),(.020,.013,.020),'Copper')
tube('Coat_Hem',[(-.30,-.19,.66),(0,-.265,.64),(.30,-.19,.66)],.018,'Teal')
export('Mio_Refined')
reset()
uv('Palm',(0,0,0),(.66,.63,.28),'Teal',smooth=True)
for i in range(4):
 uv('Finger',(-.43+i*.28,-.65,.015),(.16,.36+(0.08 if i in [1,2] else 0),.19),'Teal',smooth=True)
uv('Thumb',(.63,-.18,0),(.22,.40,.20),'Teal',smooth=True)
box('Cuff',(0,.48,.015),(1.05,.24,.46),'Ivory',bevel=.09)
export('Research_Glove')
bpy.ops.wm.open_mainfile(filepath=os.path.join(ROOT,'ArtSource','Fishmonger.blend'))
for side in [-1,1]:
 x=side*.15
 uv('EyeGlint',(x-.01,-.323,1.72),(.009,.004,.013),'Ivory',smooth=True)
 tube('Brow',[(x-.06,-.30,1.78),(x,-.316,1.80),(x+.06,-.30,1.78)],.015,'Leather',sides=6)
 box('ApronPocket',(side*.20,-.401,.85),(.25,.035,.19),'Ivory',bevel=.015)
 tube('PocketSeam',[(side*.20-.11,-.423,.92),(side*.20+.11,-.423,.92)],.009,'Leather',sides=6)
for i in range(5):
 radius=.26+i*.057;z=2.10-i*.045
 ring=[(math.cos(j*math.tau/32)*radius,math.sin(j*math.tau/32)*radius,z) for j in range(33)]
 tube('WovenHatBand',ring,.009,'Leather',sides=5)
uv('Nose',(0,-.35,1.65),(.065,.07,.06),'Skin',smooth=True)
for side in [-1,1]:tube('Moustache',[(0,-.40,1.59),(side*.12,-.40,1.57),(side*.23,-.34,1.52)],[.045,.054,.01],'Ivory',sides=8)
export('Fishmonger_Refined')
print('V3_SCULPTED_MODELS_READY')



