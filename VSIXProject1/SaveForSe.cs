#region

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#endregion

namespace SaveForSe
{
    internal class ResClassData
    {
        #region Construct

        public ResClassData()
        {
            Code = new List<string>();
        }

        #endregion

        #region Public

        public string Name;
        public int StartIdx;
        public int EndIdx;
        public readonly List<string> Code;

        #endregion
    }

    internal sealed class SaveForSe
    {
        #region Const

        private const string PROGRAM_LIT = "Program";
        private const string CLASS_LIT = "class";
        private const string FILE_EXT = "*.cs";
        private const string COMMENT_LIT = "//";
        private const string REGION_LIT = "region";
        private const string END_REGION_LIT = "endregion";

        public const int COMMAND_ID = 0x0100;

        #endregion

        #region Static

        private static DTE2 _dte;
        private static DocumentEvents _documentEvents;

        public static readonly Guid CommandSet = new Guid("12b1f285-f51a-4c47-ac6a-d8a46c3671d4");

        #endregion

        #region Private fields

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        private bool IsSaveTobuffer
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid) _package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsSaveTobuffer;
            }
        }

        private bool IsShowError
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid) _package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsShowError;
            }
        }

        private string LabelExclude
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid) _package.GetDialogPage(typeof(OptionPageGrid));
                return page.LabelExclude;
            }
        }

        private bool IsIncludeComment
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)_package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsIncludeComment;
            }
        }

        private bool IsIncludeRegion
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)_package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsIncludeRegion;
            }
        }

        private bool IsIncludeEmptyLine
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)_package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsIncludeEmptyLine;
            }
        }

        private string CommentExclude
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)_package.GetDialogPage(typeof(OptionPageGrid));
                return page.CommentExclude;
            }
        }

        private string PathToIngameScriptsLocal
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)_package.GetDialogPage(typeof(OptionPageGrid));
                return page.PathToIngameScriptsLocal;
            }
        }

        private bool IsCopyToBuffer
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)_package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsCopyToBuffer;
            }
        }

        private readonly string[] _exclude = {"obj\\Debug", "obj\\Debug", "Properties"};

        private readonly Package _package;

        #endregion

        #region Construct

        private SaveForSe(Package package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandId = new CommandID(CommandSet, COMMAND_ID);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }

            _documentEvents.DocumentSaved += DocumentEventsOnDocumentSaved;
        }

        #endregion

        #region Public

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            _dte = (package as IServiceProvider).GetService(typeof(SDTE)) as DTE2;
            if (_dte == null)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "Failed to get DTE service.",
                    "Error",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            _documentEvents = _dte.Events.DocumentEvents;

            Instance = new SaveForSe(package);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SaveForSe Instance { get; private set; }

        #endregion

        #region Private methods

        private StringBuilder GetCode(string dir, string fileExt, string[] exclude)
        {
            List<string> allfiles = Directory.GetFiles(dir, fileExt, SearchOption.AllDirectories).ToList();

            Exclude(allfiles, exclude);

            Dictionary<string, List<string>> filesLines = new Dictionary<string, List<string>>();
            foreach (string patoToFile in allfiles)
            {
                string[] lines = File.ReadAllLines(patoToFile);
                filesLines.Add(patoToFile, lines.ToList());
            }

            Dictionary<string, ResClassData> clases = new Dictionary<string, ResClassData>();
            ResClassData programClassData = null;

            foreach (KeyValuePair<string, List<string>> pair in filesLines)
            {
                if (GetLineNum(pair.Value, LabelExclude) >= 0)
                {
                    continue;
                }

                ResClassData res = null;
                int idx = 0;
                do
                {
                    res = GetClass(pair.Value, idx);
                    if (res != null)
                    {
                        if (res.Name == PROGRAM_LIT)
                        {
                            programClassData = res;
                        }

                        if (!clases.ContainsKey(res.Name))
                            clases.Add(res.Name, res);
                        else
                            throw new Exception($"There are two classes with the same name - \"{res.Name}\"\nline: {res.StartIdx}\n{pair.Key}");

                        idx = res.EndIdx;
                    }
                } while (res != null);
            }

            if (programClassData == null)
            {
                return null;
            }

            StringBuilder resCode = new StringBuilder();

            foreach (KeyValuePair<string, ResClassData> pair in clases)
            {
                if (pair.Key != PROGRAM_LIT)
                {
                    var classData = pair.Value;
                    int idx = GetLineNum(programClassData.Code, classData.Name);
                    if (idx > 0)
                    {
                        int lenTab = GetLenTab(classData.Code[0]);
                        foreach (var line in classData.Code)
                        {
                            resCode.AppendLine(line.Length > lenTab ? line.Remove(0, lenTab) : line);
                        }

                        resCode.AppendLine();
                    }
                }
            }

            int startBraces = GetStartBraces(0, programClassData.Code);
            for (int i = startBraces; i < programClassData.Code.Count; i++)
            {
                if (programClassData.Code[i].Length > 8)
                {
                    startBraces = i;
                    break;
                }
            }

            int lenTabPc = GetLenTab(programClassData.Code[startBraces]);
            for (int i = 2; i < programClassData.Code.Count - 1; i++)
            {
                resCode.AppendLine(programClassData.Code[i].Length > lenTabPc ? programClassData.Code[i].Remove(0, lenTabPc) : programClassData.Code[i]);
            }

            return resCode;
        }

        private static int GetLenTab(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ' && str[i] != '\t')
                {
                    return i;
                }
            }
            return 0;
        }

        private ResClassData GetClass(List<string> lines, int startIndex)
        {
            if (startIndex < 0 || startIndex > 0 && startIndex > lines.Count)
            {
                return null;
            }

            ResClassData resClassData = new ResClassData();
            int end = lines.Count;

            for (int i = startIndex; i < end; i++)
            {
                string line = lines[i];

                if (line.Contains(CLASS_LIT))
                {
                    var removeSpacesStr = Regex.Replace(line, @"\s{2,}|:", " ");
                    var nameClassMatch = Regex.Match(removeSpacesStr, $"(?<={CLASS_LIT} )\\w+");

                    if (nameClassMatch.Success)
                    {
                        resClassData.Name = nameClassMatch.Value;
                        resClassData.StartIdx = i;
                        resClassData.EndIdx = GetEndBraces(GetStartBraces(i, lines), lines);

                        for (int k = resClassData.StartIdx; k <= resClassData.EndIdx; k++)
                        {
                            string codeLine = lines[k];
                            if (IsIncludeEmptyLine || !string.IsNullOrEmpty(codeLine))
                            {
                                if ((IsIncludeComment || IsStartSubstring(codeLine, CommentExclude) || !IsStartSubstring(codeLine, COMMENT_LIT)) &&
                                    (IsIncludeRegion || !IsStartSubstring(codeLine, REGION_LIT)) &&
                                    (IsIncludeRegion || !IsStartSubstring(codeLine, END_REGION_LIT)))
                                {
                                    resClassData.Code.Add(codeLine);
                                }
                            }
                        }

                        return resClassData;
                    }
                }
            }

            return null;
        }

        private static bool IsStartSubstring(string str, string substr)
        {
            if (str.Length > 0 && substr.Length > 0)
                return str.IndexOf(substr, 0, StringComparison.Ordinal) >= 0;

            return false;
        }

        private static void Exclude(List<string> allfiles, string[] exclude)
        {
            for (int i = 0; i < allfiles.Count;)
            {
                var item = allfiles[i];

                if (exclude.Any(t => item.Contains(t)))
                {
                    allfiles.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private static int GetStartBraces(int startIndex, List<string> lines)
        {
            int end = lines.Count;

            if (startIndex < 0 || startIndex > 0 && startIndex > lines.Count)
            {
                return -1;
            }

            for (int i = startIndex; i < end; i++)
            {
                if (lines[i].Contains("{"))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int GetEndBraces(int startIndex, List<string> lines)
        {
            if (startIndex < 0)
            {
                return -1;
            }

            var idx = startIndex;
            var end = lines.Count;
            var countBrace = 0;

            for (; idx < end; idx++)
            {
                var line = lines[idx];

                switch (GetBrace(line))
                {
                    case 1:
                        countBrace++;
                        break;
                    case -1:
                        countBrace--;

                        if (countBrace <= 0)
                        {
                            return idx;
                        }

                        break;
                }
            }

            return -1;
        }

        private static int GetBrace(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return 0;
            }

            int countBrace = 0;
            foreach (char t in line)
            {
                if (t == '{')
                {
                    countBrace++;
                }
                else if (t == '}')
                {
                    countBrace--;
                }
            }

            if (countBrace > 0)
            {
                return 1;
            }

            if (countBrace < 0)
            {
                return -1;
            }

            return 0;
        }

        private static int GetLineNum(IReadOnlyList<string> filelines, string str, int startIndex = 0)
        {
            if (filelines.Count == 0)
            {
                return -1;
            }

            if (startIndex > 0 && startIndex > filelines.Count)
            {
                return -1;
            }

            for (int i = startIndex; i < filelines.Count; i++)
            {
                if (filelines[i].Contains(str))
                {
                    return i;
                }
            }

            return -1;
        }

        private void DocumentEventsOnDocumentSaved(Document document)
        {
            SaveDocument();
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void SaveDocument()
        {
            if (!IsSaveTobuffer)
            {
                return;
            }

            try
            {
                Project activeProject = null;
                Array activeSolutionProjects = _dte.ActiveSolutionProjects as Array;
                if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                {
                    activeProject = activeSolutionProjects.GetValue(0) as Project;
                }

                if (activeProject != null)
                {
                    string str = Path.GetDirectoryName(activeProject.FileName);
                    StringBuilder rescode = GetCode(str, FILE_EXT, _exclude);

                    if (rescode != null)
                    {
                        string resStr = rescode.ToString();

                        if (IsCopyToBuffer)
                            Clipboard.SetText(resStr);

                        string pathToIngameRoot = PathToIngameScriptsLocal;
                        if (!string.IsNullOrEmpty(pathToIngameRoot))
                        {
                            string namePrj = Path.GetFileNameWithoutExtension(activeProject.FileName);

                            if (!string.IsNullOrEmpty(namePrj) && Directory.Exists(pathToIngameRoot))
                            {
                                string pathToIngame = Path.Combine(pathToIngameRoot, namePrj);

                                if (!Directory.Exists(pathToIngame))
                                {
                                    Directory.CreateDirectory(pathToIngame);
                                }

                                string fileScript = Path.Combine(pathToIngame, "Script.cs");
                                File.WriteAllText(fileScript, resStr);

                                string fileThumb = Path.Combine(pathToIngame, "thumb.png");
                                if (!File.Exists(fileThumb))
                                {
                                    Image bmp = new Bitmap(Resource.thumb);
                                    bmp.Save(fileThumb);
                                }
                               
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (IsShowError)
                {
                    ShowMsg(e.Message + "\n\n" + e.StackTrace, "Error", OLEMSGICON.OLEMSGICON_CRITICAL);
                }
            }
        }

        private void ShowMsg(string message, string title = "", OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO)
        {
            Clipboard.SetText(message);

            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                message,
                title,
                icon,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        #endregion
    }
}