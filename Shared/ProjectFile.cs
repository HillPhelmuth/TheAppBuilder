using System;

namespace AppBuilder.Shared
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
    public enum ProjectType
    {
        [EnumString("Blazor")]
        Blazor,
        [EnumString("Console")]
        Console
    }
    public class EnumString : Attribute
    {
        public EnumString(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }
    public static class ValueExtensions
    {
        public static string AsString(this Enum value)
        {
            string output = null;
            var type = value.GetType();
            var fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof(EnumString), false) as EnumString[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }
    }
}
