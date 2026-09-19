import os, math, random, wave, struct
root=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
dest=os.path.join(root,'Assets','ReturnTide','Workshop','Audio');os.makedirs(dest,exist_ok=True)
rate=22050
durations={'slice':.12,'peel':.32,'pluck':.24,'crack':.18,'ceramic':.22,'coin':.42,'core':.80,'deny':.18,'unlock':.34,'flutter':.30}
for kind,duration in durations.items():
 rng=random.Random(kind);data=bytearray();filtered=0
 for i in range(int(rate*duration)):
  t=i/rate;u=t/duration;noise=rng.uniform(-1,1);filtered=filtered*.85+noise*.15
  if kind=='slice':value=(noise-filtered)*.20*math.sin(math.pi*u)**1.3
  elif kind=='peel':value=filtered*.7*math.sin(math.pi*u)+math.sin(2*math.pi*(150*t-85*t*t))*.12*math.sin(math.pi*u)
  elif kind=='pluck':value=math.sin(2*math.pi*(360*t-360*t*t))*.34*math.exp(-14*t)+filtered*.22*math.exp(-20*t)
  elif kind=='crack':value=noise*.42*math.exp(-42*t)+math.sin(2*math.pi*145*t)*.24*math.exp(-24*t)+noise*.17*math.exp(-((t-.045)/.008)**2)
  elif kind=='ceramic':value=(math.sin(2*math.pi*1480*t)+.45*math.sin(2*math.pi*2281*t))*.24*math.exp(-22*t)
  elif kind in ['coin','core','unlock']:
   value=0;notes=[880,1108.7,1318.5] if kind!='core' else [523.25,659.25,783.99,1046.5]
   for j,f in enumerate(notes):
    d=t-j*(.065 if kind!='core' else .10)
    if d>=0:value+=(math.sin(2*math.pi*f*d)+.2*math.sin(2*math.pi*f*2*d))*.17*math.exp(-d*9)*min(1,d/.006)
  elif kind=='deny':value=math.sin(2*math.pi*(180*t-120*t*t))*.18*math.sin(math.pi*u)
  else:value=filtered*.7*math.sin(math.pi*u)*(.6+.4*math.sin(t*90))+math.sin(2*math.pi*(450*t-400*t*t))*.1*math.sin(math.pi*u)
  value*=min(1,i/70,(rate*duration-i)/120);data+=struct.pack('<h',int(max(-1,min(1,value))*.78*32767))
 with wave.open(os.path.join(dest,kind+'.wav'),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(data)
print('V4_FEEDBACK_AUDIO_READY')
