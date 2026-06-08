using System.Security.Cryptography;
using System.Text;

namespace SistemaTraction.Infrastructure.Shopify;

internal static class GuidV5
{
    public static readonly Guid ShopifyOrderNamespace = new("a5d3e8b7-c6f1-4d2a-9e0f-1234567890ab");

    public static Guid Create(Guid namespaceId, string name)
    {
        var nsBytes = ToNetworkOrder(namespaceId.ToByteArray());
        var nameBytes = Encoding.UTF8.GetBytes(name);

        var input = new byte[nsBytes.Length + nameBytes.Length];
        nsBytes.CopyTo(input, 0);
        nameBytes.CopyTo(input, nsBytes.Length);

        var hash = SHA1.HashData(input);
        var result = hash[..16];

        result[6] = (byte)((result[6] & 0x0F) | 0x50); // version 5
        result[8] = (byte)((result[8] & 0x3F) | 0x80); // variant RFC 4122

        return new Guid(FromNetworkOrder(result));
    }

    private static byte[] ToNetworkOrder(byte[] g)
    {
        var b = (byte[])g.Clone();
        Array.Reverse(b, 0, 4);
        Array.Reverse(b, 4, 2);
        Array.Reverse(b, 6, 2);
        return b;
    }

    private static byte[] FromNetworkOrder(byte[] b)
    {
        var r = (byte[])b.Clone();
        Array.Reverse(r, 0, 4);
        Array.Reverse(r, 4, 2);
        Array.Reverse(r, 6, 2);
        return r;
    }
}
