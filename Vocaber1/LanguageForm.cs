using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vocaber1
{
    public partial class LanguageForm : Form
    {
        public LanguageForm()
        {
            InitializeComponent();
        }

        private void LanguageForm_Load(object sender, EventArgs e)
        {
            List<KeyValuePair<string, string>> languageList = new List<KeyValuePair<string, string>>();
            String languagesRaw = Vocaber1.Properties.Settings.Default.Languages;
            JArray languageParsed = JArray.Parse(languagesRaw);
            foreach (JObject item in languageParsed)
            {
                languageList.Add(new KeyValuePair<string, string>((string)item["lang_code"], (string)item["lang_name"]));
            }
            comboBoxLangFrom.DataSource = null; comboBoxLangTo.DataSource = null;
            comboBoxLangFrom.Items.Clear(); comboBoxLangTo.Items.Clear();

            comboBoxLangFrom.DataSource = new BindingSource(languageList, null);
            comboBoxLangTo.DataSource = new BindingSource(languageList, null);

            comboBoxLangFrom.DisplayMember = "Value"; comboBoxLangTo.DisplayMember = "Value";
            comboBoxLangFrom.ValueMember = "Key"; comboBoxLangTo.ValueMember = "Key";

            comboBoxLangFrom.SelectedIndex = 0; comboBoxLangTo.SelectedIndex = 1;
            System.Console.WriteLine(languagesRaw);
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (comboBoxLangFrom.SelectedIndex == comboBoxLangTo.SelectedIndex)
            {
                MessageBox.Show("Error! Source and Target language can't be the same!");
                return;
            }
            Form1 mainForm = (Form1)this.Owner;
            mainForm.langFrom = comboBoxLangFrom.SelectedValue.ToString();
            mainForm.langTo = comboBoxLangTo.SelectedValue.ToString();

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
