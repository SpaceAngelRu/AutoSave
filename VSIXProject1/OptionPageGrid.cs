using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace SaveForSe
{
    public class OptionPageGrid : DialogPage
    {
        [Category("Options")]
        [DisplayName("Label start script")]
        [Description("The contents of the comment line for marking the start line for copying to the clipboard - //Start script")]

        public string LabelStartName { get; set; } = "Start script";

        [Category("Options")]
        [DisplayName("Label end script")]
        [Description("The contents of the comment line for marking the end line for copying to the clipboard - //End script")]

        public string LabelEndName { get; set; } = "End script";

        [Category("Options")]
        [DisplayName("Enable save to buffer")]
        [Description("Enable save to buffer")]

        public bool IsSaveTobuffer { get; set; } = true;
    }
}