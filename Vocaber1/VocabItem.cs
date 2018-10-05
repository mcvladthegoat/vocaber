using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Vocaber1
{
    [Serializable]
    public class VocabItem
    {
        string oldValue, newValue;
        public bool isRaw, isValid;
        int position;

        #region Properties.

        public int No
        { get { return this.position + 1; } set { this.position = value; } }

        public string OldValue
        { get { return this.oldValue; } set { this.oldValue = value; } }

        public string NewValue
        { get { return this.newValue; } set { this.newValue = value; } }

        #endregion

        public VocabItem()
        {

        }

        public VocabItem(string oldValue, string newValue = "", int position = 0)
        {
            this.oldValue = oldValue;
            this.newValue = newValue.Trim();
            this.position = position;
            this.isRaw = true;

            if (oldValue == newValue || newValue.Trim() == "")
            {
                this.isValid = false;
            } 
            else {
                this.isValid = true;
            }
        }

        public VocabItem(string oldValue, string newValue, int position, bool isRaw, bool isValid)
        {
            this.oldValue = oldValue;
            this.newValue = newValue.Trim();
            this.position = position;
            this.isRaw = isRaw;
            this.isValid = isValid;
        }
    }
}
