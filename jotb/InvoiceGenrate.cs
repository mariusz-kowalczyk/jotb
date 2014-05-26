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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.ComponentModel.DataAnnotations;

namespace jotb
{
    public partial class InvoiceGenrate : Form
    {
        private IInvoiceRepository _invoice;
        public float Tax = 23;
        public InvoiceGenrate()
        {
            InitializeComponent();
            _invoice = new InvoiceRepository(new JotbDbContext());
        }

        private void buttonTakieSameDane_Click(object sender, EventArgs e)
        {
            tbMDostawyNazwaFirmy.Text = tbNabywcaNazwaFirmy.Text;
            tbMDostawyMiasto.Text = tbNabywcaMiasto.Text;
            tbMDostawyUlica.Text = tbNabywcaUlica.Text;
            tbMDostawyNumer.Text = tbNabywcaNumer.Text;
            tbMDostawyKodPocztowy.Text = tbNabywcaKodPocztowy.Text;
            tbMDostawyKraj.Text = tbNabywcaKraj.Text;
        }

        private void buttonDodajDoListy_Click(object sender, EventArgs e)
        {
            try
            {
                this.dataGridView1.Rows.Add(
                tbNazwaProduktu.Text, 
                tbCenaNetto.Text,
                cbTypOpakowania.SelectedItem, 
                tbIloscProduktu.Text, 
                Convert.ToSingle(tbCenaNetto.Text)*Convert.ToSingle(tbIloscProduktu.Text), 
                (Convert.ToSingle(tbCenaNetto.Text) + (Convert.ToSingle(tbCenaNetto.Text) * Tax)/100) *Convert.ToSingle(tbIloscProduktu.Text));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            tbNazwaProduktu.Text = "";
            tbIloscProduktu.Text = "";
            tbCenaNetto.Text = "";
        }

        private InvoiceModel getFromForm()
        {
            string errorsproduct = string.Empty;
            InvoiceModel invoice = new InvoiceModel();
            invoice.Number = Convert.ToString(tbNumerFaktury.Text);

            if (cbTypFaktury.SelectedItem == "Oryginal")
                invoice.Type = 9;
            if (cbTypFaktury.SelectedItem == "Kopia")
                invoice.Type = 31;
            if (cbTypFaktury.SelectedItem == "Duplikat")
                invoice.Type = 7;

            invoice.SaleDate = dtpDataSprzedazy.Value;
            invoice.PaymentDate = dtpTerminPlatnosci.Value;
            invoice.ProviderName = labelNazwaFirmy.Text;
            invoice.ProviderNip = labelNipFirmy.Text;
            invoice.BuyerName = tbNabywcaNazwaFirmy.Text;
            invoice.BuyerNip = tbNabywcaNip.Text;
            invoice.DeliveryName = tbMDostawyNazwaFirmy.Text;

            InvoiceAddressModel ProvAdr = new InvoiceAddressModel();
            ProvAdr.City = labelMiasto.Text;
            ProvAdr.Street = labelUlica.Text;
            ProvAdr.Number = labelNumer.Text;
            ProvAdr.PostCode = labelKodPocztowy.Text;
            ProvAdr.CountryCode = labelKraj.Text;


            InvoiceAddressModel BuyerAdr = new InvoiceAddressModel();
            BuyerAdr.City = tbNabywcaMiasto.Text;
            BuyerAdr.Street = tbNabywcaUlica.Text;
            BuyerAdr.Number = tbNabywcaNumer.Text;
            BuyerAdr.PostCode = tbNabywcaKodPocztowy.Text;
            BuyerAdr.CountryCode = tbNabywcaKraj.Text;

            List<ValidationResult> results3 = new List<ValidationResult>();
            if (Validator.TryValidateObject(BuyerAdr, new ValidationContext(BuyerAdr, null, null), results3))
            {

            }
            else
            {
                errorsproduct += String.Join("\n", results3) + "\n";
            }


            InvoiceAddressModel DelivAdr = new InvoiceAddressModel();
            DelivAdr.City = tbMDostawyMiasto.Text;
            DelivAdr.Street = tbMDostawyUlica.Text;
            DelivAdr.Number = tbMDostawyNumer.Text;
            DelivAdr.PostCode = tbMDostawyKodPocztowy.Text;
            DelivAdr.CountryCode = tbMDostawyKraj.Text;

            invoice.ProviderAddress = ProvAdr;
            invoice.BuyerAddress = BuyerAdr;
            invoice.DeliveryAddress = DelivAdr;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                InvoiceProductModel prod = new InvoiceProductModel();
                prod.ItemNumber = 11111;
                prod.Number = Convert.ToString(Convert.ToInt16(i + 1));
                prod.Name = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                prod.Type = Convert.ToString(dataGridView1.Rows[i].Cells[2].Value);
                prod.QuantityType = 47;
                prod.Quantity = Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value);
                prod.SumAmountType = "66 ???";                                                                      ////o co chodzi?
                prod.SumAmountNet = Convert.ToSingle(dataGridView1.Rows[i].Cells[4].Value);
                prod.UnitPriceNet = Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value);
                prod.Tax = Convert.ToInt16(Tax);
                List<ValidationResult> results2 = new List<ValidationResult>();
                if (Validator.TryValidateObject(prod, new ValidationContext(prod, null, null), results2))
                    invoice.Products.Add(prod);
                else
                {
                    errorsproduct += String.Join("\n", results2) + "\n";
                }

            }

