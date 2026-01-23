using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TongbaoExchangeCalc
{
    public partial class RecordForm : Form
    {
        //public string Title
        //{
        //    get
        //    {
        //        return this.Text;
        //    }
        //    set
        //    {
        //        this.Text = value;
        //    }
        //}

        public string Content
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                SetText(value);
            }
        }

        private Action mOnClearClick;

        public RecordForm(MainForm mainForm)
        {
            InitializeComponent();
            this.Icon = mainForm.Icon;
        }

        public void UpdateScroll()
        {
            if (textBox1.Text.Length > 0)
            {
                textBox1.SelectionLength = textBox1.Text.Length - 1;
                textBox1.ScrollToCaret();
            }
        }

        public void ClearText()
        {
            textBox1.Clear();
        }

        public void SetText(string text)
        {
            _ = SetTextAsync(text);
        }


        public void AppendText(string text)
        {
            textBox1.AppendText(text ?? string.Empty);
            UpdateScroll();
        }

        public async Task SetTextAsync(string text, int chunkSize = 5_000_000)
        {
            text ??= string.Empty;

            // 先清空（UI 线程）
            if (InvokeRequired)
            {
                await InvokeAsync(() => textBox1.Clear());
            }
            else
            {
                textBox1.Clear();
            }

            // 后台线程分块
            await Task.Run(async () =>
            {
                int length = text.Length;
                int offset = 0;

                while (offset < length)
                {
                    int size = Math.Min(chunkSize, length - offset);
                    string chunk = text.Substring(offset, size);
                    offset += size;

                    // 回 UI 线程追加
                    await InvokeAsync(() =>
                    {
                        textBox1.AppendText(chunk);
                    });
                }
            });
        }

        public void SetClearCallback(Action callback)
        {
            mOnClearClick = callback;
        }

        private Task InvokeAsync(Action action)
        {
            if (!InvokeRequired)
            {
                action();
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>();
            BeginInvoke(new Action(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }

        private void RecordForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            mOnClearClick?.Invoke();
        }
    }
}
