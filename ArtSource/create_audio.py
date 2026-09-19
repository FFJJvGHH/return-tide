import math, wave, struct, os, random
root=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
dest=os.path.join(root,'Assets','ReturnTide','Audio');os.makedirs(dest,exist_ok=True)
rate=22050;seconds=24;rng=random.Random(1937);data=bytearray()
for i in range(rate*seconds):
    t=i/rate
    swell=.65+.35*math.sin(2*math.pi*t/seconds)
    left=right=0
    for j,f in enumerate([130.8128,195.9977,261.6256,293.6648]):
        env=(.55+.45*math.sin(2*math.pi*t/seconds+j))*.018
        left+=math.sin(2*math.pi*round(f*seconds)/seconds*t)*env
        right+=math.sin(2*math.pi*round((f+.12)*seconds)/seconds*t)*env
    bubble=math.sin(2*math.pi*523*t)*max(0,math.sin(2*math.pi*t/6))**32*.008
    data.extend(struct.pack('<hh',int((left*swell+bubble)*32767),int((right*swell+bubble)*32767)))
with wave.open(os.path.join(dest,'Belly_Ambience.wav'),'wb') as w:
    w.setnchannels(2);w.setsampwidth(2);w.setframerate(rate);w.writeframes(data)
print('TIDE_AUDIO_READY')
