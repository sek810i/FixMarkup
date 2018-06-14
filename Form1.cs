using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FixMarkup
{
    public partial class Form1 : Form
    {
        public void AddLog(string log)
        {
            Invoke(new Action(() => { LogText.AppendText(DateTime.Now + " :  " + log + "\n") ; }));
        }

        public void AddLog(string log, bool mask)
        {
            Invoke(new Action(() =>
            {
                LogText.SelectionStart = LogText.TextLength;
                LogText.SelectionLength = 0;
                LogText.SelectionColor = Color.OrangeRed;
                LogText.AppendText(DateTime.Now + " :  " + log + "\n");
                LogText.SelectionColor = LogText.ForeColor;

            }));
        }
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            using (new OpenFileDialog())
            {
                openFileDialog1.InitialDirectory = TextDBPath.Text;
                openFileDialog1.RestoreDirectory = false;
                openFileDialog1.ShowDialog();
            }
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            TextDBPath.Text = openFileDialog1.FileName;
            var task = new Task(() => { process(openFileDialog1.FileName); });
            task.Start();
        }
        private void process(string basename)
        {
            if (checkBox4.Checked)
            {
                var bdconn = new DbConn { dataBasePath = basename };
                var main = new List<Records>();
                var task = new Task(() => { main = LinkProv.DelPers(this, basename); });
                task.Start();
                task.Wait();
                AddLog("Обновляем базу");
                task = new Task(() => { bdconn.Updatef1(main, this); });
                task.Start();
                task.Wait();

            }
            if (checkBox2.Checked)
            {
                var task = new Task(() => { LinkProv.LinkProvFunc(this, basename); });
                task.Start();
                task.Wait();
            }
            if (checkBox1.Checked)
            {
                var records = new List<Records>();
                var bdconn = new DbConn { dataBasePath = basename };
                AddLog(@"Загрузка базы");
                var task = new Task(delegate { records = bdconn.ReadDb(); });
                task.Start();
                var timer = new Timer { Interval = 30000 };
                timer.Tick += new EventHandler(timer_Tick);
                timer.Enabled = true;
                task.Wait();
                timer.Enabled = false;
                AddLog(@"База загружена");
                task = new Task(() => { records = Records.ParsePerson(records); });
                task.Start();
                AddLog(@"Парсим персоналии");
                task.Wait();
                var count = 0;
                task = new Task(() => { records = Records.ProvMarkup(records, ref count, checkBox3.Checked, this); });
                AddLog(@"Проверяем разметку");
                task.Start();
                task.Wait();
                AddLog("Нашлось " + count.ToString() + " подозрительных");
                task = new Task(() => { bdconn.Updatef1(records, this); });
                task.Start();
                task.Wait();
            }
            if (checkBox1.Checked == false && checkBox2.Checked == false && checkBox4.Checked == false)
            {
                MessageBox.Show("Ни одна из задач не выбрана");
            }
            else
            {
                MessageBox.Show("Готово!");
            }
        }
        private void timer_Tick(object Sender, EventArgs e)
        {
            AddLog("Всё ещё гружу....");
        }
    }
}
