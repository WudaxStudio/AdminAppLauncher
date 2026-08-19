using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AdminAppLauncher
{
    public class ConfigForm : Form
    {
        private TextBox _domainBox;
        private TextBox _userBox;
        private TextBox _passBox;
        private ListBox _appList;
        private List<AppEntry> _apps;
        private AppConfig _existingConfig;

        public ConfigForm(AppConfig existingConfig)
        {
            _existingConfig = existingConfig;
            if (_existingConfig == null)
                _existingConfig = new AppConfig();
            _apps = (_existingConfig.Applications != null)
                ? new List<AppEntry>(_existingConfig.Applications)
                : new List<AppEntry>();

            if (_apps.Count == 0)
            {
                AddDefaultApps();
            }

            InitializeUI();
        }

        private void AddDefaultApps()
        {
            string sys = Environment.GetFolderPath(Environment.SpecialFolder.System);

            string win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            _apps.Add(new AppEntry { Name = "注册表编辑器", Path = win + "\\regedit.exe" });
            _apps.Add(new AppEntry { Name = "PowerShell", Path = sys + "\\WindowsPowerShell\\v1.0\\powershell.exe" });
            _apps.Add(new AppEntry { Name = "命令提示符", Path = sys + "\\cmd.exe" });
            _apps.Add(new AppEntry { Name = "服务管理", Path = sys + "\\mmc.exe", Arguments = sys + "\\services.msc" });
            _apps.Add(new AppEntry { Name = "计算机管理", Path = sys + "\\mmc.exe", Arguments = sys + "\\compmgmt.msc" });
            _apps.Add(new AppEntry { Name = "设备管理器", Path = sys + "\\mmc.exe", Arguments = sys + "\\devmgmt.msc" });
        }

        private void InitializeUI()
        {
            Text = "配置 - 管理员应用启动器";
            Size = new Size(540, 640);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Microsoft YaHei UI", 9F);
            BackColor = Color.FromArgb(245, 247, 250);

            int labelX = 25;
            int inputX = 150;
            int inputW = 340;
            int y = 15;
            int rowH = 32;

            Label title = new Label();
            title.Text = "管理员凭据与应用配置";
            title.Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);
            title.Location = new Point(25, y);
            title.AutoSize = true;
            y += 40;

            Label hint = new Label();
            hint.Text = "密码将通过 Windows DPAPI 加密存储，仅限本机使用。";
            hint.Font = new Font("Microsoft YaHei UI", 8.5F);
            hint.ForeColor = Color.Gray;
            hint.Location = new Point(25, y);
            hint.AutoSize = true;
            y += 25;

            Label domainLabel = MakeLabel("域名/计算机名:", labelX, y);
            _domainBox = MakeTextBox(inputX, y, inputW);
            _domainBox.Text = _existingConfig.Credentials.Domain;
            Label domainHint = new Label();
            domainHint.Text = "(留空表示本地账户，输入 . 也代表本机)";
            domainHint.Font = new Font("Microsoft YaHei UI", 8F);
            domainHint.ForeColor = Color.Gray;
            domainHint.Location = new Point(25, y + 24);
            domainHint.AutoSize = true;
            y += rowH + 18;

            Label userLabel = MakeLabel("用户名:", labelX, y);
            _userBox = MakeTextBox(inputX, y, inputW);
            _userBox.Text = _existingConfig.Credentials.Username;
            y += rowH;

            Label passLabel = MakeLabel("密码:", labelX, y);
            _passBox = MakeTextBox(inputX, y, inputW);
            _passBox.PasswordChar = '*';
            _passBox.Text = _existingConfig.Credentials.Password;
            y += rowH + 8;

            Label appLabel = MakeLabel("已配置应用:", labelX, y);
            y += rowH;

            _appList = new ListBox();
            _appList.Location = new Point(inputX, y);
            _appList.Size = new Size(340, 160);
            _appList.Font = new Font("Microsoft YaHei UI", 9F);
            y += _appList.Height + 8;

            Button addBtn = new Button();
            addBtn.Text = "+ 添加";
            addBtn.Location = new Point(inputX, y);
            addBtn.Size = new Size(75, 32);
            addBtn.FlatStyle = FlatStyle.Flat;
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += AddBtn_Click;

            Button editBtn = new Button();
            editBtn.Text = "编辑";
            editBtn.Location = new Point(inputX + 80, y);
            editBtn.Size = new Size(55, 32);
            editBtn.FlatStyle = FlatStyle.Flat;
            editBtn.FlatAppearance.BorderSize = 0;
            editBtn.Click += EditBtn_Click;

            Button removeBtn = new Button();
            removeBtn.Text = "删除";
            removeBtn.Location = new Point(inputX + 140, y);
            removeBtn.Size = new Size(55, 32);
            removeBtn.FlatStyle = FlatStyle.Flat;
            removeBtn.FlatAppearance.BorderSize = 0;
            removeBtn.Click += RemoveBtn_Click;

            Button upBtn = new Button();
            upBtn.Text = "上移";
            upBtn.Location = new Point(inputX + 200, y);
            upBtn.Size = new Size(55, 32);
            upBtn.FlatStyle = FlatStyle.Flat;
            upBtn.FlatAppearance.BorderSize = 0;
            upBtn.Click += MoveUpBtn_Click;

            Button downBtn = new Button();
            downBtn.Text = "下移";
            downBtn.Location = new Point(inputX + 260, y);
            downBtn.Size = new Size(55, 32);
            downBtn.FlatStyle = FlatStyle.Flat;
            downBtn.FlatAppearance.BorderSize = 0;
            downBtn.Click += MoveDownBtn_Click;
            y += 40;

            Button saveBtn = new Button();
            saveBtn.Text = "保存配置";
            saveBtn.Location = new Point(inputX, y + 5);
            saveBtn.Size = new Size(130, 36);
            saveBtn.BackColor = Color.FromArgb(0, 120, 212);
            saveBtn.ForeColor = Color.White;
            saveBtn.FlatStyle = FlatStyle.Flat;
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            saveBtn.Click += SaveBtn_Click;

            Button cancelBtn = new Button();
            cancelBtn.Text = "取消";
            cancelBtn.Location = new Point(inputX + 140, y + 5);
            cancelBtn.Size = new Size(90, 36);
            cancelBtn.FlatStyle = FlatStyle.Flat;
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.Font = new Font("Microsoft YaHei UI", 10F);
            cancelBtn.Click += delegate(object s, EventArgs ev) { this.Close(); };

            Controls.Add(title);
            Controls.Add(hint);
            Controls.Add(domainLabel);
            Controls.Add(_domainBox);
            Controls.Add(domainHint);
            Controls.Add(userLabel);
            Controls.Add(_userBox);
            Controls.Add(passLabel);
            Controls.Add(_passBox);
            Controls.Add(appLabel);
            Controls.Add(_appList);
            Controls.Add(addBtn);
            Controls.Add(editBtn);
            Controls.Add(removeBtn);
            Controls.Add(upBtn);
            Controls.Add(downBtn);
            Controls.Add(saveBtn);
            Controls.Add(cancelBtn);

            RefreshAppList();
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

        private void RefreshAppList()
        {
            _appList.Items.Clear();
            foreach (AppEntry app in _apps)
            {
                _appList.Items.Add(app.Name + "  |  " + app.Path);
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            AppEditForm editForm = new AppEditForm(null);
            if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Result != null)
            {
                _apps.Add(editForm.Result);
                RefreshAppList();
                _appList.SelectedIndex = _apps.Count - 1;
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            int idx = _appList.SelectedIndex;
            if (idx < 0 || idx >= _apps.Count)
            {
                MessageBox.Show(this, "请先选择一个应用。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AppEditForm editForm = new AppEditForm(_apps[idx]);
            if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Result != null)
            {
                _apps[idx] = editForm.Result;
                RefreshAppList();
                _appList.SelectedIndex = idx;
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            int idx = _appList.SelectedIndex;
            if (idx < 0 || idx >= _apps.Count)
            {
                MessageBox.Show(this, "请先选择一个应用。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this, "确定要删除「" + _apps[idx].Name + "」吗？",
                "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _apps.RemoveAt(idx);
                RefreshAppList();
            }
        }

        private void MoveUpBtn_Click(object sender, EventArgs e)
        {
            int idx = _appList.SelectedIndex;
            if (idx <= 0)
            {
                MessageBox.Show(this, "请选择一个非顶部的应用进行上移。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AppEntry temp = _apps[idx];
            _apps[idx] = _apps[idx - 1];
            _apps[idx - 1] = temp;
            RefreshAppList();
            _appList.SelectedIndex = idx - 1;
        }

        private void MoveDownBtn_Click(object sender, EventArgs e)
        {
            int idx = _appList.SelectedIndex;
            if (idx < 0 || idx >= _apps.Count - 1)
            {
                MessageBox.Show(this, "请选择一个非底部的应用进行下移。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AppEntry temp = _apps[idx];
            _apps[idx] = _apps[idx + 1];
            _apps[idx + 1] = temp;
            RefreshAppList();
            _appList.SelectedIndex = idx + 1;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userBox.Text.Trim()))
            {
                MessageBox.Show(this, "请输入管理员用户名。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _userBox.Focus();
                return;
            }
            if (string.IsNullOrEmpty(_passBox.Text))
            {
                MessageBox.Show(this, "请输入管理员密码。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _passBox.Focus();
                return;
            }
            if (_apps.Count == 0)
            {
                MessageBox.Show(this, "请至少添加一个应用。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppConfig config = new AppConfig();
            config.Credentials.Domain = _domainBox.Text.Trim();
            config.Credentials.Username = _userBox.Text.Trim();
            config.Credentials.Password = _passBox.Text;
            config.Applications = _apps;

            try
            {
                ConfigManager.Save(config);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存配置失败:\n\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