            invoice.ProductQuntity = dataGridView1.Rows.Count;


            float suma = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                suma += Convert.ToSingle(dataGridView1.Rows[i].Cells[5].Value);
            }
            invoice.AmountGross = suma;

            suma = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                suma += Convert.ToSingle(dataGridView1.Rows[i].Cells[4].Value);
            }
            invoice.AmountNet = suma;

            float sumaNetto = 0;
            float sumaBrutto = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                sumaNetto += Convert.ToSingle(dataGridView1.Rows[i].Cells[4].Value);
                sumaBrutto += Convert.ToSingle(dataGridView1.Rows[i].Cells[5].Value);
            }
            invoice.AmountTax = sumaBrutto - sumaNetto;

            invoice.Tax = Convert.ToInt16(Tax);

            List<ValidationResult> results = new List<ValidationResult>();
            if (Validator.TryValidateObject(invoice, new ValidationContext(invoice, null, null), results))
            {

            }
            else
            {
                errorsproduct += String.Join("\n", results);

            }
            if (!string.IsNullOrEmpty(errorsproduct))
            {
                MessageBox.Show(errorsproduct);
                return null;
            }
            else
            {
                return invoice;
            }
        }

        private void buttonGenerujEDI_Click(object sender, EventArgs e)
        {
            InvoiceModel invoice = this.getFromForm();
            if (invoice != null)
            {
                _invoice.Add(invoice);
                String edi = _invoice.EdiEncode(invoice);
            }
            //File.WriteAllText(@"C:\Users\kamil_000\Desktop\JOBT\Faktura Nr " + Convert.ToString(tbNumerFaktury.Text) + ".edi", edi);

        }

        private void buttonGenerujFakture_Click(object sender, EventArgs e)
        {
            Document doc = new Document(iTextSharp.text.PageSize.A4);
            PdfWriter wri = PdfWriter.GetInstance(doc, new FileStream(@"C:\Users\kamil_000\Desktop\JOBT\Faktura Nr "+Convert.ToString(tbNumerFaktury.Text)+".pdf", FileMode.Create));
            doc.Open();

            PdfPTable naglowek = new PdfPTable(2);

            PdfPCell pieczec = new PdfPCell(new Phrase("Pieczec firmy"));
            pieczec.Colspan = 1;
            naglowek.AddCell(pieczec);

            PdfPTable nested = new PdfPTable(1);
            nested.AddCell("Faktura Vat");
            nested.AddCell("Numer faktury: " + tbNumerFaktury.Text);
            nested.AddCell(Convert.ToString(cbTypFaktury.SelectedItem));
            nested.AddCell("Data wystawienia :" + dtpDataSprzedazy.Value);
            PdfPCell nesthousing = new PdfPCell(nested);
            nesthousing.Padding = 0f;
            naglowek.AddCell(nesthousing);

            doc.Add(naglowek);

            PdfPTable table = new PdfPTable(2);
            table.AddCell("Dane Sprzedawcy: \n" + labelNazwaFirmy.Text + "\n"+ "Adres firmy:" + "\n" + labelUlica.Text + " " + labelNumer.Text + "\n" + labelMiasto.Text + " " + labelKodPocztowy.Text + "\n" + "NIP: " + labelNipFirmy.Text);
            table.AddCell("Dane Nabywcy: \n" + tbNabywcaNazwaFirmy.Text + "\n" + "Adres firmy:" + "\n" + tbNabywcaUlica.Text + " " + tbNabywcaNumer.Text + "\n" + tbNabywcaMiasto.Text + " " + tbNabywcaKodPocztowy.Text + "\n" + "NIP: " + tbNabywcaNip.Text);
            doc.Add(table);

            PdfPTable tableDGV = new PdfPTable(dataGridView1.Columns.Count);
            for (int j = 0; j < dataGridView1.Columns.Count;j++)
            {
                tableDGV.AddCell(new Phrase(dataGridView1.Columns[j].HeaderText));
            }
            tableDGV.HeaderRows =1;
            for (int i=0;i<dataGridView1.Rows.Count;i++)
            {
                for (int k=0; k<dataGridView1.Columns.Count;k++)
                {
                    if (dataGridView1[k, i].Value != null)
                    {
                        tableDGV.AddCell(new Phrase(dataGridView1[k, i].Value.ToString()));
                    }
                }
            }
            doc.Add(tableDGV);



            doc.Close();

        }

        private void generateXML_Click(object sender, EventArgs e)
        {
            InvoiceModel invoice = this.getFromForm();
            if (invoice != null)
            {
                string xml = _invoice.XmlEncode(invoice);
                if (xmlSaveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(xmlSaveFileDialog.FileName, xml);
                    MessageBox.Show("Zapisano  do pliku xml.");
                }
            }
        }


    }
}
