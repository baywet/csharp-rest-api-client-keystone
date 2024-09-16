using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

namespace ConsoleApplication1
{
    class TypeName
    {
        public async Task<object> DeserializeAsClassWithStringAndReflectionAsync()
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            using var streamReader = new StreamReader(memoryStream, leaveOpen: true);
            var strRepresentation = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<object>(strRepresentation);
        }
    }
}
