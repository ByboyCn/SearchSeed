"""用用户提供的真实蓝图文本测试解析链路各环节"""
import re, gzip, base64, io, struct, sys, urllib.parse
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

text = open('tools/real_bp.txt', encoding='utf-8').read().strip()
print("len:", len(text), "head:", text[:60])

m = re.search(r'"([A-Za-z0-9+/=]{100,})"', text)
print("base64 found:", bool(m), "len:", len(m.group(1)) if m else 0)

raw = base64.b64decode(m.group(1))
print("raw bytes:", len(raw), "first4:", raw[:4].hex())

gz = gzip.decompress(raw)
print("gunzip ok:", len(gz), "first16:", gz[:16].hex())

# 头部字段
parts = text.split(',')
print("segments:", len(parts))
for i, p in enumerate(parts[:14]):
    print(f"  [{i}]", urllib.parse.unquote(p)[:50])

# 按我们的二进制解析走一遍
r = io.BytesIO(gz)
ver = struct.unpack('<i', r.read(4))[0]
vals = struct.unpack('<6i', r.read(24))
print("version:", ver, "ints:", vals)
ac = r.read(1)[0]
print("areaCount:", ac)
r.read(ac * 14)
bc = struct.unpack('<i', r.read(4))[0]
print("buildingCount:", bc)
# 读第一个建筑看 tag
tag = struct.unpack('<i', r.read(4))[0]
print("first building tag:", tag)
