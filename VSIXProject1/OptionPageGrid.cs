using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace SaveForSe
{
    public class OptionPageGrid : DialogPage
    {
        [Category("Options")]
        [DisplayName("Default start name")]
        [Description("public sealed class Program : --> MyGridProgram <--")]

        public string DefaultStartName { get; set; } = "MyGridProgram";

        [Category("Options")]
        [DisplayName("Enable save to buffer")]
        [Description("Enable save to buffer")]

        public bool IsSaveTobuffer { get; set; } = true;
    }
}