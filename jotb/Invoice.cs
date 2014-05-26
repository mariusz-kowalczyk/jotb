using jotb.Library;
using jotb.Library.Helper;
using jotb.Models;
using jotb.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zen.Barcode;

namespace jotb
{
    public partial class Invoice : Form
    {
        private IInvoiceRepository _invoice;
        private List<Dict> Kody;

        public Invoice()
        {
            InitializeComponent();
            _invoice = new InvoiceRepository(new JotbDbContext());

            Kody = new List<Dict>();
            Kody.Add(new Dict("qr", "qr"));

            comboBoxKreskowe.Items.AddRange(Kody.ToArray());
            comboBoxKreskowe.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cursor = Cursor;
            Cursor = Cursors.WaitCursor;
            string Edi = "UNH+1+INVOIC:D:96A:UN:EAN008'BGM+380+432097+9'DTM+137:19971008:102'NAD+SU+4012345500004::9++Firma ABC:::::+ul. Baby Jagi 1:::+Poznań++60–250+PL'RFF+VA:7770020410'NAD+BY+4012345500004::9++Firma ABC:::::+ul. Baby Jagi 1:::+Poznań++60–250+PL'RFF+VA:7770020410'NAD+DP+4012345500004::9++Firma ABCD:::::+ul. Baby Jagi 1:::+Poznań++60–250+PL'PAT+1'DTM+13:19970831:102'LIN+1++4000862141404:EU'IMD+C+name+RC::9:nazwa towaru/uslugi'QTY+47:40'MOA+66:580:PLN'PRI+AAA:14,50'TAX+7+VAT+++:::21+S'LIN+2++4000862141404:EU'IMD+C+name2+RC::9:nazwa towaru/uslugi'QTY+47:40'MOA+66:580:PLN'PRI+AAA:14,50'TAX+7+VAT+++:::21+S'UNS+S'CNT+2:120'MOA+77:45612,20'MOA+79:45612,20'MOA+124:45612,20'MOA+125:45612,20'TAX+7+VAT+++:::22+S'MOA+79:15243,32'MOA+124:3353,53'TAX+7+VAT+++:::21+S'UNT+84+1'";
            InvoiceModel m = _invoice.EdiDecode(Edi);
            _invoice.Add(m);
            Cursor = cursor;
        }

        private void buttonEdiFile_Click(object sender, EventArgs e)
        {
            var cursor = Cursor;
            Cursor = Cursors.WaitCursor;

            openFileDialogEdi.ShowDialog();

            string filename = openFileDialogEdi.FileName;
            try
            {
            string content = File.ReadAllText(filename);

           
                InvoiceModel m = _invoice.EdiDecode(content);
                _invoice.Add(m);
                MessageBox.Show("Wprowadzono pomyślnie plik EDI do bazy :)");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            

            Cursor = cursor;
        }

        private void buttonKreskowe_Click(object sender, EventArgs e)
        {
            Dict si = comboBoxKreskowe.SelectedItem as Dict;
            string text = "Hello World";
            switch (si.Key)
            {
                case "qr" :
                    CodeQrBarcodeDraw bd = BarcodeDrawFactory.CodeQr;
                    pictureBoxKreskowe.Image = bd.Draw(text, 2, 2);
                    break;
            }
        }

        private void buttonGenerujFakture_Click(object sender, EventArgs e)
        {
            InvoiceGenrate IG = new InvoiceGenrate();
            IG.ShowDialog();
        }

        private void xmlButton_Click(object sender, EventArgs e)
        {
            if (xmlOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string xml = File.ReadAllText(xmlOpenFileDialog.FileName);
                    InvoiceModel invoice = _invoice.XmlDecode(xml);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }
    }
}
