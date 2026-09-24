using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReelManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class RoundButton : Button
    {
        public int Radius { get; set; } = 8;
        public Color Normal { get; set; } = Color.FromArgb(124, 58, 237);
        public Color Hover { get; set; } = Color.FromArgb(139, 92, 246);
        public Color Pressed { get; set; } = Color.FromArgb(109, 40, 217);
        private bool h, p;

        public RoundButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e) { h = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { h = false; p = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { p = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { p = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = !Enabled ? Color.FromArgb(70, 70, 90) : p ? Pressed : h ? Hover : Normal;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundPath(r, Radius))
            using (var b = new SolidBrush(bg))
                e.Graphics.FillPath(b, path);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath RoundPath(Rectangle r, int rad)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            path.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            path.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class MainForm : Form
    {
        static readonly Color BgDark = Color.FromArgb(18, 18, 28);
        static readonly Color CardBg = Color.FromArgb(30, 30, 46);
        static readonly Color CardBorder = Color.FromArgb(60, 60, 90);
        static readonly Color Accent = Color.FromArgb(124, 58, 237);
        static readonly Color TextMain = Color.FromArgb(235, 235, 245);
        static readonly Color Success = Color.FromArgb(34, 197, 94);
        static readonly Color Warning = Color.FromArgb(245, 158, 11);
        static readonly Color Error = Color.FromArgb(239, 68, 68);
        static readonly Color Info = Color.FromArgb(96, 165, 250);
        static readonly Color Teal = Color.FromArgb(20, 184, 166);
        static readonly Color Pink = Color.FromArgb(236, 72, 153);

        const string Dev = "وقاص عباس جاويش التوم";

        Database db;
        TabControl tabs;

        // Tab 1 - Import
        TextBox txtSource;
        TextBox txtDest;
        ComboBox cmbImportProject;
        CheckBox chkAutoOrganize;
        RichTextBox logImport;
        ProgressBar pbImport;
        RoundButton btnBrowseSource, btnBrowseDest, btnStartImport;

        // Tab 2 - Library
        ListView lvVideos;
        TextBox txtSearch;
        ComboBox cmbFilterProject;
        ComboBox cmbFilterTag;
        RoundButton btnRefresh, btnOpenFile, btnDeleteVideo, btnAddTag;
        Label lblLibStats;

        // Tab 3 - Projects
        ListView lvProjects;
        RoundButton btnAddProject, btnDeleteProject, btnChangeStatus;
        Label lblProjStats;

        // Tab 4 - Clients
        ListView lvClients;
        RoundButton btnAddClient, btnDeleteClient;

        // Tab 5 - Reports
        RichTextBox txtReport;
        RoundButton btnGenerateReport, btnExportReport;

        public MainForm()
        {
            Text = "Reel Manager - إدارة مادة مصوري الفيديو | " + Dev;
            Width = 1250; Height = 850;
            BackColor = BgDark; ForeColor = TextMain;
            Font = new Font("Segoe UI", 9.5F);
            StartPosition = FormStartPosition.CenterScreen;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            MinimumSize = new Size(1100, 750);

            db = new Database();
            db.Initialize();

            BuildUI();
            LoadEverything();
        }

        void BuildUI()
        {
            // ===== Header =====
            var header = new Panel { Dock = DockStyle.Top, Height = 105 };
            header.Paint += (s, e) =>
            {
                using (var lg = new LinearGradientBrush(header.ClientRectangle,
                    Color.FromArgb(50, 20, 100), Color.FromArgb(124, 58, 237), 0F))
                    e.Graphics.FillRectangle(lg, header.ClientRectangle);
            };

            var title = new Label
            {
                Text = "Reel Manager",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(30, 15), BackColor = Color.Transparent
            };
            var sub = new Label
            {
                Text = "إدارة مادة مصوري الفيديو — استيراد، تنظيم، مشاريع، عملاء، تقارير",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 210, 255), AutoSize = true,
                Location = new Point(35, 60), BackColor = Color.Transparent
            };
            var dev = new Label
            {
                Text = "Developed by  " + Dev + "  •  v1.0",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 220, 130), AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.Transparent
            };
            dev.Location = new Point(header.Width - 320, 75);
            header.Resize += (s, e) => dev.Location = new Point(header.Width - dev.Width - 30, 75);

            header.Controls.Add(title);
            header.Controls.Add(sub);
            header.Controls.Add(dev);

            // ===== Tabs =====
            tabs = new TabControl
            {
                Location = new Point(20, 120),
                Size = new Size(1200, 680),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Point(15, 8),
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(200, 38),
                SizeMode = TabSizeMode.Fixed
            };
            tabs.DrawItem += (s, e) =>
            {
                var g = e.Graphics;
                var rect = tabs.GetTabRect(e.Index);
                bool sel = e.Index == tabs.SelectedIndex;
                using (var bg = new SolidBrush(sel ? Accent : CardBg))
                    g.FillRectangle(bg, rect);
                TextRenderer.DrawText(g, tabs.TabPages[e.Index].Text,
                    new Font("Segoe UI", 9F, FontStyle.Bold), rect,
                    sel ? Color.White : TextMain,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var t1 = new TabPage("الاستيراد") { BackColor = CardBg, ForeColor = TextMain };
            var t2 = new TabPage("المكتبة") { BackColor = CardBg, ForeColor = TextMain };
            var t3 = new TabPage("المشاريع") { BackColor = CardBg, ForeColor = TextMain };
            var t4 = new TabPage("العملاء") { BackColor = CardBg, ForeColor = TextMain };
            var t5 = new TabPage("التقارير") { BackColor = CardBg, ForeColor = TextMain };

            BuildImportTab(t1);
            BuildLibraryTab(t2);
            BuildProjectsTab(t3);
            BuildClientsTab(t4);
            BuildReportsTab(t5);

            tabs.TabPages.Add(t1);
            tabs.TabPages.Add(t2);
            tabs.TabPages.Add(t3);
            tabs.TabPages.Add(t4);
            tabs.TabPages.Add(t5);

            Controls.Add(header);
            Controls.Add(tabs);
        }

        // ================= Import Tab =================
        void BuildImportTab(TabPage tab)
        {
            var l1 = new Label
            {
                Text = "مصدر الملفات (كارت الكاميرا أو مجلد):",
                Location = new Point(20, 20), AutoSize = true,
                ForeColor = TextMain, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            txtSource = new TextBox
            {
                Location = new Point(20, 50), Width = 900,
                BackColor = Color.FromArgb(45, 45, 65), ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F)
            };

            btnBrowseSource = new RoundButton
            {
                Text = "استعراض", Location = new Point(930, 48),
                Size = new Size(120, 32), Normal = Color.FromArgb(55, 55, 80),
                Hover = Color.FromArgb(75, 75, 105), Radius = 6
            };
            btnBrowseSource.Click += BtnBrowseSource_Click;

            var l2 = new Label
            {
                Text = "مجلد الوجهة (المكتبة الرئيسية):",
                Location = new Point(20, 95), AutoSize = true,
                ForeColor = TextMain, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            txtDest = new TextBox
            {
                Location = new Point(20, 125), Width = 900,
                BackColor = Color.FromArgb(45, 45, 65), ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F)
            };

            btnBrowseDest = new RoundButton
            {
                Text = "استعراض", Location = new Point(930, 123),
                Size = new Size(120, 32), Normal = Color.FromArgb(55, 55, 80),
                Hover = Color.FromArgb(75, 75, 105), Radius = 6
            };
            btnBrowseDest.Click += BtnBrowseDest_Click;

            var l3 = new Label
            {
                Text = "ربط بمشروع:",
                Location = new Point(20, 170), AutoSize = true,
                ForeColor = TextMain, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            cmbImportProject = new ComboBox
            {
                Location = new Point(150, 168), Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 65), ForeColor = TextMain,
                Font = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat
            };

            chkAutoOrganize = new CheckBox
            {
                Text = "تنظيم تلقائي حسب التاريخ (YYYY/MM/DD)",
                Location = new Point(580, 168), AutoSize = true,
                ForeColor = Success, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.Transparent, Checked = true
            };

            btnStartImport = new RoundButton
            {
                Text = "بدء الاستيراد", Location = new Point(20, 210),
                Size = new Size(220, 45), Normal = Accent,
                Hover = Color.FromArgb(139, 92, 246), Radius = 8,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnStartImport.Click += BtnStartImport_Click;

            pbImport = new ProgressBar
            {
                Location = new Point(260, 220), Width = 790, Height = 25,
                Style = ProgressBarStyle.Continuous,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            logImport = new RichTextBox
            {
                Location = new Point(20, 275), Size = new Size(1150, 350),
                BackColor = Color.FromArgb(12, 12, 20), ForeColor = TextMain,
                Font = new Font("Consolas", 9F), ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle, ScrollBars = RichTextBoxScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                RightToLeft = RightToLeft.No
            };

            tab.Controls.Add(l1);
            tab.Controls.Add(txtSource);
            tab.Controls.Add(btnBrowseSource);
            tab.Controls.Add(l2);
            tab.Controls.Add(txtDest);
            tab.Controls.Add(btnBrowseDest);
            tab.Controls.Add(l3);
            tab.Controls.Add(cmbImportProject);
            tab.Controls.Add(chkAutoOrganize);
            tab.Controls.Add(btnStartImport);
            tab.Controls.Add(pbImport);
            tab.Controls.Add(logImport);
        }

        void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "اختر مصدر الملفات";
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtSource.Text = fbd.SelectedPath;
            }
        }

        void BtnBrowseDest_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "اختر مجلد الوجهة";
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtDest.Text = fbd.SelectedPath;
            }
        }

        void BtnStartImport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSource.Text) || !Directory.Exists(txtSource.Text))
            { MessageBox.Show("اختر مصدرًا صحيحًا."); return; }
            if (string.IsNullOrWhiteSpace(txtDest.Text) || !Directory.Exists(txtDest.Text))
            { MessageBox.Show("اختر مجلد وجهة صحيحًا."); return; }

            string source = txtSource.Text;
            string dest = txtDest.Text;
            bool organize = chkAutoOrganize.Checked;
            int projectId = GetSelectedProjectId(cmbImportProject);

            btnStartImport.Enabled = false;
            logImport.Clear();
            pbImport.Value = 0;

            Task.Run(() => DoImport(source, dest, organize, projectId));
        }

        void DoImport(string source, string dest, bool organize, int projectId)
        {
            Log(logImport, "بدء الاستيراد من: " + source, Info);
            Log(logImport, "الوجهة: " + dest, Info);

            try
            {
                var files = new List<string>();
                CollectVideos(source, files);

                Log(logImport, "عُثر على " + files.Count + " ملف فيديو.", Info);

                int done = 0;
                int copied = 0;
                long totalBytes = 0;

                foreach (var file in files)
                {
                    try
                    {
                        var info = VideoHelper.GetInfo(file);
                        string targetFolder = dest;

                        if (organize)
                        {
                            targetFolder = Path.Combine(dest,
                                info.CaptureDate.ToString("yyyy"),
                                info.CaptureDate.ToString("MM"),
                                info.CaptureDate.ToString("dd"));
                            if (!Directory.Exists(targetFolder))
                                Directory.CreateDirectory(targetFolder);
                        }

                        string targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                        int counter = 1;
                        while (File.Exists(targetFile))
                        {
                            string nameOnly = Path.GetFileNameWithoutExtension(file);
                            string ext = Path.GetExtension(file);
                            targetFile = Path.Combine(targetFolder, nameOnly + "_" + counter + ext);
                            counter++;
                        }

                        File.Copy(file, targetFile, false);
                        copied++;
                        totalBytes += info.Size;

                        var v = new Video
                        {
                            FilePath = targetFile,
                            FileName = Path.GetFileName(targetFile),
                            FileSize = info.Size,
                            CaptureDate = info.CaptureDate,
                            DurationSeconds = info.DurationSeconds,
                            Camera = info.Camera ?? "",
                            Resolution = info.Resolution ?? "",
                            Fps = info.Fps,
                            ProjectId = projectId,
                            Tags = "",
                            ThumbnailPath = ""
                        };
                        db.AddVideo(v);

                        Log(logImport, "✓ " + Path.GetFileName(targetFile) + "  (" + VideoHelper.FormatSize(info.Size) + ")", Success);
                    }
                    catch (Exception ex)
                    {
                        Log(logImport, "✗ فشل: " + Path.GetFileName(file) + " — " + ex.Message, Error);
                    }

                    done++;
                    int pct = (int)((done * 100L) / Math.Max(1, files.Count));
                    UpdatePb(pbImport, pct);
                }

                Log(logImport, "=====================================", CardBorder);
                Log(logImport, "اكتمل الاستيراد: " + copied + " ملف", Success);
                Log(logImport, "الحجم الكلي: " + VideoHelper.FormatSize(totalBytes), Info);
            }
            catch (Exception ex)
            {
                Log(logImport, "خطأ: " + ex.Message, Error);
            }
            finally
            {
                Invoke((Action)(() =>
                {
                    btnStartImport.Enabled = true;
                    LoadLibrary();
                }));
            }
        }

        void CollectVideos(string folder, List<string> list)
        {
            try
            {
                foreach (var f in Directory.GetFiles(folder))
                    if (VideoHelper.IsVideoFile(f)) list.Add(f);

                foreach (var d in Directory.GetDirectories(folder))
                    CollectVideos(d, list);
            }
            catch { }
        }

        // ================= Library Tab =================
        void BuildLibraryTab(TabPage tab)
        {
            var lblSearch = new Label
            {
                Text = "بحث:", Location = new Point(20, 20), AutoSize = true,
                ForeColor = TextMain, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            txtSearch = new TextBox
            {
                Location = new Point(70, 18), Width = 300,
                BackColor = Color.FromArgb(45, 45, 65), ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F)
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadLibrary(); };

            var lblProj = new Label
            {
                Text = "مشروع:", Location = new Point(390, 20), AutoSize = true,
                ForeColor = TextMain, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            cmbFilterProject = new ComboBox
            {
                Location = new Point(450, 18), Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 65), ForeColor = TextMain,
                Font = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat
            };
            cmbFilterProject.SelectedIndexChanged += (s, e) => LoadLibrary();

            btnRefresh = new RoundButton
            {
                Text = "تحديث", Location = new Point(720, 16),
                Size = new Size(100, 32), Normal = Info,
                Hover = Color.FromArgb(147, 197, 253), Radius = 6
            };
            btnRefresh.Click += (s, e) => LoadLibrary();

            btnOpenFile = new RoundButton
            {
                Text = "فتح الملف", Location = new Point(830, 16),
                Size = new Size(120, 32), Normal = Success,
                Hover = Color.FromArgb(74, 222, 128), Radius = 6
            };
            btnOpenFile.Click += BtnOpenFile_Click;

            btnAddTag = new RoundButton
            {
                Text = "إضافة وسم", Location = new Point(960, 16),
                Size = new Size(110, 32), Normal = Pink,
                Hover = Color.FromArgb(244, 114, 182), Radius = 6
            };
            btnAddTag.Click += BtnAddTag_Click;

            btnDeleteVideo = new RoundButton
            {
                Text = "حذف من المكتبة", Location = new Point(1080, 16),
                Size = new Size(100, 32), Normal = Error,
                Hover = Color.FromArgb(220, 38, 38), Radius = 6
            };
            btnDeleteVideo.Click += BtnDeleteVideo_Click;

            lvVideos = new ListView
            {
                Location = new Point(20, 60), Size = new Size(1150, 500),
                View = View.Details, FullRowSelect = true, GridLines = true,
                BackColor = Color.FromArgb(12, 12, 20), ForeColor = TextMain,
                Font = new Font("Consolas", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lvVideos.Columns.Add("الاسم", 320);
            lvVideos.Columns.Add("التاريخ", 140);
            lvVideos.Columns.Add("المدة", 80);
            lvVideos.Columns.Add("الحجم", 90);
            lvVideos.Columns.Add("الدقة", 100);
            lvVideos.Columns.Add("الكاميرا", 130);
            lvVideos.Columns.Add("المشروع", 150);
            lvVideos.Columns.Add("الوسوم", 140);
            lvVideos.DoubleClick += (s, e) => BtnOpenFile_Click(null, null);

            lblLibStats = new Label
            {
                Text = "", Location = new Point(20, 570), AutoSize = true,
                ForeColor = Info, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            tab.Controls.Add(lblSearch);
            tab.Controls.Add(txtSearch);
            tab.Controls.Add(lblProj);
            tab.Controls.Add(cmbFilterProject);
            tab.Controls.Add(btnRefresh);
            tab.Controls.Add(btnOpenFile);
            tab.Controls.Add(btnAddTag);
            tab.Controls.Add(btnDeleteVideo);
            tab.Controls.Add(lvVideos);
            tab.Controls.Add(lblLibStats);
        }

        void LoadLibrary()
        {
            try
            {
                lvVideos.Items.Clear();

                // Load projects into filter combo
                var projects = db.GetAllProjects();
                string prevSel = cmbFilterProject.SelectedItem != null
                    ? cmbFilterProject.SelectedItem.ToString() : "";
                cmbFilterProject.Items.Clear();
                cmbFilterProject.Items.Add("الكل");
                foreach (var p in projects)
                    cmbFilterProject.Items.Add(p.Id + " - " + p.Name);
                if (!string.IsNullOrEmpty(prevSel))
                {
                    int idx = cmbFilterProject.Items.IndexOf(prevSel);
                    if (idx >= 0) cmbFilterProject.SelectedIndex = idx;
                    else cmbFilterProject.SelectedIndex = 0;
                }
                else cmbFilterProject.SelectedIndex = 0;

                int pid = GetSelectedProjectId(cmbFilterProject);
                string query = txtSearch.Text.Trim();

                var videos = db.SearchVideos(query, pid);

                var projectMap = new Dictionary<int, string>();
                foreach (var p in projects) projectMap[p.Id] = p.Name;

                foreach (var v in videos)
                {
                    string projectName = v.ProjectId > 0 && projectMap.ContainsKey(v.ProjectId)
                        ? projectMap[v.ProjectId] : "—";
                    var lvi = new ListViewItem(new string[]
                    {
                        v.FileName,
                        v.CaptureDate.ToString("yyyy-MM-dd HH:mm"),
                        VideoHelper.FormatDuration(v.DurationSeconds),
                        VideoHelper.FormatSize(v.FileSize),
                        string.IsNullOrEmpty(v.Resolution) ? "—" : v.Resolution,
                        string.IsNullOrEmpty(v.Camera) ? "—" : v.Camera,
                        projectName,
                        string.IsNullOrEmpty(v.Tags) ? "—" : v.Tags
                    });
                    lvi.Tag = v;
                    lvVideos.Items.Add(lvi);
                }

                int totalVideos = db.CountVideos();
                long totalBytes = db.TotalVideoBytes();
                lblLibStats.Text = "إجمالي: " + totalVideos + " فيديو  |  " + VideoHelper.FormatSize(totalBytes) + "  |  معروض: " + videos.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل المكتبة: " + ex.Message);
            }
        }

        int GetSelectedProjectId(ComboBox cmb)
        {
            if (cmb.SelectedItem == null) return 0;
            string s = cmb.SelectedItem.ToString();
            if (s == "الكل" || s.StartsWith("بدون")) return 0;
            int dash = s.IndexOf(" - ");
            if (dash > 0)
            {
                int id;
                if (int.TryParse(s.Substring(0, dash), out id)) return id;
            }
            return 0;
        }

        void BtnOpenFile_Click(object sender, EventArgs e)
        {
            if (lvVideos.SelectedItems.Count == 0) return;
            var v = lvVideos.SelectedItems[0].Tag as Video;
            if (v == null) return;
            try
            {
                if (File.Exists(v.FilePath))
                    Process.Start(new ProcessStartInfo(v.FilePath) { UseShellExecute = true });
                else
                    MessageBox.Show("الملف غير موجود: " + v.FilePath);
            }
            catch (Exception ex) { MessageBox.Show("خطأ: " + ex.Message); }
        }

        void BtnAddTag_Click(object sender, EventArgs e)
        {
            if (lvVideos.SelectedItems.Count == 0) { MessageBox.Show("اختر فيديو."); return; }
            var v = lvVideos.SelectedItems[0].Tag as Video;
            if (v == null) return;

            string newTag = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل الوسم (مثال: Drone، B-Roll، مقابلة):", "إضافة وسم", "");
            if (string.IsNullOrWhiteSpace(newTag)) return;

            string tags = v.Tags ?? "";
            if (tags.Length > 0) tags += ", ";
            tags += newTag.Trim();

            db.UpdateVideoTags(v.Id, tags);
            v.Tags = tags;
            LoadLibrary();
        }

        void BtnDeleteVideo_Click(object sender, EventArgs e)
        {
            if (lvVideos.SelectedItems.Count == 0) return;
            var v = lvVideos.SelectedItems[0].Tag as Video;
            if (v == null) return;

            var res = MessageBox.Show(
                "حذف من المكتبة (لن يُحذف الملف من القرص):\n" + v.FileName + "\n\nمتابعة؟",
                "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            db.DeleteVideo(v.Id);
            LoadLibrary();
        }

        // ================= Projects Tab =================
        void BuildProjectsTab(TabPage tab)
        {
            btnAddProject = new RoundButton
            {
                Text = "مشروع جديد", Location = new Point(20, 20),
                Size = new Size(170, 40), Normal = Accent,
                Hover = Color.FromArgb(139, 92, 246), Radius = 8
            };
            btnAddProject.Click += BtnAddProject_Click;

            btnChangeStatus = new RoundButton
            {
                Text = "تغيير الحالة", Location = new Point(200, 20),
                Size = new Size(150, 40), Normal = Info,
                Hover = Color.FromArgb(147, 197, 253), Radius = 8
            };
            btnChangeStatus.Click += BtnChangeStatus_Click;

            btnDeleteProject = new RoundButton
            {
                Text = "حذف المشروع", Location = new Point(360, 20),
                Size = new Size(150, 40), Normal = Error,
                Hover = Color.FromArgb(220, 38, 38), Radius = 8
            };
            btnDeleteProject.Click += BtnDeleteProject_Click;

            lvProjects = new ListView
            {
                Location = new Point(20, 80), Size = new Size(1150, 480),
                View = View.Details, FullRowSelect = true, GridLines = true,
                BackColor = Color.FromArgb(12, 12, 20), ForeColor = TextMain,
                Font = new Font("Consolas", 9.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lvProjects.Columns.Add("المشروع", 400);
            lvProjects.Columns.Add("العميل", 250);
            lvProjects.Columns.Add("الحالة", 180);
            lvProjects.Columns.Add("التاريخ", 180);
            lvProjects.Columns.Add("عدد المقاطع", 120);

            lblProjStats = new Label
            {
                Text = "", Location = new Point(20, 570), AutoSize = true,
                ForeColor = Info, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            tab.Controls.Add(btnAddProject);
            tab.Controls.Add(btnChangeStatus);
            tab.Controls.Add(btnDeleteProject);
            tab.Controls.Add(lvProjects);
            tab.Controls.Add(lblProjStats);
        }

        void LoadProjects()
        {
            lvProjects.Items.Clear();
            var projects = db.GetAllProjects();
            var clients = db.GetAllClients();
            var clientMap = new Dictionary<int, string>();
            foreach (var c in clients) clientMap[c.Id] = c.Name;

            int totalVideos = 0;
            foreach (var p in projects)
            {
                var vids = db.GetVideosByProject(p.Id);
                totalVideos += vids.Count;
                string clientName = p.ClientId > 0 && clientMap.ContainsKey(p.ClientId)
                    ? clientMap[p.ClientId] : "—";
                var lvi = new ListViewItem(new string[]
                {
                    p.Name,
                    clientName,
                    p.Status,
                    p.CreatedDate.ToString("yyyy-MM-dd"),
                    vids.Count.ToString()
                });
                lvi.Tag = p;
                if (p.Status == "مدفوع") lvi.ForeColor = Success;
                else if (p.Status == "ملغي") lvi.ForeColor = Error;
                else if (p.Status == "تم التسليم") lvi.ForeColor = Info;
                else lvi.ForeColor = TextMain;
                lvProjects.Items.Add(lvi);
            }
            lblProjStats.Text = "عدد المشاريع: " + projects.Count + "  |  إجمالي المقاطع المرتبطة: " + totalVideos;
        }

        void BtnAddProject_Click(object sender, EventArgs e)
        {
            var clients = db.GetAllClients();
            if (clients.Count == 0)
            {
                MessageBox.Show("أضف عميلًا أولًا من تبويب العملاء.");
                return;
            }

            string name = Microsoft.VisualBasic.Interaction.InputBox("اسم المشروع:", "مشروع جديد", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            // اختر العميل
            var names = new List<string>();
            foreach (var c in clients) names.Add(c.Name);
            string clientPick = Microsoft.VisualBasic.Interaction.InputBox(
                "اكتب اسم العميل بالضبط:\n" + string.Join(" | ", names), "العميل", names[0]);
            int clientId = clients[0].Id;
            foreach (var c in clients)
                if (c.Name == clientPick) { clientId = c.Id; break; }

            string desc = Microsoft.VisualBasic.Interaction.InputBox("وصف (اختياري):", "وصف", "");

            db.AddProject(new Project
            {
                Name = name,
                ClientId = clientId,
                Status = "قيد التصوير",
                Description = desc
            });

            LoadProjects();
            LoadImportProjects();
        }

        void BtnChangeStatus_Click(object sender, EventArgs e)
        {
            if (lvProjects.SelectedItems.Count == 0) return;
            var p = lvProjects.SelectedItems[0].Tag as Project;
            if (p == null) return;

            string statuses = string.Join(" | ", VideoHelper.ProjectStatuses);
            string pick = Microsoft.VisualBasic.Interaction.InputBox(
                "اختر الحالة:\n" + statuses, "تغيير الحالة", p.Status);
            if (string.IsNullOrWhiteSpace(pick)) return;

            db.UpdateProjectStatus(p.Id, pick);
            LoadProjects();
        }

        void BtnDeleteProject_Click(object sender, EventArgs e)
        {
            if (lvProjects.SelectedItems.Count == 0) return;
            var p = lvProjects.SelectedItems[0].Tag as Project;
            if (p == null) return;

            var res = MessageBox.Show("حذف المشروع: " + p.Name + "؟\n(لن تُحذف المقاطع)",
                "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            db.DeleteProject(p.Id);
            LoadProjects();
            LoadImportProjects();
        }

        // ================= Clients Tab =================
        void BuildClientsTab(TabPage tab)
        {
            btnAddClient = new RoundButton
            {
                Text = "عميل جديد", Location = new Point(20, 20),
                Size = new Size(170, 40), Normal = Accent,
                Hover = Color.FromArgb(139, 92, 246), Radius = 8
            };
            btnAddClient.Click += BtnAddClient_Click;

            btnDeleteClient = new RoundButton
            {
                Text = "حذف العميل", Location = new Point(200, 20),
                Size = new Size(150, 40), Normal = Error,
                Hover = Color.FromArgb(220, 38, 38), Radius = 8
            };
            btnDeleteClient.Click += BtnDeleteClient_Click;

            lvClients = new ListView
            {
                Location = new Point(20, 80), Size = new Size(1150, 500),
                View = View.Details, FullRowSelect = true, GridLines = true,
                BackColor = Color.FromArgb(12, 12, 20), ForeColor = TextMain,
                Font = new Font("Consolas", 9.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lvClients.Columns.Add("الاسم", 350);
            lvClients.Columns.Add("الهاتف", 200);
            lvClients.Columns.Add("البريد", 300);
            lvClients.Columns.Add("تاريخ الإضافة", 180);
            lvClients.Columns.Add("عدد المشاريع", 120);

            tab.Controls.Add(btnAddClient);
            tab.Controls.Add(btnDeleteClient);
            tab.Controls.Add(lvClients);
        }

        void LoadClients()
        {
            lvClients.Items.Clear();
            var clients = db.GetAllClients();
            var projects = db.GetAllProjects();

            var countByClient = new Dictionary<int, int>();
            foreach (var p in projects)
            {
                if (!countByClient.ContainsKey(p.ClientId)) countByClient[p.ClientId] = 0;
                countByClient[p.ClientId]++;
            }

            foreach (var c in clients)
            {
                int projCount = countByClient.ContainsKey(c.Id) ? countByClient[c.Id] : 0;
                var lvi = new ListViewItem(new string[]
                {
                    c.Name,
                    string.IsNullOrEmpty(c.Phone) ? "—" : c.Phone,
                    string.IsNullOrEmpty(c.Email) ? "—" : c.Email,
                    c.CreatedDate.ToString("yyyy-MM-dd"),
                    projCount.ToString()
                });
                lvi.Tag = c;
                lvClients.Items.Add(lvi);
            }
        }

        void BtnAddClient_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("اسم العميل:", "عميل جديد", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            string phone = Microsoft.VisualBasic.Interaction.InputBox("الهاتف:", "عميل جديد", "");
            string email = Microsoft.VisualBasic.Interaction.InputBox("البريد الإلكتروني:", "عميل جديد", "");

            db.AddClient(new Client { Name = name, Phone = phone, Email = email });
            LoadClients();
        }

        void BtnDeleteClient_Click(object sender, EventArgs e)
        {
            if (lvClients.SelectedItems.Count == 0) return;
            var c = lvClients.SelectedItems[0].Tag as Client;
            if (c == null) return;

            var res = MessageBox.Show("حذف العميل: " + c.Name + "؟", "تأكيد",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res != DialogResult.Yes) return;

            db.DeleteClient(c.Id);
            LoadClients();
        }

        // ================= Reports Tab =================
        void BuildReportsTab(TabPage tab)
        {
            btnGenerateReport = new RoundButton
            {
                Text = "توليد التقرير", Location = new Point(20, 20),
                Size = new Size(180, 40), Normal = Accent,
                Hover = Color.FromArgb(139, 92, 246), Radius = 8
            };
            btnGenerateReport.Click += (s, e) => GenerateReport();

            btnExportReport = new RoundButton
            {
                Text = "حفظ كملف نصي", Location = new Point(210, 20),
                Size = new Size(180, 40), Normal = Teal,
                Hover = Color.FromArgb(45, 212, 191), Radius = 8
            };
            btnExportReport.Click += BtnExportReport_Click;

            txtReport = new RichTextBox
            {
                Location = new Point(20, 80), Size = new Size(1150, 500),
                BackColor = Color.FromArgb(12, 12, 20), ForeColor = TextMain,
                Font = new Font("Consolas", 10F), ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle, ScrollBars = RichTextBoxScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                RightToLeft = RightToLeft.No
            };

            tab.Controls.Add(btnGenerateReport);
            tab.Controls.Add(btnExportReport);
            tab.Controls.Add(txtReport);
        }

        void GenerateReport()
        {
            txtReport.Clear();
            AppendReport("========================================", Info);
            AppendReport("  تقرير Reel Manager", Info);
            AppendReport("  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Info);
            AppendReport("========================================", Info);
            AppendReport("", TextMain);

            // إحصائيات عامة
            int totalVideos = db.CountVideos();
            long totalBytes = db.TotalVideoBytes();
            var projects = db.GetAllProjects();
            var clients = db.GetAllClients();

            AppendReport("الإحصائيات العامة:", Warning);
            AppendReport("  عدد المقاطع: " + totalVideos, TextMain);
            AppendReport("  الحجم الكلي: " + VideoHelper.FormatSize(totalBytes), TextMain);
            AppendReport("  عدد المشاريع: " + projects.Count, TextMain);
            AppendReport("  عدد العملاء: " + clients.Count, TextMain);
            AppendReport("", TextMain);

            // المشاريع
            AppendReport("تفاصيل المشاريع:", Warning);
            var clientMap = new Dictionary<int, string>();
            foreach (var c in clients) clientMap[c.Id] = c.Name;

            foreach (var p in projects)
            {
                var vids = db.GetVideosByProject(p.Id);
                long bytes = 0;
                double secs = 0;
                foreach (var v in vids) { bytes += v.FileSize; secs += v.DurationSeconds; }

                string cname = p.ClientId > 0 && clientMap.ContainsKey(p.ClientId)
                    ? clientMap[p.ClientId] : "—";

                AppendReport("  • " + p.Name + " [" + p.Status + "]", Info);
                AppendReport("      العميل: " + cname, TextMain);
                AppendReport("      المقاطع: " + vids.Count + "  |  الحجم: " + VideoHelper.FormatSize(bytes)
                    + "  |  المدة: " + VideoHelper.FormatDuration(secs), TextMain);
            }
            AppendReport("", TextMain);

            // العملاء
            AppendReport("العملاء:", Warning);
            foreach (var c in clients)
            {
                int pc = 0;
                foreach (var p in projects) if (p.ClientId == c.Id) pc++;
                AppendReport("  • " + c.Name + "  (" + pc + " مشروع)  — " + c.Phone, TextMain);
            }
        }

        void AppendReport(string text, Color color)
        {
            if (txtReport.InvokeRequired) { txtReport.Invoke((Action)(() => AppendReport(text, color))); return; }
            txtReport.SelectionStart = txtReport.TextLength;
            txtReport.SelectionLength = 0;
            txtReport.SelectionColor = color;
            txtReport.AppendText(text + Environment.NewLine);
            txtReport.SelectionColor = txtReport.ForeColor;
            txtReport.ScrollToCaret();
        }

        void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReport.Text)) { MessageBox.Show("ولّد التقرير أولًا."); return; }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files|*.txt";
                sfd.FileName = "reel-manager-report-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, txtReport.Text, System.Text.Encoding.UTF8);
                    MessageBox.Show("تم الحفظ: " + sfd.FileName);
                }
            }
        }

        // ================= Helpers =================
        void LoadEverything()
        {
            LoadImportProjects();
            LoadLibrary();
            LoadProjects();
            LoadClients();
        }

        void LoadImportProjects()
        {
            if (cmbImportProject == null) return;
            cmbImportProject.Items.Clear();
            cmbImportProject.Items.Add("بدون مشروع");
            foreach (var p in db.GetAllProjects())
                cmbImportProject.Items.Add(p.Id + " - " + p.Name);
            cmbImportProject.SelectedIndex = 0;
        }

        void Log(RichTextBox box, string msg, Color color)
        {
            if (box.InvokeRequired) { box.Invoke((Action)(() => Log(box, msg, color))); return; }
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(msg + Environment.NewLine);
            box.SelectionColor = box.ForeColor;
            box.ScrollToCaret();
        }

        void UpdatePb(ProgressBar pb, int pct)
        {
            if (pb.InvokeRequired) { pb.Invoke((Action)(() => UpdatePb(pb, pct))); return; }
            pb.Value = Math.Min(100, Math.Max(0, pct));
        }
    }
}
