﻿using System;
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
                //var jg = client;
                User Verify = db.Users.Where(zz => zz.Username == client.Username).FirstOrDefault();
                if (Verify == null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    //var hash = GenerateHash(ApplySomeSalt(client.Password));
                    User clu = new User();
                    clu.Username = client.Username;
                    clu.Password = client.Password;
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
                            cli.UserID = findUser;
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
                    return "Client already created";
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

        [System.Web.Http.Route("api/Clients/getallClients")]
        [System.Web.Mvc.HttpGet]
        public dynamic getallClients(int id)
        {

            db.Configuration.ProxyCreationEnabled = false;
            return db.Clients.Where(zz => zz.ClientID == id).FirstOrDefault();

        }
        private List<dynamic> getClientReturnList(List<Client> ForClient)
        {
            List<dynamic> dymanicClients = new List<dynamic>();
            foreach (Client client in ForClient)
            {
                dynamic dynamicClient = new ExpandoObject();
                dynamicClient.ClientID = client.ClientID;
                dynamicClient.Name = client.Name;
                dynamicClient.Surname = client.Surname;
                dynamicClient.ContactNo = client.ContactNo;
                dynamicClient.Email = client.Email;
                dymanicClients.Add(dynamicClient);
            }
            return dymanicClients;
        }

        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Client/UpdateClient")]
        public IHttpActionResult PutUserMaster([FromBody] Client clienT)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            Client objCli = db.Clients.Find(clienT.ClientID);

            if (objCli != null)
            {
                objCli.Name = clienT.Name;
                objCli.Surname = clienT.Surname;
                objCli.ContactNo = clienT.ContactNo;
                objCli.Email = clienT.Email;
                objCli.ClientID = clienT.ClientID;

                db.SaveChanges();

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
        [System.Web.Http.Route("api/Client/getClientPackage")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getClientPackage()
        {
            ExperTechEntities db = new ExperTechEntities();
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
            dynamicServicePackage.Description = service.Description;
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
                dynamicInstancePackage.Date = pack.Date;
                dynamicInstancePackage.SaleID = pack.SaleID;
                dynamicInstancePackage.StatusID = pack.StatusID;
                InstanceStatu stat = db.InstanceStatus.Where(zz => zz.StatusID == pack.StatusID).FirstOrDefault();
                dynamicInstancePackage.Status = stat.Status;

                dymanicinstances.Add(dynamicInstancePackage);

            }

            return dymanicinstances;
        }

        private string getStatus(InstanceStatu Stat)
        {

            return Stat.Status;
        }


        //basket functionality 
        [System.Web.Http.Route("api/Client/addtBasketline")]
        [System.Web.Mvc.HttpPost]

        public void addtBasketline(BasketLine forProduct)
        {


            db.BasketLines.Add(forProduct);
            db.SaveChanges();

        }
        [System.Web.Mvc.HttpPost]
        [System.Web.Http.Route("api/Client/AddProductTobasket")]
        public IHttpActionResult Addbasket(BasketLine forbasket)
        {
            db.Configuration.ProxyCreationEnabled = false;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                db.BasketLines.Add(forbasket);
                db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }



            return Ok(forbasket);
        }



        //[System.Web.Http.Route("api/Basket/getClientBasket")]
        //[System.Web.Mvc.HttpG
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


        [System.Web.Http.Route("api/Client/getBasketlinewithProduct")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getBasketlinewithProduct()
        {
            ExperTechEntities db = new ExperTechEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<BasketLine> linebasket = db.BasketLines.Include(zz => zz.Product).Include(dd => dd.Basket).ToList();
            return getBasketlinewithProduct(linebasket);

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
            product.Photo = getPhotos(Modell);
            return product;

        }

        private dynamic getPhotos(Product Modell)
        {
            List<dynamic> myphotos = new List<dynamic>();
            foreach (ProductPhoto item in Modell.ProductPhotoes)
            {
                dynamic photos = new ExpandoObject();
                photos.Image = item.Photo;

            }

            return myphotos;
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
        [System.Web.Http.Route("api/Client/DeleteClientBasket")]
        public dynamic DeleteClientBasket(BasketLine sbasket)
        {
            ExperTechEntities db = new ExperTechEntities();
            db.Configuration.ProxyCreationEnabled = false;
            BasketLine basket = db.BasketLines.Where(zz => zz.ProductID == sbasket.ProductID && zz.BasketID == sbasket.BasketID).FirstOrDefault();
            if (basket == null)
            {
                return NotFound();
            }

            db.BasketLines.Remove(basket);
            db.SaveChanges();

            return "Success";
        }

        [System.Web.Mvc.HttpDelete]
        [System.Web.Http.Route("api/Client/DeleteClientBooking")]
        public IHttpActionResult DeleteClientBooking(Booking booking)
        {
            ExperTechEntities db = new ExperTechEntities();
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
        [System.Web.Http.Route("api/Client/AcceptClientBooking")]
        public IHttpActionResult AcceptClientBooking(Booking booking)
        {
            ExperTechEntities db = new ExperTechEntities();
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