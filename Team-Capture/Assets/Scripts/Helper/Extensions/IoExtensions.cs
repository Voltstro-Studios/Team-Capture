using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Extensions
{
    public static class IoExtensions
    {
        public static Task WriteStringAsync(this FileStream fs, string msg)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            return fs.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}