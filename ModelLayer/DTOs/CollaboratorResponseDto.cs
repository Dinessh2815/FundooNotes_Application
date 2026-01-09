using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.DTOs
{
    public class CollaboratorResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public bool CanEdit { get; set; }
    }

}
