import bpy, math, os
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(ROOT,'Assets','ReturnTide','Models')
os.makedirs(OUT,exist_ok=True)
palette={'Ivory':'EAE4CF','Ink':'203D49','Teal':'3DAFA4','Coral':'DF837D','Gold':'EEBC69','Skin':'F1C4AD','Hair':'DDE4E1','Glow':'92F1CA','Dark':'283E54','Leather':'936B58'}
def mat(n):
    m=bpy.data.materials.get(n) or bpy.data.materials.new(n)
    h=palette[n]; c=tuple(int(h[i:i+2],16)/255 for i in (0,2,4))
    m.diffuse_color=(*c,1); return m
def uv(n,p,s,m,seg=12,rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=p)
    o=bpy.context.object; o.name=n; o.scale=s; o.data.materials.append(mat(m)); return o
def ico(n,p,s,m,sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=p)
    o=bpy.context.object;o.name=n;o.scale=s;o.data.materials.append(mat(m));return o
def cone(n,p,r1,r2,d,m,vertices=10):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices,radius1=r1,radius2=r2,depth=d,location=p)
    o=bpy.context.object;o.name=n;o.data.materials.append(mat(m));return o
def rod(n,a,b,r,m):
    a,b=Vector(a),Vector(b);o=cone(n,(a+b)/2,r,r,(a-b).length,m,8)
    o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def torus(n,p,major,minor,m,rot=(math.pi/2,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_segments=16,minor_segments=6,location=p,major_radius=major,minor_radius=minor,rotation=rot)
    o=bpy.context.object;o.name=n;o.data.materials.append(mat(m));return o
def reset():
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def save(name):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource',name+'.blend'))
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
reset()
# Hand-authored silhouette: oversized head, bobbed hair, goggles, flared lab coat.
for x in [-.19,.19]:
    rod('Leg_L' if x<0 else 'Leg_R',(x,0,.18),(x,0,.69),.105,'Ink')
    uv('Boot',(x,-.075,.15),(.145,.23,.15),'Leather')
cone('Coat_Skirt',(0,0,.88),.43,.26,.62,'Ivory')
uv('Coat_Bodice',(0,0,1.24),(.32,.23,.37),'Ivory')
cone('Collar',(0,0,1.47),.26,.2,.13,'Teal')
uv('Head',(0,-.025,1.84),(.40,.34,.43),'Skin',16,12)
uv('Hair_Cap',(0,.045,1.98),(.427,.36,.38),'Hair',16,8)
for x in [-.33,.33]:
    uv('Hair_Side',(x,.04,1.78),(.13,.32,.34),'Hair')
    ico('Hair_Tip',(x,.025,1.5),(.12,.19,.2),'Hair')
for x in [-.25,-.12,.03,.19]:
    ico('Fringe',(x,-.30,2.04),(.15,.09,.19),'Hair')
for x in [-.15,.15]:
    uv('Eye',(x,-.337,1.84),(.045,.025,.071),'Ink')
    uv('Eye_Shine',(x-.012,-.359,1.864),(.012,.009,.018),'Ivory',8,6)
    uv('Cheek',(x*1.45,-.30,1.73),(.058,.012,.025),'Coral')
    torus('Goggles_Rim',(x,-.28,2.17),.125,.026,'Leather')
    uv('Goggles_Glass',(x,-.288,2.17),(.10,.025,.10),'Teal')
rod('Goggles_Bridge',(-.05,-.3,2.17),(.05,-.3,2.17),.022,'Gold')
rod('Mouth',(-.035,-.352,1.69),(.035,-.352,1.69),.012,'Coral')
for x in [-1,1]:
    rod('Sleeve_L' if x<0 else 'Sleeve_R',(x*.27,0,1.39),(x*.44,-.035,1.02),.13,'Ivory')
    uv('Glove',(x*.45,-.055,.99),(.115,.12,.13),'Teal')
for z in [.94,1.10,1.26]:uv('Coat_Button',(0,-.24,z),(.035,.024,.035),'Gold',8,6)
uv('Backpack',(0,.30,1.16),(.26,.14,.31),'Teal')
for x in [-.21,.21]:
    rod('Sample_Vial',(x,.32,1.04),(x,.32,1.36),.055,'Glow')
rod('Scalpel_Grip',(.47,-.06,.83),(.47,-.06,1.12),.035,'Leather')
o=cone('Scalpel_Blade',(.47,-.06,.63),.008,.07,.42,'Glow',4)
save('Mio_Researcher')
reset()
for x in [-.24,.24]:
    uv('Boot',(x,-.1,.16),(.2,.3,.16),'Leather')
    rod('Leg',(x,0,.2),(x,0,.65),.13,'Ink')
uv('Body',(0,0,1.0),(.48,.32,.56),'Teal')
cone('Apron',(0,-.10,.88),.46,.32,.7,'Ivory')
uv('Head',(0,0,1.67),(.37,.31,.38),'Skin')
uv('Beard',(0,-.24,1.5),(.3,.17,.19),'Ivory')
cone('Fisher_Hat',(0,0,1.98),.58,.22,.25,'Gold')
for x in [-.15,.15]:uv('Eye',(x,-.30,1.70),(.035,.018,.04),'Ink')
for x in [-1,1]:
    rod('Arm',(x*.4,0,1.28),(x*.58,-.15,.94),.15,'Teal')
    uv('Hand',(x*.58,-.15,.94),(.13,.13,.13),'Skin')
uv('Apron_Badge',(0,-.36,1.0),(.15,.025,.1),'Coral')
save('Fishmonger')
reset()
uv('Fish_Body',(0,0,0),(1.15,2.2,1.1),'Teal',16,10)
uv('Fish_Belly',(0,-.4,-.36),(1.05,1.65,.7),'Ivory',12,8)
torus('Mouth_Rim',(0,-1.95,0),.73,.18,'Coral')
uv('Mouth_Inside',(0,-2.03,0),(.69,.06,.69),'Ink')
for x in [-1,1]:
    uv('Eye_Gold',(x*.79,-1.23,.49),(.30,.22,.31),'Gold')
    uv('Eye_Pupil',(x*.84,-1.42,.49),(.11,.07,.17),'Ink')
    o=ico('Side_Fin',(x*1.1,.3,-.15),(.9,.9,.12),'Coral');o.rotation_euler[1]=x*.4
ico('Tail',(0,2.35,.1),(1.25,.7,.70),'Coral')
ico('Dorsal_Fin',(0,.55,1.05),(.16,1.3,.65),'Gold')
for i in range(9):
    y=-.8+(i%3)*.7;x=(-1 if i%2 else 1)*(.75+(.15 if i%3 else 0))
    ico('Radiation_Crystal',(x,y,.75),(.16,.18,.35),'Glow')
save('Radiant_Fish')
print('RETURN_TIDE_MODELS_READY')

