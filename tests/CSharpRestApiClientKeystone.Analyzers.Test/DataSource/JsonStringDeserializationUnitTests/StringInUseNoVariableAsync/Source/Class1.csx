using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.IO;

namespace ConsoleApplication1
{
    class TypeName
    {
        private MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
        public async Task<object> DeserializeAsClassWithStringAndReflectionAsync()
        {
            using var streamReader = new StreamReader(memoryStream, leaveOpen: true);
            var strRepresentation = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<object>(strRepresentation);
        }
    }
}
