//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExperTech_Api.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            this.EmployeeSchedules = new HashSet<EmployeeSchedule>();
            this.EmployeeServiceTypes = new HashSet<EmployeeServiceType>();
        }
    
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public Nullable<int> UserID { get; set; }
    
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployeeSchedule> EmployeeSchedules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployeeServiceType> EmployeeServiceTypes { get; set; }
    }
}
