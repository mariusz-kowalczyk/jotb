using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jotb.Constants
{
    class InvoiceProductType
    {
        /**
         * <summary>
         * opakowanie zwrotne ("RC")
         * </summary>
         */
        public static string ReturnablePackaging { get { return "RC"; } }

        /**
         * <summary>
         * usługa ("SER")
         * </summary>
         */
        public static string Service { get { return "SER"; } }

        /**
         * <summary>
         * jednostka fakturowana ("UN")
         * </summary>
         */
        public static string InvoicedUnit { get { return "UN"; } }

        /**
         * <summary>
         * jednostka konsumencka ("CU")
         * </summary>
         */
        public static string ConsumerUnit { get { return "CU"; } }
    }
}
