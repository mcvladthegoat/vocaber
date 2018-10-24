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
using System.Text.RegularExpressions;

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
            this.projectPath = "";
            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgWorker_ProgressChanged);
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.WorkerReportsProgress = true;
            this.isPlainProject = true;
        }
        YandexTranslator yt;
        List<string> apiKeys;
        List<VocabItem> vocab, badvocab;
        public string _projectPath, importFileName;
        BackgroundWorker bgWorker;
        public int totalLines;
        public bool isPlainProject;

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
            }
        }

        private void dataGridViewMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
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
                if (e.KeyValue == 116) //F5 - COPY OLD AND VERIFY
                {
                    this.IndentRowById(rowId, "valid");
                    this.vocab[rowId].isValid = true;
                    this.vocab[rowId].isRaw = false;
                    this.vocab[rowId].NewValue = this.vocab[rowId].OldValue;
                }
                this.UpdateBadVocabluary();
                this.UpdateStats();
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
            dataGridViewMain.DataSource = vocab;
            dataGridViewBad.DataSource = badvocab; 
            this.UpdateStats();
            this.IndentRows();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a TXT or similar file";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Start translating
                vocab = new List<VocabItem>(); badvocab = new List<VocabItem>();
                this.importFileName = openFileDialog1.FileName;
                this.totalLines = File.ReadLines(this.importFileName).Count();
                bgWorker.RunWorkerAsync();
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
            // сохран

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

        private void toolStripSeparator2_Click(object sender, EventArgs e)
        {

        }

        //Structured import
        private void importStructuredVocabularyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.isPlainProject = true;

        }

        private void autoVerifyByPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string regpattern = Microsoft.VisualBasic.Interaction.InputBox("Your pattern", "Type pattern", "\\b.{1,}[{]");
            if (regpattern.Length == 0)
            {
                return;
            }
            Regex rx = new Regex(@regpattern);
            int counter = 0;
            for(int i = 0; i < this.vocab.Count; i++)
            {
                Match match = rx.Match(this.vocab[i].OldValue);
                if (match.Success)
                {
                    this.IndentRowById(i, "valid");
                    this.vocab[i].NewValue = this.vocab[i].OldValue;
                    this.vocab[i].isValid = true;
                    this.vocab[i].isRaw = false;
                    counter++;
                }
            }
            this.UpdateBadVocabluary();
            this.UpdateStats();
            MessageBox.Show("Verifying finished. Total lines: " + counter.ToString());
        }

        private void mergeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Select a TXT or similar file";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                // получаем выбранный файл
                StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.Unicode);
                string line, resultFilePath = saveFileDialog1.FileName;
                int lineCounter = 0;
                foreach(string sourceFilePath in openFileDialog1.FileNames)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(sourceFilePath);
                    while ((line = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(line);
                        lineCounter++;
                    }
                    sr.Close();
                }

                sw.Close();
                MessageBox.Show("Successfully merged. Total lines: " + lineCounter.ToString());
            }
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
                if (e.KeyValue == 116) //F4 - NOT VALID
                {
                    this.IndentRowById(rowId, "notvalid");
                    this.vocab[rowId].isValid = false;
                    this.vocab[rowId].isRaw = false;
                }
                //if (e.KeyValue == 117) //F5 - COPY OLD AND VERIFY
                //{
                //    this.IndentRowById(rowId, "valid");
                //    this.vocab[rowId].isValid = true;
                //    this.vocab[rowId].isRaw = false;
                //    this.vocab[rowId].NewValue = this.vocab[rowId].OldValue;
                //}
                this.UpdateBadVocabluary();
                this.UpdateStats();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
