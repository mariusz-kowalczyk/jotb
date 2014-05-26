using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jotb.Models
{
    [Table("InvoiceAddresses")]
    class InvoiceAddressModel
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string Street { get; set; }

        
        [MaxLength(20)]
        public string Number { get; set; }

        [Required]
        [MaxLength(6)]
        public string PostCode { get; set; }

        [Required]
        [MaxLength(4)]
        public string CountryCode { get; set; }
    }
}
