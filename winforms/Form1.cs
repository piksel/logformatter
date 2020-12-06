using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogFormatter.Parser;
using Piksel.LogFormatter.Parser;

namespace LogFormatter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void bConvert_Click(object sender, EventArgs e)
        {
            var parser = new LogFmtParser(tbSource.Text);
            if (parser.TryParse())
            {
                var items = parser.GetItems();
                tbResult.Clear();
                
                tbError.Text = $"Parsed {items.Count()} items in {parser.Row} row(s)";

                
                foreach (var item in items)
                {
                    tbResult.AppendText(item.FormatRow(textBox1.Text, (int)numericUpDown1.Value) + Environment.NewLine);
                }
            }
            else
            {
                tbError.Text = parser.ParseError?.ToString() ?? "Unknown error";
            }
        }
    }
}
