using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace MindMirror
{
    static class Program
    {
        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        private static extern int CredUIPromptForCredentials(
            ref CREDUI_INFO creditUR,
            string targetName,
            IntPtr reserved1,
            int iError,
            StringBuilder userName,
            int maxUserName,
            StringBuilder password,
            int maxPassword,
            ref bool save,
            int flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [STAThread]
        static void Main()
        {
            var credUI = new CREDUI_INFO
            {
                cbSize = Marshal.SizeOf(typeof(CREDUI_INFO)),
                pszCaptionText = "ورود به Mind Mirror",
                pszMessageText = "لطفاً رمز ویندوز خود را وارد کنید"
            };

            StringBuilder user = new StringBuilder(100);
            StringBuilder pass = new StringBuilder(100);
            bool save = false;

            int result = CredUIPromptForCredentials(
                ref credUI,
                Environment.MachineName,
                IntPtr.Zero,
                0,
                user,
                100,
                pass,
                100,
                ref save,
                1);

            if (result == 0)
            {
                string[] parts = user.ToString().Split('\\');
                string username = parts.Length > 1 ? parts[1] : parts[0];
                string domain = parts.Length > 1 ? parts[0] : Environment.MachineName;

                bool valid = LogonUser(username, domain, pass.ToString(), 2, 0, out IntPtr token);
                if (valid)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    MessageBox.Show("رمز اشتباه بود یا دسترسی ندارید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}