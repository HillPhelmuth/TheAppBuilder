using System.Collections.Generic;

namespace Shared
{
    public class CodeSample
    {
        public CodeSample()
        {
        }

        public CodeSample(string key, string value, string description, string toolTip)
        {
            Key = key;
            Value = value;
            Description = description;
            ToolTip = toolTip;
        }

        public CodeSample(string key, string value, string description)
        {
            Key = key;
            Value = value;
            Description = description;
        }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string ToolTip { get; set; }
    }

    public class CodeSamples
    {
        public SampleSection Section { get; set; }
        public List<CodeSample> Samples { get; set; }
    }

    public enum SampleSection
    {
        Linq, Collection, String, ConditionalsLoops, Extension
    }
}
