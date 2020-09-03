using ExperTech_Api.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Mail;
using System.Data.Entity;

namespace SteveAPI.Controllers
{
    public class SaleController : ApiController
    {

        private ExperTechEntities db = new ExperTechEntities();

        [Route("api/Sale/GetSaleList")]
        [HttpGet]

        public List<dynamic> GetSaleList()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return SaleList(db.Sales.Include(zz => zz.SaleLines).ToList());


        }

        private List<dynamic> SaleList(List<Sale> Model1)
        {
            List<dynamic> newlist = new List<dynamic>();
            foreach (Sale loop in Model1)
            {
                dynamic dynobject = new ExpandoObject();
                dynobject.SaleID = loop.SaleID;
                dynobject.Status = db.SaleStatus.Where(zz => zz.StatusID == loop.StatusID).Select(zz => zz.Status).FirstOrDefault();
                dynobject.PaymentType = db.PaymentTypes.Where(zz => zz.PaymentTypeID == loop.PaymentTypeID).Select(zz => zz.Type).FirstOrDefault();
                dynobject.Client = db.Clients.Where(zz => zz.ClientID == loop.ClientID).Select(zz => zz.Name).FirstOrDefault();
                dynobject.Date = loop.Date;
                dynobject.SaleType = db.SaleTypes.Where(zz => zz.SaleTypeID == loop.SaleTypeID).Select(zz => zz.Type).FirstOrDefault();
                dynobject.Payment = loop.Payment;
                if (loop.ReminderID != null)
                {
                    TimeSpan daysLeft = loop.Date.AddDays(10) - DateTime.Now;
                    dynobject.Reminder = daysLeft.Days;
                }

                List<dynamic> saleThings = new List<dynamic>();
                foreach (SaleLine items in loop.SaleLines)
                {
                    dynamic newObject = new ExpandoObject();
                    newObject.ProductID = items.ProductID;
                    Product findProds = db.Products.Where(zz => zz.ProductID == items.ProductID).FirstOrDefault();
                    newObject.ProductName = findProds.Name;
                    newObject.Price = findProds.Price;
                    newObject.Quantity = items.Quantity;
                    saleThings.Add(newObject);
                }
                dynobject.Products = saleThings;
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

        public static void Email(int AdminID, string Email)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("hairexhilaration@gmail.com");
                message.To.Add(new MailAddress(Email));
                message.Subject = "Exhilartion Hair & Beauty Registration";
                message.IsBodyHtml = false;
                message.Body = "Click the link below to setup account:" + "/n" + "hhtp://localhost:4200/" + AdminID.ToString();
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("hairexhilaration@gmail.com", "@Exhileration1");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception)
            {

            }
        }

    }
    //8.6 Regenerate Sale Invoice 



}
