import re, os
SRC = r'C:/Users/Byboy/AppData/Local/Temp/dsp/cpp_source_code'

src = open(os.path.join(SRC,'NameGen.hpp'), encoding='utf-8').read()
arrays = re.findall(r'const char\* (\w+)\[\d+\]\s*\{(.*?)\};', src, re.S)
out = ['// Auto-generated from dsp_search_seed NameGen.hpp',
       'namespace SearchSeed.Core.Data;',
       '',
       'public static partial class NameTables',
       '{']
for name, body in arrays:
    items = re.findall(r'"((?:[^"\\]|\\.)*)"', body)
    out.append(f'    public static readonly string[] {name} =')
    out.append('    {')
    for i in range(0, len(items), 5):
        out.append('        ' + ''.join(f'"{it}", ' for it in items[i:i+5]).rstrip())
    out.append('    };')
    out.append('')
out.append('}')
open('SearchSeed.Core/Data/NameTables.cs','w',encoding='utf-8').write('\n'.join(out))
print('NameTables arrays:', [a[0] for a in arrays])

src = open(os.path.join(SRC,'LDB.hpp'), encoding='utf-8').read()
def fixf(x):
    x = x.strip()
    return x if x.endswith('f') else x + 'f'

def cvec(body, typ):
    items = re.findall(r'[-\d.eE]+', body)
    if typ == 'float':
        items = [i if i.endswith('f') else i + 'f' for i in items]
    return 'new %s[]{%s}' % (typ, ', '.join(items))
def vec2(body):
    nums = re.findall(r'[-\d.eE]+', body)
    return 'new Vector2(%sf, %sf)' % (nums[0], nums[1])
lines = []
for m in re.finditer(r'themes\.push_back\(\{(.*?)\}\);', src, re.S):
    body = m.group(1)
    parts, depth, cur = [], 0, ''
    for ch in body:
        if ch in '{(': depth += 1
        if ch in '})': depth -= 1
        if ch == ',' and depth == 0:
            parts.append(cur.strip()); cur = ''
        else:
            cur += ch
    parts.append(cur.strip())
    parts = [x.strip('"') for x in parts]
    (typeid, name, id_, disp, ptype, temp, dist, algos, modx, mody, veinspot,
     veincount, veinopacity, rareveins, raresettings, gasitems, gasspeeds,
     useheight, wind, ionh, waterh, wateritem) = parts
    s = []
    s.append(f'TypeId = {typeid}, Name = "{name}", ID = {id_}, DisplayName = "{disp}",')
    s.append(f'PlanetType = EPlanetType.{ptype.split("::")[-1]}, Temperature = {fixf(temp)},')
    s.append(f'Distribute = EThemeDistribute.{dist.split("::")[-1]},')
    s.append(f'Algos = {cvec(algos,"int")}, ModX = {vec2(modx)}, ModY = {vec2(mody)},')
    s.append(f'VeinSpot = {cvec(veinspot,"int")}, VeinCount = {cvec(veincount,"float")}, VeinOpacity = {cvec(veinopacity,"float")},')
    s.append(f'RareVeins = {cvec(rareveins,"int")}, RareSettings = {cvec(raresettings,"float")},')
    s.append(f'GasItems = {cvec(gasitems,"int")}, GasSpeeds = {cvec(gasspeeds,"float")},')
    s.append(f'UseHeightForBuild = {useheight.lower() == 'true'}, Wind = {fixf(wind)}, IonHeight = {fixf(ionh)}, WaterHeight = {fixf(waterh)}, WaterItemId = {wateritem}')
    lines.append('new ThemeProto\n{\n    ' + ',\n    '.join(s) + '\n}')
out = ['// Auto-generated from dsp_search_seed LDB.hpp',
       'using System.Numerics;',
       '',
       'namespace SearchSeed.Core.Data;',
       '',
       'public static class Ldb',
       '{',
       '    public static readonly ThemeProto[] Themes = new ThemeProto[]',
       '    {',
       '        ' + ',\n        '.join(lines).replace('\n', '\n        ').replace('        \n','\n'),
       '    };',
       '',
       '    public static ThemeProto Select(int index) => Themes[index - 1];',
       '}']
open('SearchSeed.Core/Data/Ldb.cs','w',encoding='utf-8').write('\n'.join(out))
print('themes:', len(lines))
