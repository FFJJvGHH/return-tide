"""Encode the unmodified Unity recording frames; no generated concept frames."""
import bpy, os, math, bisect, sys
folder=os.path.abspath(sys.argv[sys.argv.index('--')+1])
frames=os.path.join(folder,'Frames')
with open(os.path.join(folder,'frame-times.txt')) as f:times=[float(x) for x in f if x.strip()]
assert times, 'No captured Unity frames'
fps=12;total=math.ceil(times[-1]*fps)
chosen=[]
for i in range(total):
 index=max(0,min(len(times)-1,bisect.bisect_right(times,i/fps)-1))
 chosen.append('frame_%04d.png'%index)
scene=bpy.context.scene
editor=scene.sequence_editor_create()
strip=editor.strips.new_image('Unity footage',filepath=os.path.join(frames,chosen[0]),channel=1,frame_start=1)
for name in chosen[1:]:strip.elements.append(name)
strip.frame_final_duration=total
scene.frame_start=1;scene.frame_end=total
scene.render.resolution_x=960;scene.render.resolution_y=540;scene.render.resolution_percentage=100;scene.render.fps=fps
scene.render.use_sequencer=True;scene.render.image_settings.file_format='FFMPEG';scene.render.ffmpeg.format='MPEG4';scene.render.ffmpeg.codec='H264';scene.render.ffmpeg.constant_rate_factor='HIGH';scene.render.ffmpeg.audio_codec='NONE'
scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
scene.render.filepath=os.path.join(folder,'Feedback-preview.mp4')
bpy.ops.render.render(animation=True)
print('FEEDBACK_VIDEO_READY')
