using ExperTech_Api.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
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

        [Route("api/Sale/CancelSale")]
        [HttpDelete]
        public dynamic CancelSale(int SaleID, string SessionID, int? AuthorizeID)
        {
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser == null)
            {
                return UserController.SessionError();
            }

            try
            {
                Sale findSale = db.Sales.Where(zz => zz.SaleID == SaleID).FirstOrDefault();

                List<SaleLine> lines2delete = new List<SaleLine>();
                foreach(SaleLine items in findSale.SaleLines)
                {
                    Product findProduct = db.Products.Where(zz => zz.ProductID == items.ProductID).FirstOrDefault();
                    findProduct.QuantityOnHand += items.Quantity;
                    db.SaveChanges();

                    lines2delete.Add(items);
                }
                if(lines2delete.Count >0)
                    db.SaleLines.RemoveRange(lines2delete);

                db.Sales.Remove(findSale);
                db.SaveChanges();
                return "success";
            }
            catch(Exception err)
            {
                return err.Message;
            }
        }

        [Route("api/Sale/GetProductSales")]
        [HttpGet]
        public dynamic GetProductSales()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Sale> getSales = db.Sales.Include(zz => zz.SaleLines).Where(zz => zz.SaleTypeID == 1 && zz.Payment == null).ToList();
            return SaleList(getSales);
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
                dynobject.ClientSurname = db.Clients.Where(zz => zz.ClientID == loop.ClientID).Select(zz => zz.Surname).FirstOrDefault();
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
                    newObject.Name = findProds.Name;
                    newObject.Price = findProds.Price;
                    newObject.Quantity = items.Quantity;
                    saleThings.Add(newObject);
                }
                dynobject.Products = saleThings;
                newlist.Add(dynobject);


            }
            return newlist;
        }

        [Route("api/Sale/SalePayment")]
        [HttpPost]
        public dynamic SalePayment(dynamic SaleDetails)
        {
            try
            {
                string SessionID = SaleDetails.SessionID;
                User findUser = UserController.CheckUser(SessionID);
                if(findUser != null)
                {
                    int SaleID = (int)SaleDetails.SaleID;
                    int PaymentTypeID = (int)SaleDetails.PaymentTypeID;
                    decimal Total = (decimal)SaleDetails.Price;

                    Sale findSale = db.Sales.Find(SaleID);
                    findSale.PaymentTypeID = PaymentTypeID;
                    findSale.Payment = Total;
                    db.SaveChanges();

                    return "success";
                }
                else
                {
                    return UserController.SessionError();
                }
            }
            catch(Exception err)
            {
                return err.Message;
            }
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
                Client findClient = db.Clients.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();

                Sale MakeSale = new Sale();
                MakeSale.ClientID = findClient.ClientID ;
                MakeSale.StatusID = 1;
                MakeSale.ReminderID = 2;
                MakeSale.Date = DateTime.Now;
                MakeSale.SaleTypeID = 1;
                db.Sales.Add(MakeSale);
                db.SaveChanges();

                int SaleID = MakeSale.SaleID;
                int BasketID = db.Baskets.Where(zz => zz.ClientID == findClient.ClientID).Select(zz => zz.BasketID).FirstOrDefault();

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
                string name = findClient.Name + " " + findClient.Surname;

                string table = FormatTable(SaleID);


                string body = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/EmailTemplates/SaleInvoice.html"));
                body = body.Replace("#Name#", name).Replace("#Table#", table);

                UserController.Email(body, findClient.Email, "Sale Invoice");

                string smsBody = "Exhilaration Beauty & Hair Salon: Your booking has been advised. Log in to website to Accept or Reject Booking.";
                string cellNo = db.Clients.Where(zz => zz.ClientID == findClient.ClientID).Select(zz => zz.ContactNo).FirstOrDefault(); ;
                if (cellNo != "")
                    UserController.SMS(smsBody, cellNo);

                return "success";
            }
            catch(Exception err)
            {
                return err.Message;
            }

        }

        private string FormatTable(int SaleID)
        {
            string messageBody = "";
            Sale Modell = db.Sales.Include(zz => zz.SaleLines).Include(zz => zz.Client).Where(zz => zz.SaleID == SaleID).FirstOrDefault();
            if (Modell.SaleLines.Count == 0) return "";

            //< table class="table">    
            //                <thead>    
            //                    <tr>    
            //                        <th width = "25%" > Product Name</th>    
            //                        <th width = "25%" > Price(R) </ th >
            //                        < th width="25%">Quantity</th>       
            //                    </tr>    
            //                </thead>    
            //                <tbody>    
            //                    <tr* ngFor = "let item of clientObject.Products" >
            //                        < td scope="row">
            //                        {{item.Name}}</td>    
            //                        <td>R{{item.Price}}</td>    
            //                        <td>{{item.Quantity}}</td>    
            //                    </tr> 
            //                    <tr>
            //                        <td></td>
            //                        <td><strong>Total</strong></td>
            //                        <td style = "border-bottom-style: double;" > 100 </ td >
            //                    </ tr >
            //                </ tbody >
            //            </ table >

            string htmlTableStart = "<table style='border-collapse:collapse; text-align:center;' >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style='background-color:#6FA1D2; color:#ffffff;'>";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style='color:#555555;'>";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style='border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;'>";
            string htmlTdEnd = "</td>";
            string htmlTotalTdStart = "<td style='border-color:#5c87b2; border-bottom-style:double; border-width:thin; padding: 5px;'>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Product Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Price" + htmlTdEnd;
            messageBody += htmlTdStart + "Quantity" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            decimal Total = 0;
            //Loop all the rows from grid vew and added to html td  
            foreach (SaleLine items in Modell.SaleLines)
            {
                Product findProduct = db.Products.Where(zz => zz.ProductID == items.ProductID).FirstOrDefault();
                messageBody = messageBody + htmlTrStart;
                messageBody = messageBody + htmlTdStart + findProduct.Name + htmlTdEnd; //adding student name  
                messageBody = messageBody + htmlTdStart + findProduct.Price + htmlTdEnd; //adding DOB  
                messageBody = messageBody + htmlTdStart + items.Quantity + htmlTdEnd; //adding Email   
                messageBody = messageBody + htmlTrEnd;

                Total += (findProduct.Price * items.Quantity);
            }
            messageBody += htmlTrStart;
            messageBody += htmlTdStart + "" + htmlTdEnd;
            messageBody += htmlTdStart + "Total" + htmlTdEnd;
            messageBody += htmlTotalTdStart + Total + htmlTdEnd;
            messageBody += htmlTrEnd;

            messageBody = messageBody + htmlTableEnd;

            return messageBody;
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

    


