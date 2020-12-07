using System;
using System.Collections.Generic;

namespace AppBuilder.Shared
{
    [Serializable]
    public class UserProject
    {
        public string Name { get; set; }
        public List<ProjectFile> Files { get; set; }
    }
}