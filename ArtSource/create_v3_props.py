import bpy, math, os
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(ROOT,'Assets','ReturnTide','Workshop','Models')
def material(n):
 m=bpy.data.materials.get(n) or bpy.data.materials.new(n);m.diffuse_color=(.5,.6,.6,1);return m
def box(n,p,s,m,bevel=.04):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=s;o.data.materials.append(material(m));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 mod=o.modifiers.new('Machined bevel','BEVEL');mod.width=bevel;mod.segments=3;bpy.ops.object.modifier_apply(modifier=mod.name);return o
def lathe(n,profile,m,sx=1,sy=1):
 v=[];f=[];N=40
 for r,z in profile:
  for j in range(N):a=j*math.tau/N;v.append((math.cos(a)*r*sx,math.sin(a)*r*sy,z))
 for i in range(len(profile)-1):
  for j in range(N):a=i*N+j;b=i*N+(j+1)%N;f.append((a,b,b+N,a+N))
 mesh=bpy.data.meshes.new(n);mesh.from_pydata(v,[],f);mesh.update();o=bpy.data.objects.new(n,mesh);bpy.context.collection.objects.link(o);o.data.materials.append(material(m));return o
def save(n):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource',n+'.blend'));bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,n+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
def reset():bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
reset()
lathe('Enamel well',[(0,.02),(.52,.02),(.61,.035),(.65,.09),(.67,.13),(.71,.13),(.72,.09),(.68,.01),(.60,-.015),(0,-.015)],'Pearl',1,1.43)
lathe('Rolled brass lip',[(.667,.12),(.685,.146),(.71,.146),(.725,.125)],'Copper',1,1.43)
for side in [-1,1]:box('Handle',(side*.76,0,.09),(.17,.28,.055),'Metal',.024)
save('Surgical_Tray')
reset()
box('Frame',(0,0,0),(6.80,3.6,.30),'Dark',.075)
box('Enamel top',(0,0,.18),(6.76,3.56,.10),'Table',.045)
for side in [-1,1]:box('Edge',(0,side*1.77,.19),(6.65,.035,.11),'Copper',.016)
for side in [-1,1]:box('Edge',(side*3.34,0,.19),(.035,3.45,.11),'Copper',.016)
for i in range(18):box('Drain slot',(-2.82,-.72+i*.085,.238),(.30,.022,.005),'Dark',.008)
for i in range(7):
 o=box('Enamel edge chip',(-2.8+i*.83,-1.68,.235),(.10+(i%3)*.025,.035,.004),'Metal',.008);o.rotation_euler[2]=i*.3
save('Surgical_Bench')
reset()
lathe('Bottle',[(0,0),(.15,0),(.18,.05),(.18,.50),(.14,.56),(.09,.58),(.09,.65),(.11,.65)],'Glass')
lathe('Cap',[(0,.65),(.12,.65),(.12,.72),(0,.72)],'Copper')
lathe('Label band',[(.184,.15),(.184,.28)],'Paper')
save('Specimen_Bottle')
print('V3_PROPS_READY')
