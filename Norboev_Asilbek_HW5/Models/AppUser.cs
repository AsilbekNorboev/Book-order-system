using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace Norboev_Asilbek_HW5.Models
{
    public class AppUser : IdentityUser
    {
        //Add additional user fields here
        //First name is provided as an example


        [Display(Name = "First Name")]
        public String FirstName { get; set; }


        [Display(Name = "Last Name")]
        public String LastName { get; set; }


        [Display(Name = "Full Name")]
        public String FullName
        {
            get { return FirstName + " " + LastName; }
        }
        //user can have many orders
        public List<Order> Orders { get; set; }
        public AppUser()
        {
            if (Orders == null)
            {
                Orders = new List<Order>();
            }
        }

    }
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (DateTime.Today.AddYears(-_minimumAge) < date)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? "Invalid age.");
        }
    }
}
