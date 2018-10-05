using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Vocaber1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            vocab = new List<VocabItem>();
            badvocab = new List<VocabItem>();
            totalLines = 0;
            this.apiKeys = new List<string>();
            this.RefreshAPIKeys();
            yt = new YandexTranslator(this.apiKeys);
            //infoForm = new InfoForm();
            this.projectPath = "";
            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgWorker_ProgressChanged);
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.WorkerReportsProgress = true;
        }
        //InfoForm infoForm;
        YandexTranslator yt;
        List<string> apiKeys;
        List<VocabItem> vocab;
        List<VocabItem> badvocab;
        public string _projectPath, importFileName;
        BackgroundWorker bgWorker;
        public int totalLines;

        public String projectPath { get { return this._projectPath; } set { this._projectPath = value; this.toolStripStatusLabel2.Text = value; } }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void IndentRows()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle();

                for (int i = 0; i < this.vocab.Count; i++)
                {
                    if (!this.vocab[i].isValid)
                    {
                        this.IndentRowById(i, "notvalid");
                    }
                    else if (this.vocab[i].isRaw){
                        this.IndentRowById(i, "raw");
                    }
                    else {
                        this.IndentRowById(i, "valid");
                    }
                }
                //dataGridViewMain.Rows[e.RowIndex].DefaultCellStyle = style;

            
            //style.Font = new Font(dataGridViewMain.Font, FontStyle.Italic);
            //for (int i = 0; i < this.badvocab.Count; i++)
            //{
            //    //DataGridViewRow c = dataGridViewMain.Rows[i];
            //    dataGridViewMain.Rows[this.badvocab[i].No-1].DefaultCellStyle = style;
            //}
        }

        public void IndentRowById(int rowId, string mode)
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle();
            switch (mode)
            {
                case "valid":
                case "notraw":
                    style.Font = new Font(dataGridViewMain.Font, FontStyle.Strikeout);
                    break;
                case "notvalid":
                    style.Font = new Font(dataGridViewMain.Font, FontStyle.Italic);
                    style.BackColor = Color.PaleVioletRed;
                    break;
                case "raw":
                    style.Font = new Font(dataGridViewMain.Font, FontStyle.Bold);
                    style.BackColor = Color.Wheat;
                    break;
                default:
                    style.Font = new Font(dataGridViewMain.Font, FontStyle.Regular);
                    break;
            }
            dataGridViewMain.Rows[rowId].DefaultCellStyle = style;
            dataGridViewMain.Columns["No"].ReadOnly = true;
            dataGridViewMain.Columns["OldValue"].ReadOnly = true;
        }

        public void UpdateBadVocabluary()
        {
            badvocab.Clear(); badvocab = new List<VocabItem>();

            for (int i = 0; i < vocab.Count; i++)
            {
                System.Console.WriteLine(vocab[i].isValid);
                if (!vocab[i].isValid)
                {
                    badvocab.Add(vocab[i]);
                }
            }
            dataGridViewBad.DataSource = badvocab;
            dataGridViewBad.Columns["No"].ReadOnly = true;
            dataGridViewBad.Columns["OldValue"].ReadOnly = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a TXT or similar file";
            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .CUR file was selected, open it.  
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vocab.Clear(); badvocab.Clear();
                vocab = new List<VocabItem>(); badvocab = new List<VocabItem>();

                XmlSerializer formatter = new XmlSerializer(typeof(VocabItem[]));
                using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.OpenOrCreate))
                {
                    VocabItem[] tmp = (VocabItem[])formatter.Deserialize(fs);
                    int counter = 0;
                    foreach (VocabItem p in tmp)
                    {
                        vocab.Add(new VocabItem(p.OldValue, p.NewValue, counter, p.isRaw, p.isValid));
                        counter++;
                    }
                    this.UpdateBadVocabluary();
                }
                dataGridViewMain.DataSource = vocab;
                dataGridViewBad.DataSource = badvocab;
                projectPath = openFileDialog1.FileName;
                this.UpdateStats();
                this.IndentRows();
                dataGridViewMain.Columns["No"].ReadOnly = true;
                dataGridViewMain.Columns["OldValue"].ReadOnly = true;
                dataGridViewBad.Columns["No"].ReadOnly = true;
                dataGridViewBad.Columns["OldValue"].ReadOnly = true;

                dataGridViewBad.Columns["OldValue"].HeaderText = "Original";
                dataGridViewBad.Columns["NewValue"].HeaderText = "Translated";
                dataGridViewMain.Columns["OldValue"].HeaderText = "Original";
                dataGridViewMain.Columns["NewValue"].HeaderText = "Translated";
                //this.IndentRowById(1, "raw");
            }
        }

        private void dataGridViewMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //int a;
            //DataGridViewCellStyle style = new DataGridViewCellStyle();
            //foreach (DataGridViewColumn c in dataGridViewMain.Columns)
            //{
            //    c.DefaultCellStyle.Font = new Font("Arial Black", 16.5F, GraphicsUnit.Pixel);
            //}
            //style.Font = new Font(dataGridViewMain.Font, FontStyle.Bold);
            //dataGridViewMain.Rows[e.RowIndex].DefaultCellStyle = style;
            //this.vocab[e.RowIndex].NewValue = "1239219321983213";
        }

        private void dataGridViewMain_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int rowId = dataGridViewMain.CurrentCell.RowIndex;
                if (e.KeyValue == 113) //F2 - VALID - NOT RAW
                {
                    this.IndentRowById(rowId, "valid");
                    this.vocab[rowId].isValid = true;
                    this.vocab[rowId].isRaw = false;
                    //// set isValid = true
                    //int curindex = dataGridViewMain.CurrentCell.RowIndex + 1;
                    //dataGridViewMain.Rows[curindex].Selected = false;
                    //curindex = (this.vocab.Count > curindex) ? curindex + 1 : curindex;
                    ////dataGridViewMain.SelectedRows.Clear();
                    //dataGridViewMain.Rows[curindex].Selected = true;
                }
                if (e.KeyValue == 114) //F3 - RAW (need to check)
                {
                    this.IndentRowById(rowId, "raw");
                    this.vocab[rowId].isRaw = true;
                    this.vocab[rowId].isValid = true;
                }
                if (e.KeyValue == 115) //F4 - NOT VALID
                {
                    this.IndentRowById(rowId, "notvalid");
                    this.vocab[rowId].isValid = false;
                    this.vocab[rowId].isRaw = false;
                }
                this.UpdateBadVocabluary();
                this.UpdateStats();
                int b;
            }
            catch (Exception ex)
            {

            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            // получаем выбранный файл
            this.projectPath = saveFileDialog1.FileName;

            StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.Unicode);
            for(int i = 0; i < this.vocab.Count; i++)
            {
                sw.WriteLine(this.vocab[i].NewValue);
            }
            sw.Close();
            MessageBox.Show("Successfully exported " + this.vocab.Count + " words");
        }


        void bgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.toolStripStatusLabel3.Text = e.ProgressPercentage.ToString() + " / " + this.totalLines;   
        }

        void bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(this.importFileName);
            int counter = 0;
            string oldValue, newValue, line;

            while ((line = sr.ReadLine()) != null)
            {
                oldValue = line.Trim();
                newValue = yt.Translate(oldValue, "en-ru");
                vocab.Add(new VocabItem(oldValue, newValue, counter));
                System.Console.WriteLine(line);
                System.Console.WriteLine(newValue);
                counter++;
                bgWorker.ReportProgress(counter);
            }

            for (int i = 0; i < vocab.Count; i++)
            {
                System.Console.WriteLine(vocab[i].isValid);
                if (!vocab[i].isValid)
                {
                    badvocab.Add(vocab[i]);
                }
            }
            sr.Close();
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //this.UpdateBadVocabluary();
            dataGridViewMain.DataSource = vocab;
            dataGridViewBad.DataSource = badvocab; 
            this.UpdateStats();
            this.IndentRows();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a TXT or similar file";
            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .CUR file was selected, open it.  
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Start translating
                vocab = new List<VocabItem>(); badvocab = new List<VocabItem>();

                //InfoForm infoForm = new InfoForm();
                //infoForm.ShowDialog();
                this.importFileName = openFileDialog1.FileName;
                this.totalLines = File.ReadLines(this.importFileName).Count();
                bgWorker.RunWorkerAsync();
                //yt.Translate(inputTextBox.Text, 'en-ru');
                // Assign the cursor in the Stream to the Form's Cursor property.  
                //this.Cursor = new Cursor(openFileDialog1.OpenFile());
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this._projectPath.Length == 0)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                // получаем выбранный файл
                this.projectPath= saveFileDialog1.FileName;
            }
            // сохраняем текст в файл
            VocabItem[] tmp;
            tmp = this.vocab.ToArray();
            XmlSerializer formatter = new XmlSerializer(typeof(VocabItem[]));
            //File.Delete(projectPath);
            using (FileStream fs = new FileStream(projectPath, FileMode.Create))
            {
                formatter.Serialize(fs, tmp);
            }
            MessageBox.Show("Project saved");
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            // получаем выбранный файл
            string filename = saveFileDialog1.FileName;
            // сохраняем текст в файл


            VocabItem[] tmp;
            tmp = this.vocab.ToArray();
            XmlSerializer formatter = new XmlSerializer(typeof(VocabItem[]));
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, tmp);
            }
            MessageBox.Show("Project saved in new copy");
        }

        public void UpdateStats()
        {
            int rawcounter = 0;
            for (int i = 0; i < this.vocab.Count; i++)
            {
                if (this.vocab[i].isRaw)
                {
                    rawcounter++;
                }
            }
            this.totalLabel.Text = "Total: " + this.vocab.Count.ToString();
            this.badLabel.Text = "Bad: " + this.badvocab.Count.ToString();
            this.rawLabel.Text = "Raw: " + rawcounter.ToString();
            this.validLabel.Text = "Valid: " + (this.vocab.Count - rawcounter - this.badvocab.Count).ToString();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void RefreshAPIKeys(string filepath = "apikeys.txt")
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(filepath);
            int counter = 0; string line;
            this.apiKeys.Clear(); this.apiKeys = new List<string>();

            while ((line = sr.ReadLine()) != null)
            {
                this.apiKeys.Add(line.Trim());
            }
        }

        private void loadAPIKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewBad_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int rowId = this.badvocab[dataGridViewBad.CurrentCell.RowIndex].No-1;
                if (e.KeyValue == 113) //F2 - VALID - NOT RAW
                {
                    this.IndentRowById(rowId, "valid");
                    this.vocab[rowId].isValid = true;
                    this.vocab[rowId].isRaw = false;
                    //// set isValid = true
                    //int curindex = dataGridViewMain.CurrentCell.RowIndex + 1;
                    //dataGridViewMain.Rows[curindex].Selected = false;
                    //curindex = (this.vocab.Count > curindex) ? curindex + 1 : curindex;
                    ////dataGridViewMain.SelectedRows.Clear();
                    //dataGridViewMain.Rows[curindex].Selected = true;
                }
                if (e.KeyValue == 114) //F3 - RAW (need to check)
                {
                    this.IndentRowById(rowId, "raw");
                    this.vocab[rowId].isRaw = true;
                    this.vocab[rowId].isValid = true;
                }
                if (e.KeyValue == 115) //F4 - NOT VALID
                {
                    this.IndentRowById(rowId, "notvalid");
                    this.vocab[rowId].isValid = false;
                    this.vocab[rowId].isRaw = false;
                }
                this.UpdateBadVocabluary();
                this.UpdateStats();
                int b;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
