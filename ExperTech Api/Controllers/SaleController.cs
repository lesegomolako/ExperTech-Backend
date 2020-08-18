using ExperTech_Api.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ExperTech_Api.Controllers
{
    public class SaleController : ApiController
    {
        private ExperTechEntities db = new ExperTechEntities();

        [Route("api/Sale/GetSaleList")]
        [HttpGet]

        public List<dynamic> GetSaleList()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return SaleList(db.SaleLines.ToList());


        }

        private List<dynamic> SaleList(List<SaleLine> Model1)
        {
            List<dynamic> newlist = new List<dynamic>();
            foreach (SaleLine loop in Model1)
            {
                dynamic dynobject = new ExpandoObject();
                dynobject.SaleID = loop.SaleID;
                dynobject.Date = loop.Sale.Date;
                dynobject.Client = loop.Sale.Client.Name;
                dynobject.ContactNo = loop.Sale.Client.ContactNo;
                dynobject.Product = loop.Product.Name;
                if (loop.Sale.ReminderID == 2)
                {
                    TimeSpan daysLeft = loop.Sale.Date.AddDays(10) - DateTime.Now;
                    dynobject.Reminder = daysLeft.Days;
                    dynobject.PaymentType = loop.Sale.PaymentType.Type;
                }
                else if (loop.Sale.ReminderID == 3)
                {
                    TimeSpan daysLeft = loop.Sale.Date.AddDays(10) - DateTime.Now;
                    dynobject.Reminder = daysLeft.Days;
                    dynobject.PaymentType = loop.Sale.PaymentType.Type;
                }

                newlist.Add(dynobject);


            }
            return newlist;
        }

        [Route("api/Sale/AddMakeSale")]
        [HttpPost]
        public dynamic AddMakeSale([FromBody] Sale AddObject)
        {
            if (AddObject != null)
            {
                Sale MakeSale = new Sale();
                MakeSale.ClientID = AddObject.ClientID;
                MakeSale.StatusID = 1;
                MakeSale.ReminderID = 2;
                MakeSale.Date = DateTime.Now;
                db.Sales.Add(MakeSale);
                db.SaveChanges();

                int SaleID = db.Sales.Where(zz => zz.ClientID == AddObject.ClientID).Select(zz => zz.SaleID).LastOrDefault();
                int BasketID = db.Baskets.Where(zz => zz.ClientID == AddObject.ClientID).Select(zz => zz.BasketID).FirstOrDefault();

                List<BasketLine> getBasket = db.BasketLines.Where(zz => zz.BasketID == BasketID).ToList();

                foreach (BasketLine items in getBasket)
                {
                    SaleLine AddSaleLine = new SaleLine();
                    AddSaleLine.Quantity = items.Quantity;
                    AddSaleLine.ProductID = items.ProductID;
                    AddSaleLine.SaleID = SaleID;
                    db.SaleLines.Add(AddSaleLine);
                    db.SaveChanges();
                }

                return "success";

            }
            else
            {
                return null;
            }
        }

        //8.4 Cancel Sale
        //8.5 Send Pick Up Reminder 
        //8.6 Regenerate Sale Invoice 

        //10.7 Regenerate Supplier Order   

    }

}
