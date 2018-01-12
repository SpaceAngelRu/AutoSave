using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace SaveForSe
{
    public class OptionPageGrid : DialogPage
    {
        [Category("Options")]
        [DisplayName("Enable save to buffer")]
        [Description("Enable saving to buffer")]

        public bool IsSaveTobuffer { get; set; } = true;

        [Category("Options")]
        [DisplayName("Enable all errors")]
        [Description("Enable displaying all errors")]

        public bool IsShowError { get; set; } = true;

        [Category("Options")]
        [DisplayName("Label exclude")]
        [Description("Tag deactivate the file from the project")]

        public string LabelExclude { get; set; } = "Exclude from project";

        [Category("Options")]
        [DisplayName("Comment exclude ")]
        [Description("Exclude comments from processing")]

        public string CommentExclude { get; set; } = "//-";

        [Category("Options")]
        [DisplayName("Include comments")]
        [Description("Include comments in the code")]

        public bool IsIncludeComment { get; set; } = false;

        [Category("Options")]
        [DisplayName("Include regions")]
        [Description("Include regions in the code")]

        public bool IsIncludeRegion { get; set; } = false;

        [Category("Options")]
        [DisplayName("Include empty line")]
        [Description("Include empty line in the code")]

        public bool IsIncludeEmptyLine { get; set; } = false;

        [Category("Options")]
        [DisplayName("Path to ingameScripts\\local")]
        [Description("Path to c:\\users\\<user name>\\appData\\roaming\\spaceEngineers\\ingameScripts\\local\\")]

        public string PathToIngameScriptsLocal { get; set; } = "";

        [Category("Options")]
        [DisplayName("Copy to clipboard")]
        [Description("Copy to clipboard")]

        public bool IsCopyToBuffer { get; set; } = true;
    }
}