using System;

namespace Shared
{
    public class ProjectFile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public FileType FileType { get; set; }
        public string Content { get; set; }
    }

    public enum FileType
    {
        [EnumString("cs")]
        Class,
        [EnumString("razor")]
        Razor
    }

    public class EnumString : Attribute
    {
        public EnumString(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }
}
