# Generate selection metadata, not replacement food artwork. Requires Pillow.
import json,base64
from pathlib import Path
from PIL import Image
repo=Path(__file__).resolve().parents[1]
foods=json.loads((repo/'Assets/Asado/Resources/Definitions/FoodCatalog.json').read_text())['Foods']
w,h=128,64
masks=[]
for food in foods:
    im=Image.open(repo/f"Assets/Asado/Resources/Art/Foods/States/{food['Id']}.png").convert('RGBA')
    c=food['SpriteCrop']; cell=im.height/6
    alpha=im.getchannel('A').crop((c['XMin']*im.width,c['YMin']*cell,c['XMax']*im.width,c['YMax']*cell)).resize((w,h),Image.Resampling.BILINEAR)
    bits=bytearray(w*h//8)
    for y in range(h):
        for x in range(w):
            if alpha.getpixel((x,h-1-y))>=48:
                index=y*w+x;bits[index//8]|=1<<(index%8)
    masks.append({'FoodId':food['Id'],'Bits':base64.b64encode(bits).decode()})
(repo/'Assets/Asado/Resources/Definitions/FoodHitMasks.json').write_text(json.dumps({'Width':w,'Height':h,'Masks':masks},indent=2)+'\n')
print(len(masks),'food masks;',len(masks)*w*h//8,'bytes decoded; non-readable atlas preserved')
