﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EntryEdit
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (!HandleCommandLinePatch(args))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }
        }

        private static bool HandleCommandLinePatch(string[] args)
        {
            System.Collections.Generic.KeyValuePair<string, string> patchFilepaths = PatcherLib.Utilities.Utilities.GetPatchFilepaths(args, ".eepatch");

            if ((string.IsNullOrEmpty(patchFilepaths.Key)) || (string.IsNullOrEmpty(patchFilepaths.Value)))
            {
                return false;
            }
            else
            {
                System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                //while (!System.Diagnostics.Debugger.IsAttached) System.Threading.Thread.Sleep(100);

                DataHelper dataHelper = new DataHelper();
                EntryData patchData = PatchHelper.GetEntryDataFromPatchFile(patchFilepaths.Key, dataHelper);

                if (patchFilepaths.Value.ToLower().Trim().EndsWith(".psv"))
                {
                    PatchHelper.PatchPsxSaveState(patchData, patchFilepaths.Value, dataHelper);
                }
                else
                {
                    PatchHelper.PatchISO(patchData, patchFilepaths.Value, dataHelper);
                }

                return true;
            }
        }
    }
}
