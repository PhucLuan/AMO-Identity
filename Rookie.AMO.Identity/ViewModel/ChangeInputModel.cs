using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.Identity.ViewModel
{
    public class ChangeInputModel
    {
        [Required(ErrorMessage = "Old password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        public string CurrentPass { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPass { get; set; }
        public string ReturnUrl { get; set; }
    }
}
