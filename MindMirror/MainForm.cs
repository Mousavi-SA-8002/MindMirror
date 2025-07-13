using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MindMirror
{
    public partial class MainForm : MaterialForm
    {
        string filePath = "thoughts.txt";
        string encryptionKey = "MySuperSecretKey123!";

        string[] dailyQuestions = new string[]
        {
            "امروز چه چیزی باعث خوشحالیت شد؟",
            "امروز چی یاد گرفتی؟",
            "اگه امروز رو دوباره زندگی می‌کردی، چی رو تغییر می‌دادی؟",
            "چه کاری امروز انجام دادی که بهش افتخار می‌کنی؟",
            "امروز از کی یا چی سپاسگزار بودی؟",
            "چه چیزی ذهنت رو امروز درگیر کرده بود؟",
            "چه تصمیم مهمی امروز گرفتی؟"
        };

        public MainForm()
        {
            InitializeComponent();
            LoadThoughts();
            ShowDailyQuestion();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string userInput = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            string dateLine = $"[{DateTime.Now:yyyy/MM/dd - HH:mm}]";
            string entry = $"{dateLine}\n{userInput}\n---";

            string existingData = File.Exists(filePath) ? DecryptFile() : "";
            string newData = existingData + entry + Environment.NewLine + Environment.NewLine;

            EncryptFile(newData);
            txtInput.Clear();
            txtInput.Focus();

            LoadThoughts();
        }

        private void LoadThoughts()
        {
            txtHistory.Clear();
            if (!File.Exists(filePath)) return;

            string decryptedData = DecryptFile();
            string[] lines = decryptedData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            txtHistory.SuspendLayout();
            txtHistory.Font = new Font("Segoe UI", 10);
            txtHistory.SelectionStart = txtHistory.TextLength;

            string date = "";
            string content = "";

            foreach (string line in lines)
            {
                if (line.StartsWith("["))
                {
                    date = line;
                }
                else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("---"))
                {
                    content += line + Environment.NewLine;
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (string.IsNullOrWhiteSpace(lastSearch) ||
                        content.IndexOf(lastSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        date.IndexOf(lastSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        txtHistory.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
                        txtHistory.SelectionColor = Color.Teal;
                        txtHistory.AppendText(date + Environment.NewLine);

                        txtHistory.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
                        txtHistory.SelectionColor = Color.Black;
                        txtHistory.AppendText("\t" + content + Environment.NewLine);
                    }

                    date = "";
                    content = "";
                }
            }

            txtHistory.ScrollToCaret();
            txtHistory.ResumeLayout();
        }

        private void ShowDailyQuestion()
        {
            int dayIndex = DateTime.Now.DayOfYear % dailyQuestions.Length;
            lblQuestion.Text = dailyQuestions[dayIndex] + " 🧠";
        }

        string lastSearch = "";

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lastSearch = txtSearch.Text.Trim();
            LoadThoughts();
        }

        private void EncryptFile(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes("Salt1234!"), 10000);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                    sw.Close();
                    File.WriteAllBytes(filePath, ms.ToArray());
                }
            }
        }

        private string DecryptFile()
        {
            try
            {
                byte[] cipherBytes = File.ReadAllBytes(filePath);
                using (Aes aes = Aes.Create())
                {
                    var key = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes("Salt1234!"), 10000);
                    aes.Key = key.GetBytes(32);
                    aes.IV = key.GetBytes(16);

                    using (var ms = new MemoryStream(cipherBytes))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}