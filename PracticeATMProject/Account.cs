using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticeATMProject {
    internal class Account {
        public int ID { get; set; }
        [StringLength(2)]
        public string Type { get; set; } = string.Empty;
        [StringLength(80)]
        public string Description { get; set; } = string.Empty;
        [Column(TypeName = "decimal(3,2)")]
        public decimal InterestRate { get; set; } = 0;
        [Column(TypeName = "decimal(11,2)")]
        public decimal Balance { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        [Column(TypeName = "datetime")]
        public DateTime? ModifiedDate { get; set; }

        public int CustomerID { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
