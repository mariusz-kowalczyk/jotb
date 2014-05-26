using jotb.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jotb.Models
{
    [Table("InvoiceProducts")]
    class InvoiceProductModel
    {
        public enum QuantityTypes {
            /**
             * <summary>
             * ilość zafakturowana
             * </summary>
             */
            AmountInvoiced = 47,
            /**
             * <summary>
             * ilość wysłana
             * </summary>
             */
            AmountSent = 12,
            /**
             * <summary>
             * liczba jednostek konsumenckich w jednostce zbiorczej
             * </summary>
             */
            NumberOfConsumerUnitsInTradedUnit = 59,
            /**
             * <summary>
             * ilość w opakowaniu
             * </summary>
             */
            AmountInBox = 52
        }

        public int ID { get; set; }

        [Required]
        public Int16 ItemNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Number { get; set; }

        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        [Required]
        [MaxLength(10)]
        public string Type { get; set; }

        [Required]
        [DefaultValue(QuantityTypes.AmountSent)]
        public Int16 QuantityType { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(10)]
        public string SumAmountType { get; set; }

        [Required]
        public float SumAmountNet { get; set; }

        [Required]
        public float UnitPriceNet { get; set; }

        [Required]
        public Int16 Tax { get; set; }

        public virtual InvoiceModel Invoice {get;set;}

    }
}
