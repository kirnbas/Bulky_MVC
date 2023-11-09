using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class RoleManagementVM
    {
        public IEnumerable<SelectListItem> Roles { get; set; }
        public IEnumerable<SelectListItem> Companies { get; set; }

        public ApplicationUser User { get; set; }
    }
}
