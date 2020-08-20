namespace ST.Web.Models.Admin
{
    using System.ComponentModel.DataAnnotations;

    public class AdminUpateUserBindingModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    
        public bool? IsAdmin { get; set; }
    }
}
