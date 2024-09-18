using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace ConsoleApplication1;

class TypeName
{
    public async Task<object> MemoryStreamCopyToAsync()
    {
        const string StringStream = "String Format Test for MemoryStream";
        var memoryStreamOrigin = new MemoryStream(Encoding.UTF8.GetBytes(StringStream));
        memoryStreamOrigin!.Seek(0, SeekOrigin.Begin);
        using var memoryStreamDestination = new MemoryStream();
        await memoryStreamOrigin!.CopyToAsync(memoryStreamDestination).ConfigureAwait(false);
        return memoryStreamDestination;
    }
}
