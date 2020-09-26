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
//using System.Windows.Forms;

namespace ExperTech_Api.Controllers
{
    public class SaleController : ApiController
    {

         ExperTechEntities db = new ExperTechEntities();

      
        [System.Web.Http.Route("api/Client/getBasketlinewithProduct")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getBasketlinewithProduct(string sess)
        {
            {
                var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
                if (admin == null)
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "Session is no longer available";
                    return toReturn;
                }

                db.Configuration.ProxyCreationEnabled = false;

                List<BasketLine> linebasket = db.BasketLines.Include(zz => zz.Product).Include(dd => dd.Basket).ToList();
                return getBasketlinewithProduct(linebasket);
            }
        }
        private List<dynamic> getBasketlinewithProduct(List<BasketLine> forProduct)
        {
            List<dynamic> dymanicProducts = new List<dynamic>();
            foreach (BasketLine product in forProduct)
            {
                dynamic obForProduct = new ExpandoObject();
                obForProduct.ProductID = product.ProductID;
                obForProduct.BasketID = product.BasketID;
                obForProduct.Quantity = product.Quantity;
                obForProduct.Product = getProduct(product.Product);


                dymanicProducts.Add(obForProduct);
            }
            return dymanicProducts;
        }

        private dynamic getProduct(Product Modell)
        {
            dynamic product = new ExpandoObject();
            product.ProductID = Modell.ProductID;
            product.Name = Modell.Name;
            product.Price = Modell.Price;
            product.Description = Modell.Description;

            product.Category = db.ProductCategories.Where(xx => xx.CategoryID == Modell.CategoryID).Select(zz => zz.Category).FirstOrDefault();
            return product;

        }
        private dynamic getBasketlines(Product forProduct)
        {
            List<dynamic> dynamicBasketline = new List<dynamic>();
            foreach (BasketLine Products in forProduct.BasketLines)
            {
                dynamic dynamicBasketlines = new ExpandoObject();
                dynamicBasketlines.BasketID = Products.BasketID;
                dynamicBasketlines.ProductID = Products.ProductID;
                dynamicBasketlines.Product = Products.Product;
                dynamicBasketlines.Quantity = Products.Quantity;
            }

            return dynamicBasketline;
        }

