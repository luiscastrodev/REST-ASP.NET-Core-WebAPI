using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FirstAPI.Model
{
    public class Customer
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Document { get; set; }
        public int Type { get; set; }
        public bool Enable { get; set; }

    }
}
