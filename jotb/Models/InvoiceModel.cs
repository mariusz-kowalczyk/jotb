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
    [Table("Invoices")]
    class InvoiceModel
    {
        public enum Types { Orginal = 9, Duplicate = 7, Copy = 31 }

        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Number { get; set; }

        [Required]
        [DefaultValue(Types.Orginal)]
        public Int16 Type { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        [Required]        
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(300)]
        [MinLength(3)]
        public string ProviderName { get; set; }

        [Required]
        public InvoiceAddressModel ProviderAddress { get; set; }

        [Required]
        [MaxLength(10)]
        public string ProviderNip { get; set; }

        [Required]
        [MaxLength(300)]
        [MinLength(3)]
        public string BuyerName { get; set; }

        [Required]
        public InvoiceAddressModel BuyerAddress { get; set; }

        [Required]
        [MaxLength(10)]
        public string BuyerNip { get; set; }

        [MaxLength(300)]
        public string DeliveryName { get; set; }

        public InvoiceAddressModel DeliveryAddress { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public ICollection<InvoiceProductModel> Products { get; set; }

        [Required]
        public int ProductQuntity { get; set; }

        [Required]
        public float AmountGross { get; set; }

        [Required]
        public float AmountNet { get; set; }

        [Required]
        public float AmountTax { get; set; }

        [Required]
        public Int32 Tax { get; set; }

        public InvoiceModel()
        {
            this.Products = new HashSet<InvoiceProductModel>();
            this.Date = DateTime.Now;
        }
    }
}
