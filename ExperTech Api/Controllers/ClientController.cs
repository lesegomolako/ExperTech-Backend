using System;
using System.Collections.Generic;
using System.Dynamic;
using ExperTech_Api.Models;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;
using System.Text;

namespace ExperTech_Api.Controllers
{
    public class ClientController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();
        [System.Web.Mvc.HttpPost]
        [System.Web.Http.Route("api/Clients/registerUser")]
        public object registerUser([FromBody] User client)
        {
            try
            {
                User Verify = db.Users.Where(zz => zz.Username == client.Username).FirstOrDefault();
                if (Verify == null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var hash = GenerateHash(ApplySomeSalt(client.Password));
                    User clu = new User();
                    clu.Username = client.Username;
                    clu.Password = hash;
                    clu.RoleID = 1;
                    Guid g = Guid.NewGuid();
                    clu.SessionID = g.ToString();
                    db.Users.Add(clu);
                    db.SaveChanges();

                    int findUser = db.Users.Where(zz => zz.Username == client.Username).Select(zz => zz.UserID).FirstOrDefault();



                    foreach (Client items in client.Clients)
                    {
                        Client Verfiy = db.Clients.Where(zz => zz.Name == items.Name && zz.Surname == items.Surname && zz.Email == items.Email).FirstOrDefault();
                        if (Verify == null)
                        {
                            Client cli = new Client();
                            cli.Name = items.Name;
                            cli.Surname = items.Surname;
                            cli.ContactNo = items.ContactNo;
                            cli.Email = items.Email;
                            cli.UserID = items.UserID;
                            db.Clients.Add(cli);
                            db.SaveChanges();

                            int find = db.Clients.Where(zz => zz.Name == items.Name && zz.Surname == items.Surname && zz.Email == items.Email).Select(zz => zz.ClientID).FirstOrDefault();

                            Basket CreateBasket = new Basket();
                            CreateBasket.ClientID = find;
                            db.Baskets.Add(CreateBasket);
                            db.SaveChanges();
                        }


                    }
                    return "success";
                }
                else
                {
                    return "duplicate";
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }


        public static string ApplySomeSalt(string input)
        {
            return input + "abcdefghijklmnopqrsuotwxyz0123456789ABCDEFGHIJKLMNOPQRSOUTWXYZ";
        }
        public static string GenerateHash(string inputString)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);

        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSUTWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

        }
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        [System.Web.Http.Route("api/Client/getALLClientsWithUser")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLClientsWithUser(dynamic sess)
        {
            ExperTechEntities db = new ExperTechEntities();
            string sessionId = sess.token;
            var user = db.Users.Where(zz => zz.SessionID == sessionId).FirstOrDefault();
            db.Configuration.ProxyCreationEnabled = false;
            List<Client> CLIENT = db.Clients.Include(zz => zz.User).ToList();
            return getALLClientsWithUser(CLIENT);

        }

        private List<dynamic> getALLClientsWithUser(List<Client> forClient)
        {
            List<dynamic> dymanicClients = new List<dynamic>();
            foreach (Client CLIENT in forClient)
            {
                dynamic obForClient = new ExpandoObject();
                obForClient.ID = CLIENT.ClientID;
                obForClient.Name = CLIENT.Name;
                obForClient.Surname = CLIENT.Surname;
                obForClient.ContactNo = CLIENT.ContactNo;
                obForClient.Email = CLIENT.Email;
                obForClient.User = getUsers(CLIENT.User);


                dymanicClients.Add(obForClient);
            }
            return dymanicClients;
        }
        private dynamic getUsers(User CLIENT1)
        {


            dynamic dynamicUser = new ExpandoObject();
            dynamicUser.UserID = CLIENT1.UserID;
            dynamicUser.Username = CLIENT1.Username;
            return dynamicUser;
        }

        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Client/UpdateClient")]
        public IHttpActionResult PutUserMaster([FromBody] dynamic clienT)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Client objCli = db.Clients.Find(clienT.ClientID);

                if (objCli != null)
                {
                    objCli.Name = clienT.Name;
                    objCli.Surname = clienT.Surname;
                    objCli.ContactNo = clienT.ContactNo;
                    objCli.Email = clienT.Email;
                    objCli.UserID = clienT.UserID;

                }

            }
            catch (Exception)
            {
                throw;
            }
            return Ok(clienT);
        }

        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/User/ForgotPassword")]
        public IHttpActionResult PutUserMaster(User useR)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var hash = GenerateHash(ApplySomeSalt(useR.Password));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User objuse = db.Users.Find(useR.UserID);


                if (objuse != null)
                {
                    objuse.Password = hash;
                    Guid g = Guid.NewGuid();



                }


                int i = this.db.SaveChanges();

            }
            catch (Exception)
            {
                throw;
            }
            return Ok(useR);

        }
        //View service package 
        [System.Web.Http.Route("api/ClientPackage/getClientPackagewithWervicePackage")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getClientPackagewithWervicePackage(int id)
        {

            db.Configuration.ProxyCreationEnabled = false;
            List<ClientPackage> CLINETPAKCAGE = db.ClientPackages.Include(zz => zz.ServicePackage).Include(ii => ii.PackageInstances).ToList();
            return getClientPackagewithWervicePackage(CLINETPAKCAGE);

        }
        private List<dynamic> getClientPackagewithWervicePackage(List<ClientPackage> forPAckage)
        {
            List<dynamic> dymanicPackages = new List<dynamic>();
            foreach (ClientPackage CLINETPAKCAGE in forPAckage)
            {
                dynamic obForPackage = new ExpandoObject();
                obForPackage.SaleID = CLINETPAKCAGE.SaleID;
                obForPackage.PackageID = CLINETPAKCAGE.PackageID;
                obForPackage.Date = CLINETPAKCAGE.Date;
                obForPackage.ExpiryDate = CLINETPAKCAGE.ExpiryDate;
                obForPackage.ServicePackage = getServicePackage(CLINETPAKCAGE.ServicePackage);
                obForPackage.getInstancePackage = getInstancePackage(CLINETPAKCAGE);


                dymanicPackages.Add(obForPackage);
            }
            return dymanicPackages;
        }
        private dynamic getServicePackage(ServicePackage service)
        {


            dynamic dynamicServicePackage = new ExpandoObject();
            dynamicServicePackage.PackageID = service.PackageID;
            dynamicServicePackage.Quantity = service.Quantity;
            return dynamicServicePackage;
        }
        private dynamic getInstancePackage(ClientPackage service)
        {
            List<dynamic> dymanicinstances = new List<dynamic>();
            foreach (PackageInstance pack in service.PackageInstances)
            {
                dynamic dynamicInstancePackage = new ExpandoObject();
                dynamicInstancePackage.PackageID = pack.PackageID;
                dynamicInstancePackage.SaleID = pack.SaleID;
                dynamicInstancePackage.StatusID = pack.StatusID;
                dynamicInstancePackage.Status = getStatus(pack.InstanceStatu);

            }

            return dymanicinstances;
        }

        private string getStatus(InstanceStatu Stat)
        {
            return Stat.Status;
        }


        //basket functionality 
        [System.Web.Http.Route("api/Product/addtBasketline")]
        [System.Web.Mvc.HttpPost]

        private void addtBasketline(BasketLine forProduct)
        {


            db.BasketLines.Add(forProduct);
            db.SaveChanges();

        }




        //[System.Web.Http.Route("api/Basket/getClientBasket")]
        //[System.Web.Mvc.HttpGet]
        //public List<dynamic> getClientBasket()
        //{
        //    ExperTechEntities2 db = new ExperTechEntities2();
        //    db.Configuration.ProxyCreationEnabled = false;
        //    return getBasketReturnList(db.BasketLines.ToList());

        //}
        //private List<dynamic> getBasketReturnList(List<BasketLine> ForBasketline)
        //{
        //    List<dynamic> dymanicBasketLines = new List<dynamic>();
        //    foreach (BasketLine basketlinE in ForBasketline)
        //    {
        //        dynamic dymanicBasketLine = new ExpandoObject();
        //        dymanicBasketLine.ID = basketlinE.BasketID;
        //        dymanicBasketLine.ProductID = basketlinE.ProductID;
        //        dymanicBasketLine.Quantity = basketlinE.Quantity;

        //        dymanicBasketLines.Add(dymanicBasketLine);
        //    }
        //    return dymanicBasketLines;
        //}


        [System.Web.Http.Route("api/Product/getBasketlinewithProduct")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getBasketlinewithProduct()
        {

            db.Configuration.ProxyCreationEnabled = false;
            List<Product> PRODUCT = db.Products.Include(zz => zz.BasketLines).Include(ii => ii.ProductCategory).Include(dd => dd.ProductPhotoes).ToList();
            return getBasketlinewithProduct(PRODUCT);

        }
        private List<dynamic> getBasketlinewithProduct(List<Product> forProduct)
        {
            List<dynamic> dymanicProducts = new List<dynamic>();
            foreach (Product product in forProduct)
            {
                dynamic obForProduct = new ExpandoObject();
                obForProduct.ProductID = product.ProductID;
                obForProduct.Name = product.Name;
                obForProduct.ProductCategory = product.ProductCategory;
                obForProduct.Price = product.Price;
                obForProduct.ProductPhotoes = product.ProductPhotoes;
                obForProduct.Description = product.Description;
                obForProduct.CategoryID = product.CategoryID;
                obForProduct.BasketLine = getBasketlines(product);
                obForProduct.ProductCategory = getProductCategory(product.ProductCategory);
                obForProduct.ProductPhoto = getProductPhoto(product);

                dymanicProducts.Add(obForProduct);
            }
            return dymanicProducts;
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
        private dynamic getProductCategory(ProductCategory productCategory)
        {


            dynamic dynamicProductCategory = new ExpandoObject();
            dynamicProductCategory.CategoryID = productCategory.CategoryID;
            dynamicProductCategory.Category = productCategory.Category;

            return dynamicProductCategory;
        }
        private dynamic getProductPhoto(Product forProduct)
        {
            List<dynamic> dynamicProductPhoto = new List<dynamic>();
            foreach (ProductPhoto Products in forProduct.ProductPhotoes)
            {
                dynamic dynamicProductPhotos = new ExpandoObject();
                dynamicProductPhotos.PhotoID = Products.PhotoID;
                dynamicProductPhotos.Photo = Products.Photo;
            }


            return dynamicProductPhoto;
        }

        [System.Web.Mvc.HttpDelete]
        [System.Web.Http.Route("api/BasketLine/DeleteClientBasket")]
        public IHttpActionResult DeleteClientBasket(BasketLine sbasket)
        {

            db.Configuration.ProxyCreationEnabled = false;
            BasketLine basket = db.BasketLines.Where(zz => zz.ProductID == sbasket.ProductID && zz.BasketID == sbasket.BasketID).FirstOrDefault();
            if (basket == null)
            {
                return NotFound();
            }

            db.BasketLines.Remove(basket);
            db.SaveChanges();

            return Ok(basket);
        }

        [System.Web.Mvc.HttpDelete]
        [System.Web.Http.Route("api/Booking/DeleteClientBooking")]
        public IHttpActionResult DeleteClientBooking(Booking booking)
        {

            db.Configuration.ProxyCreationEnabled = false;
            Booking bookings = db.Bookings.Where(zz => zz.BookingID == booking.BookingID && zz.ClientID == booking.ClientID).FirstOrDefault();

            bookings.StatusID = 3;
            db.SaveChanges();

            foreach (EmployeeSchedule emschedule in booking.EmployeeSchedules)
            {
                EmployeeSchedule bookinglist = db.EmployeeSchedules.Where(zz => zz.TypeID == emschedule.TypeID && zz.EmployeeID == emschedule.EmployeeID
                && zz.DateID == emschedule.DateID && zz.TimeID == emschedule.TimeID).FirstOrDefault();
                if (bookinglist != null)
                {
                    emschedule.StatusID = 1;
                    db.SaveChanges();
                }

            }


            return Ok(booking);
        }

        //the one the client does
        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Booking/AcceptClientBooking")]
        public IHttpActionResult AcceptClientBooking(Booking booking)
        {

            db.Configuration.ProxyCreationEnabled = false;
            Booking bookings = new Booking();
            db.Configuration.ProxyCreationEnabled = false;

            bookings.BookingID = booking.BookingID;
            bookings.ClientID = booking.ClientID;
            bookings.StatusID = 1;
            foreach (EmployeeSchedule emschedule in booking.EmployeeSchedules)
            {
                EmployeeSchedule bookinglist = db.EmployeeSchedules.Where(zz => zz.TypeID == emschedule.TypeID && zz.EmployeeID == emschedule.EmployeeID
                && zz.DateID == emschedule.DateID && zz.TimeID == emschedule.TimeID).FirstOrDefault();
                if (bookinglist != null)
                {
                    emschedule.StatusID = emschedule.StatusID;
                    db.SaveChanges();
                }

            }
            return Ok(bookings);
        }






    }
}
