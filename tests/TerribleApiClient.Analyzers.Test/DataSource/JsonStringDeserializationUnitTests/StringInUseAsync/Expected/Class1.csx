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
        public async Task<object> DeserializeAsClassWithStringAndReflectionAsync()
        {
            using var MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            return JsonSerializer.Deserialize<object>(memoryStream);
        }
    }
}
