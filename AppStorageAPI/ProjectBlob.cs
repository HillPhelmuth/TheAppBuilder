using System;
using System.Collections.Generic;
using System.Text;

namespace AppStorageAPI
{
    public class ProjectBlob
    {
        public string Name { get; set; }
        public byte[] CompressedFiles { get; set; }
    }
}
