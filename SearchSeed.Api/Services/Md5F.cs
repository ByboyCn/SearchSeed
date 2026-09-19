namespace SearchSeed.Api.Services;

// 游戏 MD5F（官方 Assembly-CSharp 反编译移植）：
// 标准 MD5 轮函数 + 魔改 IV（B=0xEFDCAB89、D=0x10325746，各两字节调换），
// 输出标准 MD5 小端 hex 大写；输入按 UTF-8 字节
public static class Md5F
{
    static readonly uint[] K =
    {
        0xd76aa478,0xe8d7b756,0x242070db,0xc1bdceee,0xf57c0faf,0x4787c62a,0xa8304623,0xfd469501,
        0x698098d8,0x8b44f7af,0xffff5bb1,0x895cd7be,0x6b9f1122,0xfd987193,0xa679438e,0x39b40821,
        0xf61e2562,0xc040b340,0x265e5a51,0xc9b6c7aa,0xd62f105d,0x02443453,0xd8a1e681,0xe7d3fbc8,
        0x21f1cde6,0xc33707d6,0xf4d50d87,0x475a14ed,0xa9e3e905,0xfcefa3f8,0x676f02d9,0x8d2a4c8a,
        0xfffa3942,0x8771f681,0x6d9d6122,0xfde5380c,0xa4beea44,0x4bdecfa9,0xf6bb4b60,0xbebfbc70,
        0x289b7ec6,0xeaa127fa,0xd4ef3085,0x04881d05,0xd9d4d039,0xe6db99e5,0x1fa27cf8,0xc4ac5665,
        0xf4292244,0x432aff97,0xab9423a7,0xfc93a039,0x655b59c3,0x8f0ccc92,0xffeff47d,0x85845dd1,
        0x6fa87e4f,0xfe2ce6e0,0xa3014314,0x4e0811a1,0xf7537e82,0xbd3af235,0x2ad7d2bb,0xeb86d391,
    };
    static readonly int[] S = { 7,12,17,22,7,12,17,22,7,12,17,22,7,12,17,22, 5,9,14,20,5,9,14,20,5,9,14,20,5,9,14,20, 4,11,16,23,4,11,16,23,4,11,16,23,4,11,16,23, 6,10,15,21,6,10,15,21,6,10,15,21,6,10,15,21 };

    public static string Compute(string message) => Compute(System.Text.Encoding.UTF8.GetBytes(message));

    public static string Compute(byte[] input)
    {
        uint a0 = 0x67452301, b0 = 0xefdcab89, c0 = 0x98badcfe, d0 = 0x10325746;

        void Block(ref uint a, ref uint b, ref uint c, ref uint d, byte[] buf)
        {
            uint aa = a, bb = b, cc = c, dd = d;
            for (int i = 0; i < 64; i++)
            {
                uint f; int g;
                if (i < 16) { f = (bb & cc) | (~bb & dd); g = i; }
                else if (i < 32) { f = (dd & bb) | (~dd & cc); g = (5 * i + 1) % 16; }
                else if (i < 48) { f = bb ^ cc ^ dd; g = (3 * i + 5) % 16; }
                else { f = cc ^ (bb | ~dd); g = (7 * i) % 16; }
                uint m = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(g * 4));
                f += aa + K[i] + m;
                aa = dd; dd = cc; cc = bb;
                bb += f << S[i] | f >> (32 - S[i]);
            }
            a += aa; b += bb; c += cc; d += dd;
        }

        int totalLen = input.Length;
        int padLen = ((totalLen + 8 + 63) / 64) * 64;
        var padded = new byte[padLen];
        Array.Copy(input, padded, totalLen);
        padded[totalLen] = 0x80;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(padded.AsSpan(padLen - 8), (ulong)totalLen * 8);

        uint a = a0, b = b0, c = c0, d = d0;
        var buf = new byte[64];
        for (int off = 0; off < padLen; off += 64)
        {
            Array.Copy(padded, off, buf, 0, 64);
            Block(ref a, ref b, ref c, ref d, buf);
        }
        // 官方 ArrayToHexString：按 uint 小端字节序逐字节输出（同标准 MD5 hex）
        var sb = new System.Text.StringBuilder(32);
        foreach (uint w in new[] { a, b, c, d })
        {
            sb.Append(((byte)(w & 0xFF)).ToString("x2"));
            sb.Append(((byte)((w >> 8) & 0xFF)).ToString("x2"));
            sb.Append(((byte)((w >> 16) & 0xFF)).ToString("x2"));
            sb.Append(((byte)((w >> 24) & 0xFF)).ToString("x2"));
        }
        return sb.ToString().ToUpperInvariant();
    }
}
