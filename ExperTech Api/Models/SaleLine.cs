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
    
    public partial class SaleLine
    {
        public int ProductID { get; set; }
        public int SaleID { get; set; }
        public int Quantity { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Sale Sale { get; set; }
    }
}