        [Route("api/Sale/GetSaleList")]
        [HttpGet]
        public List<dynamic> GetSaleList()
        {
            {
               // var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
                //if (admin == null)
                //{
                //    dynamic toReturn = new ExpandoObject();
                //    toReturn.Error = "Session is no longer available";
                //    return toReturn;
                //}
                db.Configuration.ProxyCreationEnabled = false;
                return SaleList(db.Sales.Include(zz => zz.SaleLines).ToList());
            }
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
                dynobject.ClientName = db.Clients.Where(zz => zz.ClientID == loop.ClientID).Select(zz => zz.Name).FirstOrDefault();
                dynobject.ClientEmail = db.Clients.Where(zz => zz.ClientID == loop.ClientID).Select(zz => zz.Email).FirstOrDefault();
                dynobject.ClientContact = db.Clients.Where(zz => zz.ClientID == loop.ClientID).Select(zz => zz.ContactNo).FirstOrDefault();
                dynobject.Date = loop.Date;
                dynobject.SaleType = db.SaleTypes.Where(zz => zz.SaleTypeID == loop.SaleTypeID).Select(zz => zz.Type).FirstOrDefault();
                dynobject.Payment = loop.Payment;
                if (loop.ReminderID != null)
                {
                    TimeSpan daysLeft = loop.Date.AddDays(10) - DateTime.Now;
                    if (daysLeft.Days <= 0)
                    {
                        dynobject.Reminder = "sent";
                    }
                    else
                    {
                        dynobject.Reminder = daysLeft.Days;
                    }
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
        [HttpGet]
        public dynamic AddMakeSale(string SessionID)
        {

            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is no longer available";
                return toReturn;
            }

            try
            {
                int ClientID = db.Clients.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.ClientID).FirstOrDefault();

                Sale MakeSale = new Sale();
                MakeSale.ClientID = ClientID;
                MakeSale.StatusID = 1;
                MakeSale.ReminderID = 2;
                MakeSale.Date = DateTime.Now;
                MakeSale.SaleTypeID = 1;
                db.Sales.Add(MakeSale);
                db.SaveChanges();

                int SaleID = MakeSale.SaleID;
                int BasketID = db.Baskets.Where(zz => zz.ClientID == ClientID).Select(zz => zz.BasketID).FirstOrDefault();

                List<BasketLine> getBasket = db.BasketLines.Include(zz => zz.Product).Where(zz => zz.BasketID == BasketID).ToList();

                List<dynamic> Problems = new List<dynamic>();
                foreach (BasketLine items in getBasket)
                {
                    if (items.Product.QuantityOnHand > items.Quantity)
                    {
                        SaleLine AddSaleLine = new SaleLine();
                        AddSaleLine.Quantity = items.Quantity;
                        AddSaleLine.ProductID = items.ProductID;
                        AddSaleLine.SaleID = SaleID;
                        db.SaleLines.Add(AddSaleLine);
                        db.SaveChanges();

                        Product decreaseQuantity = db.Products.Where(zz => zz.ProductID == items.ProductID).FirstOrDefault();
                        decreaseQuantity.QuantityOnHand -= items.Quantity;
                        db.SaveChanges();
                   
                    }
                    else
                    {
                        dynamic problem = new ExpandoObject();
                        problem = "Item: " + items.Product.Name + "(out of stock)";
                        Problems.Add(problem);
                    }
                }
                db.BasketLines.RemoveRange(getBasket);
                db.SaveChanges();
                return "success";
            }
            catch(Exception err)
            {
                return err.Message;
            }

        }
    }
}

        //public static void Email(string sess, int AdminID, string Email)
        //{
        //        {
        //            var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
        //            if (admin == null)
        //            {
        //                dynamic toReturn = new ExpandoObject();
        //                toReturn.Error = "Session is no longer available";
        //                return toReturn;
        //            }
        //        }
        //    try
        //    {
        //        MailMessage message = new MailMessage();
        //        SmtpClient smtp = new SmtpClient();
        //        message.From = new MailAddress("hairexhilaration@gmail.com");
        //        message.To.Add(new MailAddress(Email));
        //        message.Subject = "Exhilartion Hair & Beauty Registration";
        //        message.IsBodyHtml = false;
        //        message.Body = "Click the link below to setup account:" + "/n" + "hhtp://localhost:4200/" + AdminID.ToString();
        //        smtp.Port = 587;
        //        smtp.Host = "smtp.gmail.com";
        //        smtp.EnableSsl = true;
        //        smtp.EnableSsl = true;
        //        smtp.UseDefaultCredentials = false;
        //        smtp.Credentials = new NetworkCredential("hairexhilaration@gmail.com", "@Exhileration1");
        //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        smtp.Send(message);
        //    }
        //    catch (Exception)
        //    {

        //    }

        //}

        //public static string getHtml(string sess, DataGridView grid)
        //    {
        //        {
        //            var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
        //            if (admin == null)
        //            {
        //                dynamic toReturn = new ExpandoObject();
        //                toReturn.Error = "Session is no longer available";
        //                return toReturn;
        //            }
        //            try
        //            {
        //                string messageBody = "<font>The following are Sale Invoice Details: </font><br><br>";
        //                if (grid.RowCount == 0) return messageBody;
        //                string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
        //                string htmlTableEnd = "</table>";
        //                string htmlHeaderRowStart = "<tr style=\"background-color:#6FA1D2; color:#ffffff;\">";
        //                string htmlHeaderRowEnd = "</tr>";
        //                string htmlTrStart = "<tr style=\"color:#555555;\">";
        //                string htmlTrEnd = "</tr>";
        //                string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
        //                string htmlTdEnd = "</td>";
        //                messageBody += htmlTableStart;
        //                messageBody += htmlHeaderRowStart;
        //                messageBody += htmlTdStart + "Client Name" + htmlTdEnd;
        //                messageBody += htmlTdStart + "Email" + htmlTdEnd;
        //                messageBody += htmlTdStart + "Products" + htmlTdEnd;
        //                messageBody += htmlTdStart + "Payment" + htmlTdEnd;
        //                messageBody += htmlHeaderRowEnd;
        //                //Loop all the rows from grid vew and added to html td  
        //                for (int i = 0; i <= grid.RowCount - 1; i++)
        //                {
        //                    messageBody = messageBody + htmlTrStart;
        //                    messageBody = messageBody + htmlTdStart + grid.Rows[i].Cells[0].Value + htmlTdEnd; //adding client name  
        //                    messageBody = messageBody + htmlTdStart + grid.Rows[i].Cells[1].Value + htmlTdEnd; //adding email 
        //                    messageBody = messageBody + htmlTdStart + grid.Rows[i].Cells[2].Value + htmlTdEnd; //adding products
        //                    messageBody = messageBody + htmlTdStart + grid.Rows[i].Cells[3].Value + htmlTdEnd; //adding payment
        //                    messageBody = messageBody + htmlTrEnd;
        //                }
        //                messageBody = messageBody + htmlTableEnd;
        //                return messageBody; // return HTML Table as string from this function  
        //            }
        //            catch (Exception ex)
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //private void btnSent_Click(object sender, EventArgs e)
        //{
        //    string htmlString = getHtml(DataGridView); //here you will be getting an html string  
        //    Email(htmlString); //Pass html string to Email function.  
        //}

    //8.6 Regenerate Sale Invoice 

    


