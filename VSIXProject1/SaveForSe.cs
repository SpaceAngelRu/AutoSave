#region

using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#endregion

namespace SaveForSe
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SaveForSe
    {
        #region Const

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        #endregion

        #region Static

        private static DTE2 _dte;
        private static DocumentEvents _documentEvents = null;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("12b1f285-f51a-4c47-ac6a-d8a46c3671d4");

        #endregion

        #region Private fields

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return this._package; }
        }

        private string LabelStartName
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid) _package.GetDialogPage(typeof(OptionPageGrid));
                return page.LabelStartName;
            }
        }

        private string LabelEndName
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid) _package.GetDialogPage(typeof(OptionPageGrid));
                return page.LabelEndName;
            }
        }

        private bool IsSaveTobuffer
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid) _package.GetDialogPage(typeof(OptionPageGrid));
                return page.IsSaveTobuffer;
            }
        }

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        #endregion

        #region Construct

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveForSe"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private SaveForSe(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this._package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
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

        private static void ClipboartUpdate(string[] lines, string startName, string endName)
        {
            var start = GetLineNum(lines, startName);
            var end = GetLineNum(lines, endName);

            StringBuilder sb = new StringBuilder();

            for (int i = start + 1; i < end; i++)
            {
                sb.AppendLine(lines[i]);
            }

            Clipboard.SetText(sb.ToString());

            //            var start = GetStart(lines, startName);
            //            var end = GetEnd(start, lines);
            //
            //            if (lines[start].Contains("{") && lines[end].Contains("}"))
            //            {
            //                StringBuilder sb = new StringBuilder();
            //
            //                for (int i = start + 1; i < end; i++)
            //                {
            //                    sb.AppendLine(lines[i]);
            //                }
            //
            //                Clipboard.SetText(sb.ToString());
            //            }
        }

        private static int GetStart(string[] lines, string strName)
        {
            var idx = GetLineNum(lines, strName);
            var end = lines.Length;

            if (idx < 0)
            {
                return -1;
            }

            for (; idx < end; idx++)
            {
                if (lines[idx].Contains("{"))
                {
                    return idx;
                }
            }

            return -1;
        }

        private static int GetEnd(int start, string[] lines)
        {
            if (start < 0)
            {
                return -1;
            }

            var idx = start;
            var end = lines.Length;
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

        private static int GetLineNum(string[] filelines, string str)
        {
            for (int i = 0; i < filelines.Length; i++)
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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void SaveDocument()
        {
            Document ac = _dte.ActiveDocument;

            if (ac == null || !(ac.Object() is TextDocument activeDocument) || !IsSaveTobuffer)
            {
                return;
            }

            var text = activeDocument.CreateEditPoint(activeDocument.StartPoint).GetText(activeDocument.EndPoint);

            string[] lines = text.Split('\n').ToArray();
            ClipboartUpdate(lines, LabelStartName, LabelEndName);
        }

        private void ShowMsg(string message, string title = "")
        {
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        #endregion
    }
}