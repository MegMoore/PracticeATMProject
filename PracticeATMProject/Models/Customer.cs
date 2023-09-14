using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticeATMProject.Models
{
    public class Customer
    {
        public int ID { get; set; }
        [StringLength(30)] public string Name { get; set; } = string.Empty;
        public int CardCode { get; set; }
        public int PinCode { get; set; }
        [Column(TypeName = "datetime")] public DateTime LastTransactionDate { get; set; }
        [Column(TypeName = "datetime")] public DateTime CreationDate { get; set; } = DateTime.Now;
        [Column(TypeName = "datetime")] public DateTime ModifiedDate { get; set; }

    }
}
