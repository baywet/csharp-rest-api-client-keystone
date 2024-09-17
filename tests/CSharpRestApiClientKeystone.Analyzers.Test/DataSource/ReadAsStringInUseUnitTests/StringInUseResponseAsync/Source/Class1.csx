using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace ConsoleApplication1
{
    class TypeName
    {
        public async Task<object> DeserializeAsClassWithStringAndReflectionAsync()
        {
            using var responseMessage = new HttpResponseMessage();
            string strRepresentation = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<object>(strRepresentation);
            // the stream reader and strRepresentation lines remain but other analyzers will catch unused variables
        }
    }
}
