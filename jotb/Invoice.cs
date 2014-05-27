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
            Kody.Add(new Dict("code_11", "Code 11"));
            Kody.Add(new Dict("code_39", "Code 39"));
            Kody.Add(new Dict("code_128", "Code 128"));
            Kody.Add(new Dict("ean8", "EAN-8"));
            Kody.Add(new Dict("ean13", "EAN-13"));

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
                    try
                    {
                        _invoice.Add(invoice);
                        MessageBox.Show("Zapisano z XML'a do bazy");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                    
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void buttonKreskowe_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Dict si = comboBoxKreskowe.SelectedItem as Dict;
            string text = textBoxBarcode.Text;
            Image barcodeImage = null;
            switch (si.Key)
            {
                case "qr":
                    CodeQrBarcodeDraw bd = BarcodeDrawFactory.CodeQr;
                    barcodeImage = bd.Draw(text, 2, 2);
                    break;
                case "code_11":
                    if (Barcode.isValidCode11(text))
                    {
                        Code11BarcodeDraw bd_code11 = BarcodeDrawFactory.Code11WithoutChecksum;
                        barcodeImage = bd_code11.Draw(text, 60, 1);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzona wartość nie może zostać wygenerowana.\nCode 11 zawiera cyfry od 0 do 9 oraz łącznik (-).");
                    }
                    break;
                case "code_39":
                    if (Barcode.isValidCode39(text))
                    {
                        Code39BarcodeDraw bd_code39 = BarcodeDrawFactory.Code39WithoutChecksum;
                        barcodeImage = bd_code39.Draw(text, 60, 1);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzona wartość nie może zostać wygenerowana.\n" +
                            "Code 39 zawiera wszystkie litery, cyfry od 0 do 9, następujące znaki specjalne \"-.$/+%\" i spacje.");
                    }
                    break;
                case "code_128":
                    if (Barcode.isValidCode128(text))
                    {
                        Code128BarcodeDraw bd_code128 = BarcodeDrawFactory.Code128WithChecksum;
                        barcodeImage = bd_code128.Draw(text, 60, 1);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzona wartość nie może zostać wygenerowana.\n" +
                            "");
                    }
                    break;
                case "ean8":
                    if (Barcode.isValidEan8(text))
                    {
                        CodeEan8BarcodeDraw bd_ean8 = BarcodeDrawFactory.CodeEan8WithChecksum;
                        barcodeImage = bd_ean8.Draw(text, 60, 1);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzona wartość nie może zostać wygenerowana.\n" +
                            "Kod ten zawiera 8 cyfr, ale ostatni znak jest sumą kontrolną, więc należy wprowadzić 7 znaków.");
                    }
                    break;
                case "ean13":
                    if (Barcode.isValidEan13(text))
                    {
                        CodeEan13BarcodeDraw bd_ean13 = BarcodeDrawFactory.CodeEan13WithChecksum;
                        barcodeImage = bd_ean13.Draw(text, 60, 1);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzona wartość nie może zostać wygenerowana.\n" +
                            "Kod ten zawiera 13 cyfr, ale ostatni znak jest sumą kontrolną, więc należy wprowadzić 12 znaków.");
                    }
                    break;
            }
            if (barcodeImage != null)
            {
                Print print = new Print();
                print.printBarcode(barcodeImage, text);
                pictureBoxKreskowe.Image = barcodeImage;
            }

            Cursor = Cursors.Default;
        }
    }
}
