using jotb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jotb.Repositories
{
    interface IInvoiceRepository : IDisposable
    {
        void Add(InvoiceModel invoice);
        string EdiEncode(InvoiceModel invoice);
        InvoiceModel EdiDecode(string strEdi);
        string XmlEncode(InvoiceModel invoice);
        InvoiceModel XmlDecode(string str);
    }
}
