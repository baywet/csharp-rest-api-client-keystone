using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Net.Http;

namespace ConsoleApplication1
{
    class TypeName
    {
        public async Task<object> DeserializeAsClassWithStringAndReflectionAsync()
        {
            using var responseMessage = new HttpResponseMessage();
            using var memoryStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<object>(memoryStream);
            // the stream reader and strRepresentation lines remain but other analyzers will catch unused variables
        }
    }
}
