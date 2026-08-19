using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AdminAppLauncher
{
    public class MainForm : Form
    {
        private AppConfig _config;
        private Label _statusLabel;
        private FlowLayoutPanel _appPanel;
        private Panel _dropPanel;

        public MainForm(AppConfig config)
        {
            _config = config;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "管理员应用启动器";
            Size = new Size(420, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Microsoft YaHei UI", 9F);

            Label titleLabel = new Label();
            titleLabel.Text = "请选择要启动的应用";
            titleLabel.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 55;
            titleLabel.Padding = new Padding(0, 18, 0, 10);

            _appPanel = new FlowLayoutPanel();
            _appPanel.Dock = DockStyle.Fill;
            _appPanel.FlowDirection = FlowDirection.TopDown;
            _appPanel.WrapContents = false;
            _appPanel.Padding = new Padding(25, 5, 25, 10);
            _appPanel.AutoScroll = true;
            _appPanel.BackColor = Color.FromArgb(245, 247, 250);

            PopulateAppButtons();

            _dropPanel = new Panel();
            _dropPanel.Dock = DockStyle.Bottom;
            _dropPanel.Height = 55;
            _dropPanel.BackColor = Color.FromArgb(235, 240, 248);
            _dropPanel.AllowDrop = true;
            _dropPanel.Paint += DropPanel_Paint;
            _dropPanel.DragEnter += DropPanel_DragEnter;
            _dropPanel.DragLeave += DropPanel_DragLeave;
            _dropPanel.DragDrop += DropPanel_DragDrop;

            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 40;
            bottomPanel.BackColor = Color.FromArgb(238, 240, 244);

            _statusLabel = new Label();
            _statusLabel.Text = "就绪 - 请选择上方应用以管理员身份启动";
            _statusLabel.Font = new Font("Microsoft YaHei UI", 9F);
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.Padding = new Padding(15, 0, 0, 0);
            _statusLabel.ForeColor = Color.Gray;

            Button settingsBtn = new Button();
            settingsBtn.Text = "设置";
            settingsBtn.Dock = DockStyle.Right;
            settingsBtn.Width = 75;
            settingsBtn.FlatStyle = FlatStyle.Flat;
            settingsBtn.FlatAppearance.BorderSize = 0;
            settingsBtn.BackColor = Color.FromArgb(225, 228, 232);
            settingsBtn.Font = new Font("Microsoft YaHei UI", 9F);
            settingsBtn.Cursor = Cursors.Hand;
            settingsBtn.Click += SettingsBtn_Click;

            Button aboutBtn = new Button();
            aboutBtn.Text = "关于";
            aboutBtn.Dock = DockStyle.Right;
            aboutBtn.Width = 60;
            aboutBtn.FlatStyle = FlatStyle.Flat;
            aboutBtn.FlatAppearance.BorderSize = 0;
            aboutBtn.BackColor = Color.FromArgb(225, 228, 232);
            aboutBtn.Font = new Font("Microsoft YaHei UI", 9F);
            aboutBtn.Cursor = Cursors.Hand;
            aboutBtn.Click += AboutBtn_Click;

            bottomPanel.Controls.Add(_statusLabel);
            bottomPanel.Controls.Add(aboutBtn);
            bottomPanel.Controls.Add(settingsBtn);

            Controls.Add(_appPanel);
            Controls.Add(_dropPanel);
            Controls.Add(titleLabel);
            Controls.Add(bottomPanel);
        }

        private void PopulateAppButtons()
        {
            _appPanel.Controls.Clear();

            foreach (AppEntry app in _config.Applications)
            {
                Button btn = new Button();
                btn.Text = app.Name;
                btn.Font = new Font("Microsoft YaHei UI", 11F);
                btn.Size = new Size(340, 52);
                btn.Margin = new Padding(5, 5, 5, 5);
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.FromArgb(0, 120, 212);
                btn.ForeColor = Color.White;
                btn.Cursor = Cursors.Hand;
                btn.Tag = app;
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 180);
                btn.Click += AppButton_Click;
                _appPanel.Controls.Add(btn);
            }
        }

        private void RefreshAppList()
        {
            try
            {
                _config = ConfigManager.Load();
                PopulateAppButtons();
                _statusLabel.Text = "配置已更新 - 共 " + _config.Applications.Count + " 个应用";
                _statusLabel.ForeColor = Color.FromArgb(22, 163, 74);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "重新加载配置失败:\n\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            AppConfig currentConfig;
            try
            {
                currentConfig = ConfigManager.Load();
            }
            catch
            {
                currentConfig = _config;
            }

            ConfigForm configForm = new ConfigForm(currentConfig);
            DialogResult result = configForm.ShowDialog(this);
            configForm.Dispose();

            if (result == DialogResult.OK)
            {
                RefreshAppList();
            }
        }

        private void AboutBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "版本信息：V1.1.2\n" +
                "开发人员：Wudax\n" +
                "联系邮箱：wudaxstudio@qq.com\n" +
                "\n" +
                "软件仅用于测试使用，切勿非法使用！",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void AppButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            AppEntry app = btn.Tag as AppEntry;
            if (app == null) return;

            try
            {
                _statusLabel.Text = "正在启动: " + app.Name + " ...";
                _statusLabel.ForeColor = Color.FromArgb(0, 120, 212);
                Application.DoEvents();

                ProcessLauncher.Launch(_config, app);

                _statusLabel.Text = "已启动: " + app.Name;
                _statusLabel.ForeColor = Color.FromArgb(22, 163, 74);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "启动失败: " + ex.Message;
                _statusLabel.ForeColor = Color.FromArgb(220, 38, 38);
                MessageBox.Show(
                    "启动「" + app.Name + "」失败:\n\n" + ex.Message +
                    (ex.InnerException != null ? "\n\n详细信息: " + ex.InnerException.Message : ""),
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DropPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            if (p == null) return;

            ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle,
                Color.FromArgb(180, 190, 200), 2, ButtonBorderStyle.Dashed,
                Color.FromArgb(180, 190, 200), 2, ButtonBorderStyle.Dashed,
                Color.FromArgb(180, 190, 200), 2, ButtonBorderStyle.Dashed,
                Color.FromArgb(180, 190, 200), 2, ButtonBorderStyle.Dashed);

            string text = "拖拽文件到此区域以管理员身份运行（临时，不保存到配置）";
            using (Font f = new Font("Microsoft YaHei UI", 9F))
            using (SolidBrush b = new SolidBrush(Color.Gray))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(text, f, b, p.ClientRectangle, sf);
            }
        }

        private void DropPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                _dropPanel.BackColor = Color.FromArgb(220, 237, 252);
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void DropPanel_DragLeave(object sender, EventArgs e)
        {
            _dropPanel.BackColor = Color.FromArgb(235, 240, 248);
        }

        private void DropPanel_DragDrop(object sender, DragEventArgs e)
        {
            _dropPanel.BackColor = Color.FromArgb(235, 240, 248);

            if (string.IsNullOrEmpty(_config.Credentials.Username) ||
                string.IsNullOrEmpty(_config.Credentials.Password))
            {
                MessageBox.Show(this, "管理员凭据未配置，请先点击「设置」配置管理员账号和密码。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show(this, "文件不存在或不是有效文件:\n" + file,
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    continue;
                }

                try
                {
                    _statusLabel.Text = "正在以管理员身份启动: " + Path.GetFileName(file) + " ...";
                    _statusLabel.ForeColor = Color.FromArgb(0, 120, 212);
                    Application.DoEvents();

                    ProcessLauncher.LaunchFile(_config, file);

                    _statusLabel.Text = "已以管理员身份启动: " + Path.GetFileName(file);
                    _statusLabel.ForeColor = Color.FromArgb(22, 163, 74);
                }
                catch (Exception ex)
                {
                    _statusLabel.Text = "启动失败: " + ex.Message;
                    _statusLabel.ForeColor = Color.FromArgb(220, 38, 38);
                    MessageBox.Show(
                        "以管理员身份启动「" + Path.GetFileName(file) + "」失败:\n\n" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            }
        }
    }
}
