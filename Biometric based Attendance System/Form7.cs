using BAL;
using SourceAFIS.Simple;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{    
    public partial class Form7 : Form
    {
        BLayer.SelectClass selectClassObj = new BLayer.SelectClass();

        static int count = 0;
        static AfisEngine afis = new AfisEngine();
        IEnumerable<Person> candidates = new List<Person>();
        List<Person> cand = new List<Person>();

        public Form7()
        {
            InitializeComponent();
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            drp_select_class.Items.Clear();
            DataSet ds = selectClassObj.get_class();
            ds.Tables[0].DefaultView.Sort = "class_name";
            drp_select_class.DataSource = ds.Tables[0];
            drp_select_class.DisplayMember = "class_name";
            drp_select_class.ValueMember = "class_id";
        }

        private void btn_scan_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            btn_cmp.Enabled = true;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Load(openFileDialog1.FileName);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btn_cmp_Click(object sender, EventArgs e)
        {
            Fingerprint fp2 = new Fingerprint();
            fp2.AsBitmap = new Bitmap(Bitmap.FromFile(pictureBox1.ImageLocation));
            Person p2 = new Person();
            p2.Fingerprints.Add(fp2);
            afis.Extract(p2);

            Person ans = afis.Identify(p2, candidates).FirstOrDefault();
            bool match = (ans != null);
            if (match)
            {
                DataSet ds = selectClassObj.get_enroll_no_by_id(ans.Id);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    count++;
                    List<Fingerprint> li = ans.Fingerprints;
                    foreach (Fingerprint f in li)
                    {
                        Bitmap bitmp = new Bitmap(f.AsBitmap);
                        pictureBox2.Image = bitmp;
                        pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox2.Refresh();
                    }
                    lbl_result.Text = "Matched";
                    lbl_result.ForeColor = Color.Green;

                    lbl_result.Text += " " + ds.Tables[0].Rows[0]["enroll_no"];
                }
                else
                {
                    pictureBox2.Image = null;
                    lbl_result.Text = "Not Matched";
                    lbl_result.ForeColor = Color.Red;
                }
            }
            else
            {
                pictureBox2.Image = null;
                lbl_result.Text = "Not Matched";
                lbl_result.ForeColor = Color.Red;
            }
            MessageBox.Show("\nCount: " + count);
        }

        public void run(Object f, string path)
        {
            FileInfo fi = (FileInfo)f;
            //Console.WriteLine("Thread " + fi.Name);
            Fingerprint fp1 = new Fingerprint();
            fp1.AsBitmap = new Bitmap(Bitmap.FromFile(path + "\\" + fi.Name));
            Person p1 = new Person();
            p1.Fingerprints.Add(fp1);
            DataSet ds = selectClassObj.get_id_by_enroll_no(Path.GetFileNameWithoutExtension(fi.Name));
            if (ds.Tables[0].Rows.Count > 0)
            {
                p1.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["stu_id"]);
            }
            afis.Extract(p1);
            cand.Add(p1);
        }



        private void btn_select_class_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;
            Stopwatch sw = Stopwatch.StartNew();
            string path = "../../Lib/Images/" + drp_select_class.SelectedValue;
            DirectoryInfo d = new DirectoryInfo(path);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = d.GetFiles().Length;
            foreach (FileInfo f in d.GetFiles())
            {
                if (f.Extension == ".jpeg" || f.Extension == ".jpg" || f.Extension == ".png")
                {
                    Thread t1 = new Thread(delegate()
                    {
                        run(f, path);
                    });
                    t1.Start();
                    t1.Join();
                    progressBar1.PerformStep();
                }
            }
            candidates = cand;
            sw.Stop();
            label1.Text = sw.ElapsedMilliseconds.ToString() + " Milliseconds";
            drp_select_class.Enabled = false;
            btn_scan.Enabled = true;
            progressBar1.Visible = false;
            btn_select_class.Enabled = false;
        }

        private void Form7_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            MessageBox.Show("You can't close this application");
        }
    }
}