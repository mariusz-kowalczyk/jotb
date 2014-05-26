using jotb.Library;
using jotb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace jotb.Repositories
{
    class InvoiceRepository : IInvoiceRepository, IDisposable
    {
        private JotbDbContext _context;
        public InvoiceRepository(JotbDbContext context)
        {
            _context = context;
        }

        public void Add(InvoiceModel invoice)
        {
            /*
            _context.InvoiceAddresses.Add(invoice.BuyerAddress);
            _context.InvoiceAddresses.Add(invoice.DeliveryAddress);
            _context.InvoiceAddresses.Add(invoice.ProviderAddress);
            _context.SaveChanges();

            ICollection<InvoiceProductModel> products = invoice.Products;
            invoice.Products = null;

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            invoice.Products = products;
            
            foreach(InvoiceProductModel i in invoice.Products)
            {
                _context.InvoiceProducts.Add(i);
            }
            */
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
        }

        public string EdiEncode(InvoiceModel invoice)
        {
            List<string> segments = new List<string>();

            segments.Add("UNH+1+INVOIC:D:96A:UN:EAN008");
            //numer faktury
            segments.Add("BGM+380+" + invoice.Number + "+" + invoice.Type);
            //data wystawienia faktury
            segments.Add("DTM+137:"+invoice.SaleDate.ToString("yyyyMMdd")+":102");
            //dane dostawcy
            segments.Add("NAD+SU+4012345500004::9++" +
                Edi.Filter(invoice.ProviderName) + ":::::+" +
                Edi.Filter(invoice.ProviderAddress.Street) +
                " " +
                invoice.ProviderAddress.Number +
                ":::+" + Edi.Filter(invoice.ProviderAddress.City) + "++" + invoice.ProviderAddress.PostCode + "+" + invoice.ProviderAddress.CountryCode
                );
            segments.Add("RFF+VA:" + invoice.ProviderNip);
            //dane nabywcy
            segments.Add("NAD+BY+4012345500004::9++" +
                Edi.Filter(invoice.BuyerName) +
                ":::::+" +
                Edi.Filter(invoice.BuyerAddress.Street) +
                " " +
                invoice.BuyerAddress.Number +
                ":::+" +
                Edi.Filter(invoice.BuyerAddress.City) +
                "++" +
                invoice.BuyerAddress.PostCode + "+" + invoice.BuyerAddress.CountryCode
                );
            segments.Add("RFF+VA:" + invoice.BuyerNip);
            //miejsce dostwy
            if (invoice.DeliveryName != null && invoice.DeliveryAddress != null)
            {
                segments.Add("NAD+DP+4012345500004::9++" +
                    Edi.Filter(invoice.DeliveryName) +
                    ":::::+" +
                    Edi.Filter(invoice.DeliveryAddress.Street) +
                    " " +
                    invoice.DeliveryAddress.Number +
                    ":::+" +
                    Edi.Filter(invoice.DeliveryAddress.City) +
                    "++" +
                    invoice.DeliveryAddress.PostCode + "+" +
                    invoice.DeliveryAddress.CountryCode
                    );
            }
            //
            segments.Add("PAT+1");
            //data płatności
            segments.Add("DTM+13:" + invoice.PaymentDate.ToString("yyyyMMdd") + ":102");
            
            //wypisanie towarów/usług
            foreach (InvoiceProductModel p in invoice.Products)
            {
                //numer pozycji towarowej na fakturze++globalny numer towaru
                segments.Add("LIN+" + p.ItemNumber + "++" + p.Number);
                //nazwa towaru/uslugi
                segments.Add("IMD+C++" + p.Type + "::9:" + Edi.Filter(p.Name));
                //ilość
                segments.Add("QTY+" + p.QuantityType + ":" + p.Quantity);
                //sumaryczna wartość towarów
                segments.Add("MOA+" + p.SumAmountType + ":" + p.SumAmountNet + ":PLN");
                //cena jednostkowa netto
                segments.Add("PRI+AAA:" + p.UnitPriceNet);
            }

            //liczba pozycji towarowych
            segments.Add("CNT+" + invoice.ProductQuntity + ":120");
            //kwota do zapłaty
            segments.Add("MOA+77:" + invoice.AmountGross);
            //kwota netto
            segments.Add("MOA+79:" + invoice.AmountNet);
            //kwota podatku
            segments.Add("MOA+124:" + invoice.AmountTax);
            //kwota opodatkowana
            segments.Add("MOA+125:" + invoice.AmountTax);
            //stawka podatku
            segments.Add("TAX+7+VAT+++:::" + invoice.Tax + "+S");

            //liczba segmentow w komunikacie
            segments.Add("UNT+" + (segments.Count() + 1) + "+1");

            return String.Join("'", segments);
        }

        public InvoiceModel EdiDecode(string strEdi)
        {
            InvoiceModel invoice = new InvoiceModel();

            string[] segments = Edi.ParseToSegmets(strEdi);

            int nSegments = segments.Length;

            // ustawiam czy odczytywane dane należą do dostwcy czy nabywcy, lub czy są to dane dostwy
            // 0 - dostawca, 1 - nabywca, 2 - dane do dostwy
            int who = 0;
            string[] arguments;
            string[] parameters;
            InvoiceAddressModel address;
            int productIndex = -1;
            InvoiceProductModel product;
            List<InvoiceProductModel> products = new List<InvoiceProductModel>();
            bool details = false;

            for (int i = 0; i < nSegments; i++)
            {
                if (String.IsNullOrEmpty(segments[i]))
                    continue;
                arguments = Edi.ParseToArguments(segments[i]);

                switch (arguments[0])
                {
                    case "UNH" :
                        parameters = Edi.ParseToParameters(arguments[2]);
                        if (parameters[0] != "INVOIC")
                        {
                            return null;
                        }
                        break;
                    case "BGM" :
                        if (arguments[1] == "380")
                        {
                            invoice.Number = arguments[2];
                            invoice.Type = Int16.Parse(arguments[3]);
                        }
                        break;
                    case "DTM" :
                        DateTime date;
                        parameters = Edi.ParseToParameters(arguments[1]);
                        if (parameters[2] == "102")
                        {
                            date = DateTime.ParseExact(parameters[1], "yyyyMMdd", null);
                        }
                        else
                        {
                            date = DateTime.Parse(parameters[1]);
                        }
                        switch (parameters[0])
                        {
                            case "137" :
                                invoice.SaleDate = date;
                                break;
                            case "13" :
                                invoice.PaymentDate = date;
                                break;
                        }
                        break;
                    case "NAD" :
                        address = new InvoiceAddressModel();
                        parameters = Edi.ParseToParameters(arguments[5]);
                        address.Street = parameters[0];
                        address.City = arguments[6];
                        address.PostCode = arguments[8];
                        address.CountryCode = arguments[9];

                        switch(arguments[1]) {
                            case "SU" :
                                //dane dostawcy
                                who = 0;
                                parameters = Edi.ParseToParameters(arguments[4]);
                                invoice.ProviderName = parameters[0];
                                invoice.ProviderAddress = address;
                                break;
                            case "BY" :
                                //dane nabywcy
                                who = 1;
                                parameters = Edi.ParseToParameters(arguments[4]);
                                invoice.BuyerName = parameters[0];
                                invoice.BuyerAddress = address;
                                break;
                            case "DP" :
                                //dane do dostawy
                                who = 2;
                                parameters = Edi.ParseToParameters(arguments[4]);
                                invoice.DeliveryName = parameters[0];
                                invoice.DeliveryAddress = address;
                                break;
                        }
                        break;
                    case "RFF" :
                        parameters = Edi.ParseToParameters(arguments[1]);
                        if (parameters[0] == "VA")
                        {
                            switch (who)
                            {
                                case 0 :
                                    //nip dostawcy
                                    invoice.ProviderNip = parameters[1];
                                    break;
                                case 1 :
                                    //nip nabywcy
                                    invoice.BuyerNip = parameters[1];
                                    break;
                            }
                        }
                        break;
                    case "PAT" :
                        details = true;
                        break;
                    case "LIN" :
                        productIndex++;
                        product = new InvoiceProductModel();
                        product.ItemNumber = Int16.Parse(arguments[1]);
                        parameters = Edi.ParseToParameters(arguments[3]);
                        product.Number = parameters[0];
                        products.Insert(productIndex, product);
                        break;
                    case "IMD" :
                        product = products[productIndex];
                        product.Name = arguments[2];
                        parameters = Edi.ParseToParameters(arguments[3]);
                        product.Type = parameters[0];
                        break;
                    case "QTY" :
                        product = products[productIndex];
                        parameters = Edi.ParseToParameters(arguments[1]);
                        product.QuantityType = Int16.Parse(parameters[0]);
                        product.Quantity = int.Parse(parameters[1]);
                        break;
                    case "MOA" :
                        parameters = Edi.ParseToParameters(arguments[1]);
                        if (details)
                        {
                            switch (parameters[0])
                            {
                                case "66":
                                case "35E":
                                    product = products[productIndex];
                                    product.SumAmountType = parameters[0];
                                    product.SumAmountNet = float.Parse(parameters[1]);
                                    break;
                            }
                        }
                        else
                        {
                            float amount = float.Parse(parameters[1]);
                            switch (parameters[0])
                            {
                                case "77" :
                                    invoice.AmountGross = amount;
                                    break;
                                case "79" :
                                    invoice.AmountNet = amount;
                                    break;
                                case "124" :
                                    invoice.AmountTax = amount;
                                    break;
                            }
                        }
                        break;
                    case "PRI" :
                        parameters = Edi.ParseToParameters(arguments[1]);
                        if (parameters[0] == "AAA")
                        {
                            product = products[productIndex];
                            product.UnitPriceNet = float.Parse(parameters[1]);
                        }
                        break;
                    case "TAX" :
                        if (arguments[1] == "7" && arguments[2] == "VAT")
                        {
                            parameters = Edi.ParseToParameters(arguments[5]);
                            if (details)
                            {
                                product = products[productIndex];
                                product.Tax = Int16.Parse(parameters[3]);
                            }
                            else
                            {
                                invoice.Tax = Int16.Parse(parameters[3]);
                            }
                        }
                        break;
                    case "UNS" :
                        if (arguments[1] == "S")
                        {
                            invoice.Products = products;
                            details = false;
                        }
                        break;
                    case "CNT" :
                        parameters = Edi.ParseToParameters(arguments[1]);
                        invoice.ProductQuntity = int.Parse(parameters[0]);
                        break;
                }
            }
            
            return invoice;
        }

        public string XmlEncode(InvoiceModel invoice)
        {
            MemoryStream strm = new MemoryStream(); 
            XmlTextWriter xml = new XmlTextWriter(strm, Encoding.UTF8);
            xml.WriteStartDocument();

            xml.WriteStartElement("faktura");
            xml.WriteAttributeString("nr", invoice.Number);
            xml.WriteAttributeString("type", (invoice.Type == (Int16)InvoiceModel.Types.Orginal ? "orginal" : (invoice.Type == (Int16)InvoiceModel.Types.Copy ? "kopia" : "duplikat")));

            xml.WriteStartElement("naglowek");

            xml.WriteStartElement("szczegoly");
            xml.WriteElementString("datawystawienia", System.DateTime.Now.ToShortDateString());
            xml.WriteEndElement(); //szczegoly

            xml.WriteStartElement("platnosc");
            xml.WriteAttributeString("waluta", "PLN");
            xml.WriteEndElement(); //platnosc

            xml.WriteStartElement("sprzedawca");
            xml.WriteElementString("nip", invoice.ProviderNip);
            xml.WriteElementString("nazwa", invoice.ProviderName);
            xml.WriteElementString("ulica", invoice.ProviderAddress.Street);
            xml.WriteElementString("nrlokalu", invoice.ProviderAddress.Number);
            xml.WriteElementString("kodpocztowy", invoice.ProviderAddress.PostCode);
            xml.WriteElementString("miasto", invoice.ProviderAddress.City);
            xml.WriteElementString("kraj", invoice.ProviderAddress.CountryCode);
            xml.WriteEndElement(); //sprzedawca

            xml.WriteStartElement("klient");
            if (!String.IsNullOrEmpty(invoice.BuyerNip))
            {
                xml.WriteElementString("nip", invoice.BuyerNip);
            }
            xml.WriteElementString("nazwa", invoice.BuyerName);
            xml.WriteElementString("ulica", invoice.BuyerAddress.Street);
            xml.WriteElementString("nrlokalu", invoice.BuyerAddress.Number);
            xml.WriteElementString("kodpocztowy", invoice.BuyerAddress.PostCode);
            xml.WriteElementString("miasto", invoice.BuyerAddress.City);
            xml.WriteElementString("kraj", invoice.BuyerAddress.CountryCode);
            xml.WriteEndElement(); //klient

            xml.WriteStartElement("dostawca");
            xml.WriteElementString("nazwa", invoice.DeliveryName);
            xml.WriteElementString("ulica", invoice.DeliveryAddress.Street);
            xml.WriteElementString("nrlokalu", invoice.DeliveryAddress.Number);
            xml.WriteElementString("kodpocztowy", invoice.DeliveryAddress.PostCode);
            xml.WriteElementString("miasto", invoice.DeliveryAddress.City);
            xml.WriteElementString("kraj", invoice.DeliveryAddress.CountryCode);
            xml.WriteEndElement(); //dostawca

            xml.WriteEndElement(); //naglowek

            xml.WriteStartElement("pozycje");
            xml.WriteAttributeString("liczbapozycji", invoice.ProductQuntity.ToString());

            foreach (InvoiceProductModel item in invoice.Products)
            {
                xml.WriteStartElement("pozycja");
                xml.WriteElementString("nazwa", item.Name);
                xml.WriteElementString("opis", "");
                xml.WriteStartElement("ilosc");
                xml.WriteAttributeString("jednostka", item.QuantityType.ToString());
                xml.WriteString(item.Quantity.ToString());
                xml.WriteEndElement(); //ilosc
                xml.WriteElementString("rabat", "");
                xml.WriteElementString("podatekvat", item.Tax.ToString());
                xml.WriteElementString("cenanetto", item.SumAmountNet.ToString());
                xml.WriteElementString("cenabrutto", (item.SumAmountNet * item.Tax / 100).ToString());
                xml.WriteEndElement(); //pozycja
            }

            xml.WriteEndElement(); //pozycje

            xml.WriteEndElement(); //faktura
            
            xml.Flush();
            xml.Close();
            return Encoding.UTF8.GetString(strm.ToArray());
        }

        public InvoiceModel XmlDecode(string str)
        {
            
            InvoiceModel invoice = new InvoiceModel();
            InvoiceAddressModel address;
            InvoiceProductModel product;

            XDocument xml = XDocument.Parse(str);

            foreach (XAttribute attr in xml.Root.Attributes())
            {
                switch (attr.Name.LocalName.ToString())
                {
                    case "nr":
                        invoice.Number = attr.Value.ToString();
                        break;
                    case "type":
                        switch (attr.Value.ToString())
                        {
                            case "orginal":
                                invoice.Type = (Int16)InvoiceModel.Types.Orginal;
                                break;
                            case "kopia":
                                invoice.Type = (Int16)InvoiceModel.Types.Copy;
                                break;
                            case "duplikat":
                                invoice.Type = (Int16)InvoiceModel.Types.Duplicate;
                                break;
                        }
                        break;
                }
            }

            address = new InvoiceAddressModel();
            foreach (XElement el in xml.Descendants("sprzedawca").Elements())
            {
                switch (el.Name.LocalName.ToString())
                {
                    case "nip":
                        invoice.ProviderNip = el.Value.ToString();
                        break;
                    case "nazwa":
                        invoice.ProviderName = el.Value.ToString();
                        break;
                    case "ulica":
                        address.Street = el.Value.ToString();
                        break;
                    case "nrlokalu":
                        address.Number = el.Value.ToString();
                        break;
                    case "kodpocztowy":
                        address.PostCode = el.Value.ToString();
                        break;
                    case "miasto":
                        address.City = el.Value.ToString();
                        break;
                    case "kraj":
                        address.CountryCode = el.Value.ToString();
                        break;
                }
            }
            invoice.ProviderAddress = address;

            address = new InvoiceAddressModel();
            foreach (XElement el in xml.Descendants("klient").Elements())
            {
                switch (el.Name.LocalName.ToString())
                {
                    case "nip":
                        invoice.BuyerNip = el.Value.ToString();
                        break;
                    case "nazwa":
                        invoice.BuyerName = el.Value.ToString();
                        break;
                    case "ulica":
                        address.Street = el.Value.ToString();
                        break;
                    case "nrlokalu":
                        address.Number = el.Value.ToString();
                        break;
                    case "kodpocztowy":
                        address.PostCode = el.Value.ToString();
                        break;
                    case "miasto":
                        address.City = el.Value.ToString();
                        break;
                    case "kraj":
                        address.CountryCode = el.Value.ToString();
                        break;
                }
            }
            invoice.BuyerAddress = address;


            address = new InvoiceAddressModel();
            foreach (XElement el in xml.Descendants("dostawca").Elements())
            {
                switch (el.Name.LocalName.ToString())
                {
                    case "nazwa":
                        invoice.DeliveryName = el.Value.ToString();
                        break;
                    case "ulica":
                        address.Street = el.Value.ToString();
                        break;
                    case "nrlokalu":
                        address.Number = el.Value.ToString();
                        break;
                    case "kodpocztowy":
                        address.PostCode = el.Value.ToString();
                        break;
                    case "miasto":
                        address.City = el.Value.ToString();
                        break;
                    case "kraj":
                        address.CountryCode = el.Value.ToString();
                        break;
                }
            }
            invoice.DeliveryAddress = address;
            XElement items = xml.Descendants("pozycje").First();
            invoice.ProductQuntity = int.Parse(items.Attribute("liczbapozycji").Value.ToString());

            foreach (XElement item in items.Elements())
            {
                product = new InvoiceProductModel();
                foreach (XElement el in item.Elements())
                {
                    string value = el.Value.ToString();
                    switch (el.Name.LocalName.ToString())
                    {
                        case "nazwa" :
                            product.Name = value;
                            break;
                        case "ilosc" :
                            product.Quantity = int.Parse(value);
                            product.QuantityType = Int16.Parse(el.Attribute("jednostka").Value.ToString());
                            break;
                        case "podatekvat" :
                            product.Tax = Int16.Parse(value);
                            break;
                        case "cenanetto" :
                            product.SumAmountNet = float.Parse(value);
                            break;
                    }
                }
                invoice.Products.Add(product);
            }

            return invoice;
        }


        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                _context.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

    }
}
