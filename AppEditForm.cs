using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdminAppLauncher
{
    public class AppEditForm : Form
    {
        private TextBox _nameBox;
        private TextBox _pathBox;
        private TextBox _argsBox;
        private TextBox _workDirBox;
        public AppEntry Result;

        public AppEditForm(AppEntry existing)
        {
            Result = (existing != null) ? existing.Clone() : null;
            InitializeUI();
            if (existing != null)
            {
                _nameBox.Text = existing.Name;
                _pathBox.Text = existing.Path;
                _argsBox.Text = existing.Arguments;
                _workDirBox.Text = existing.WorkingDirectory;
            }
        }

        private void InitializeUI()
        {
            Text = "应用配置";
            Size = new Size(460, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Microsoft YaHei UI", 9F);

            int labelX = 20;
            int inputX = 110;
            int inputW = 310;
            int y = 15;
            int rowH = 32;

            Label l1 = MakeLabel("显示名称:", labelX, y);
            _nameBox = MakeTextBox(inputX, y, inputW);
            y += rowH;

            Label l2 = MakeLabel("程序路径:", labelX, y);
            _pathBox = MakeTextBox(inputX, y, inputW - 85);
            Button browseBtn = new Button();
            browseBtn.Text = "浏览...";
            browseBtn.Location = new Point(inputX + inputW - 75, y + 1);
            browseBtn.Size = new Size(75, 25);
            browseBtn.Click += delegate(object s, EventArgs ev)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
                dlg.Title = "选择要启动的程序";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _pathBox.Text = dlg.FileName;
                    if (string.IsNullOrEmpty(_nameBox.Text.Trim()))
                        _nameBox.Text = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                }
            };
            y += rowH;

            Label l3 = MakeLabel("启动参数:", labelX, y);
            _argsBox = MakeTextBox(inputX, y, inputW);
            y += rowH;

            Label l4 = MakeLabel("工作目录:", labelX, y);
            _workDirBox = MakeTextBox(inputX, y, inputW);
            y += rowH + 10;

            Button okBtn = new Button();
            okBtn.Text = "确定";
            okBtn.Location = new Point(inputX, y);
            okBtn.Size = new Size(95, 32);
            okBtn.BackColor = Color.FromArgb(0, 120, 212);
            okBtn.ForeColor = Color.White;
            okBtn.FlatStyle = FlatStyle.Flat;
            okBtn.FlatAppearance.BorderSize = 0;
            okBtn.Click += OkBtn_Click;

            Button cancelBtn = new Button();
            cancelBtn.Text = "取消";
            cancelBtn.Location = new Point(inputX + 105, y);
            cancelBtn.Size = new Size(95, 32);
            cancelBtn.FlatStyle = FlatStyle.Flat;
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.Click += delegate(object s, EventArgs ev)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            Controls.Add(l1);
            Controls.Add(l2);
            Controls.Add(l3);
            Controls.Add(l4);
            Controls.Add(_nameBox);
            Controls.Add(_pathBox);
            Controls.Add(_argsBox);
            Controls.Add(_workDirBox);
            Controls.Add(browseBtn);
            Controls.Add(okBtn);
            Controls.Add(cancelBtn);

            AcceptButton = okBtn;
            CancelButton = cancelBtn;
        }

        private Label MakeLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y + 4);
            l.AutoSize = true;
            return l;
        }

        private TextBox MakeTextBox(int x, int y, int w)
        {
            TextBox tb = new TextBox();
            tb.Location = new Point(x, y);
            tb.Size = new Size(w, 25);
            return tb;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_nameBox.Text.Trim()))
            {
                MessageBox.Show(this, "请输入显示名称。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameBox.Focus();
                return;
            }
            if (string.IsNullOrEmpty(_pathBox.Text.Trim()))
            {
                MessageBox.Show(this, "请输入程序路径。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _pathBox.Focus();
                return;
            }

            if (Result == null)
                Result = new AppEntry();
            Result.Name = _nameBox.Text.Trim();
            Result.Path = _pathBox.Text.Trim();
            Result.Arguments = _argsBox.Text.Trim();
            Result.WorkingDirectory = _workDirBox.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
