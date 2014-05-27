using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jotb.Library
{
    class Print
    {
        private Image tmpImage;
        private string tmpText;

        public void printBarcode(Image barcode, string text)
        {
            this.tmpImage = barcode;
            this.tmpText = text;
            PrintDocument pd = new PrintDocument();
            pd.DocumentName = "Barcode";

            try
            {
                pd.PrintPage += new PrintPageEventHandler(this.printBarcodePage);
                pd.Print();
            }
            catch (Exception err)
            {
                MessageBox.Show("Wystąpił błąd podczas drukowania barcode. " + err.Message);
            }
        }

        private void printBarcodePage(object sender, PrintPageEventArgs ev)
        {
            Font font = new Font("Arial", 14);
            ev.Graphics.DrawImage(this.tmpImage, ev.MarginBounds.Left, ev.MarginBounds.Top);

            //ev.Graphics.DrawString(this.tmpText, font, Brushes.Black, ev.MarginBounds.Left, ev.MarginBounds.Top + 65);

            ev.HasMorePages = false;
        }
    }
}
