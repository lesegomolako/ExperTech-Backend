using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExperTech_Api.Models;
using System.Dynamic;
using System.Web.Http.Cors;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;



namespace ExperTech_Api.Controllers
{
    [RoutePrefix("api/Clients")]
    public class ClientController : ApiController
    {
        private ExperTechEntities db = new ExperTechEntities();
        //***********************************add client (make booking)***************************
        [Route("AddClient")]
        [HttpPost]
        public dynamic AddClient([FromBody]Client Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            dynamic toReturn = new ExpandoObject();
            try
            {
                Client findClient = db.Clients.Where(zz => zz.Name == Modell.Name && zz.Surname == Modell.Surname).FirstOrDefault();
                if (findClient == null)
                {
                    db.Clients.Add(Modell);
                    db.SaveChanges();
                    toReturn.Message = "success";
                    toReturn.Client = Modell;
                    return toReturn;
                }
                else
                {
                    toReturn.Message = "duplicate";
                    toReturn.Client = findClient;
                    return toReturn;
                }    
            }
            catch
            {
                return toReturn.Message = "Error saving client details";
            }
        }

        //************************************read client*****************************************
        [Route("getClient")]
        [HttpGet]
        public dynamic getClient()
        {
            db.Configuration.ProxyCreationEnabled = false;
            
            //User findUser = db.Users.Where(zz => zz.SessionID == sessionID).FirstOrDefault();

     
            return getClientID(db.Clients.ToList());
            
         
        }
        private List<dynamic> getClientID(List<Client> forClient)
        {
            List<dynamic> dynamicClients = new List<dynamic>();
            foreach (Client clientname in forClient)
            {
                dynamic dynamicClient = new ExpandoObject();
                dynamicClient.ClientID = clientname.ClientID;
                dynamicClient.Name = clientname.Name;
                dynamicClient.Surname = clientname.Surname;
                dynamicClient.ContactNo = clientname.ContactNo;
                dynamicClient.Email = clientname.Email;

                dynamicClients.Add(dynamicClient);
            }
            return dynamicClients;

        }

        //*****************************************update admin****************************************
        [Route("updateClient")]
        [System.Web.Mvc.HttpPost]

        public IHttpActionResult updateClient([FromBody] Client forClient)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Client adminzz = db.Clients.Find(forClient.ClientID);

                if (adminzz != null)
                {
                    adminzz.Name = forClient.Name;
                    adminzz.Surname = forClient.Surname;
                    adminzz.Email = forClient.Email;
                    adminzz.ContactNo = forClient.ContactNo;
                    adminzz.UserID = forClient.UserID;

                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forClient);
        }

        //***************delete client from system, i really didn't have to say from system but i did it anyways, lmao************************
        [Route("deleteClient")]
        [HttpDelete]
        public List<dynamic> deleteClient([FromBody] Client forClient)
        {
            if (forClient != null)
            {
                db.Configuration.ProxyCreationEnabled = false;

                // Client clientThings = db.Clients.Where(rr => rr.ClientID == forClient.ClientID).FirstOrDefault();
                User userThings = db.Users.Where(rr => rr.UserID == forClient.UserID).FirstOrDefault();

                // db.Clients.Remove(clientThings);
                db.Users.Remove(userThings);
                db.SaveChanges();

                return getClient();
            }
            else
            {
                return null;
            }
        }
        [System.Web.Mvc.HttpPost]
        [System.Web.Http.Route("registerUser")]
        public object registerUser([FromBody] User client)
        {

            try
            {
                dynamic toReturn = new ExpandoObject();
                //var jg = client;
                User Verify = db.Users.Where(zz => zz.Username == client.Username).FirstOrDefault();
                if (Verify == null)
                {

                    db.Configuration.ProxyCreationEnabled = false;
                    var hash = UserController.GenerateHash(UserController.ApplySomeSalt(client.Password));
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
                    toReturn.Message = "success";
                    toReturn.SessionID = clu.SessionID;
                    toReturn.RoleID = clu.RoleID;
                    return toReturn;
                }
                else
                {
                    return toReturn.Error = "Client details already exist";
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }




        [System.Web.Http.Route("getALLClientsWithUser")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLClientsWithUser()
        {

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

        [System.Web.Http.Route("getallClients")]
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
        [System.Web.Http.Route("UpdateClient")]
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



        //View service package 
        [System.Web.Http.Route("getClientPackage")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getClientPackage()
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
                obForPackage.TotalAvailable = db.PackageInstances.Where(zz => zz.PackageID == CLINETPAKCAGE.PackageID && zz.StatusID == 1).Count();
                obForPackage.ServicePackage = getServicePackage(CLINETPAKCAGE.ServicePackage);
                obForPackage.InstancePackage = getInstancePackage(CLINETPAKCAGE);


                dymanicPackages.Add(obForPackage);
            }
            return dymanicPackages;
        }
        private dynamic getServicePackage(ServicePackage service)
        {


            dynamic dynamicServicePackage = new ExpandoObject();
            dynamicServicePackage.Name = db.Services.Where(zz => zz.ServiceID == service.ServiceID).Select(zz => zz.Name).FirstOrDefault();
            dynamicServicePackage.PackageID = service.PackageID;
            dynamicServicePackage.Quantity = service.Quantity;

            return dynamicServicePackage;
        }
        private dynamic getInstancePackage(ClientPackage service)
        {
            List<dynamic> dymanicinstances = new List<dynamic>();
            //int Total = 0;
            foreach (PackageInstance pack in service.PackageInstances)
            {
                dynamic dynamicInstancePackage = new ExpandoObject();
                dynamicInstancePackage.PackageID = pack.PackageID;
                dynamicInstancePackage.Date = pack.Date;
                dynamicInstancePackage.SaleID = pack.SaleID;
                dynamicInstancePackage.StatusID = pack.StatusID;
                InstanceStatu stat = db.InstanceStatus.Where(zz => zz.StatusID == pack.StatusID).FirstOrDefault();
                dynamicInstancePackage.Status = stat.Status;
                //if (stat.Status == "Active")
                //    Total++;

                //dynamicInstancePackage.TotalAvailable = Total;

                dymanicinstances.Add(dynamicInstancePackage);

            }

            return dymanicinstances;
        }

        private string getStatus(InstanceStatu Stat)
        {

            return Stat.Status;
        }

        [System.Web.Http.Route("getALLProductsWithPhoto")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLProductsWithPhoto()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Product> product = db.Products.Include(zz => zz.ProductPhotoes).Include(ii => ii.ProductCategory).ToList();
            return getALLProductssWithPhoto(product);

        }
        private List<dynamic> getALLProductssWithPhoto(List<Product> forProduct)
        {
            List<dynamic> dymanicProducts = new List<dynamic>();
            foreach (Product PRODUCT in forProduct)
            {
                dynamic obForProduct = new ExpandoObject();
                obForProduct.ProductID = PRODUCT.ProductID;
                obForProduct.SupplierID = PRODUCT.SupplierID;
                obForProduct.CategoryID = PRODUCT.CategoryID;
                obForProduct.Name = PRODUCT.Name;
                obForProduct.Description = PRODUCT.Description;
                obForProduct.Price = PRODUCT.Price;
                obForProduct.QuantityOnHand = PRODUCT.QuantityOnHand;
                obForProduct.Category = PRODUCT.ProductCategory.Category;
                obForProduct.Photo = getProductPhotos(PRODUCT);

                dymanicProducts.Add(obForProduct);
            }
            return dymanicProducts;
        }
        private dynamic getProductCategorys(ProductCategory productCategory)
        {


            dynamic dynamicProductCategory = new ExpandoObject();
            dynamicProductCategory.CategoryID = productCategory.CategoryID;
            dynamicProductCategory.Category = productCategory.Category;

            return dynamicProductCategory;
        }

        private dynamic getProductPhotos(Product forProduct)
        {
            List<dynamic> myphotos = new List<dynamic>();
            foreach (ProductPhoto item in forProduct.ProductPhotoes)
            {
                dynamic photos = new ExpandoObject();
                photos.Image = item.Photo;

            }

            return myphotos;
        }




        //basket functionality 
        [System.Web.Http.Route("addtBasketline")]
        [System.Web.Mvc.HttpPost]

        public void addtBasketline(string SessionID, [FromBody] BasketLine forProduct)
        {

            int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz => zz.UserID).FirstOrDefault();
            if (findUser != 0)
            {
                int ClientID = db.Clients.Where(zz => zz.UserID == findUser).Select(zz => zz.ClientID).FirstOrDefault();
                int BasketID = db.Baskets.Where(zz => zz.ClientID == ClientID).Select(zz => zz.BasketID).FirstOrDefault();
                BasketLine findBasket = db.BasketLines.Where(zz => zz.BasketID == BasketID && zz.ProductID == forProduct.ProductID).FirstOrDefault();
                Debug.Write("Adding Product", forProduct.Quantity.ToString());
                if (findBasket == null)
                {
                    BasketLine newBasket = new BasketLine();
                    newBasket.BasketID = BasketID;
                    newBasket.ProductID = forProduct.ProductID;
                    newBasket.Quantity = forProduct.Quantity;
                    db.BasketLines.Add(newBasket);
                    db.SaveChanges();
                }
                else
                {
                    if (forProduct.Quantity == 0)
                    {
                        db.BasketLines.Remove(findBasket);
                    }
                    else
                        findBasket.Quantity += forProduct.Quantity;


                    db.SaveChanges();
                }

            }
         


        }
        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("Updatebasketline")]
        public IHttpActionResult Updatebasketline([FromBody]BasketLine line, string SessionID)
        {

            db.Configuration.ProxyCreationEnabled = false;


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz => zz.UserID).FirstOrDefault();
            if (findUser != 0)
            {
                try
                {
                    BasketLine objEmp = db.BasketLines.Where(zz => zz.BasketID == line.BasketID && zz.ProductID == line.ProductID).FirstOrDefault();

                    if (objEmp != null)
                    {
                        objEmp.Quantity = line.Quantity;
                        db.SaveChanges();


                    }


                }
                catch (Exception)
                {
                    throw;
                }
                return Ok(line);
            }
            else
            {
                return BadRequest("Session is not valid");
            }
        }

        [Route("getBasketlinewithProduct")]
        [System.Web.Mvc.HttpGet]
        public dynamic getBasketlinewithProduct(string SessionID)
        {

            db.Configuration.ProxyCreationEnabled = false;
            int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz=> zz.UserID).FirstOrDefault();
            if (findUser != 0)
            {
                int ClientID = db.Clients.Where(zz => zz.UserID == findUser).Select(zz => zz.ClientID).FirstOrDefault();
                List<BasketLine> linebasket = db.BasketLines.Include(zz => zz.Product).Include(dd => dd.Basket).Where(zz => zz.Basket.ClientID == ClientID).ToList();
                return getBasketlinewithProduct(linebasket);
            }
            else
            {
                return "Session not valid";
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
            List<ProductPhoto> findPhotos = db.ProductPhotoes.Where(zz => zz.ProductID == Modell.ProductID).ToList();
            product.Photo = getPhotos(findPhotos);
            return product;

        }

        private dynamic getPhotos(List<ProductPhoto> Modell)
        {
            List<dynamic> myphotos = new List<dynamic>();
            //foreach (ProductPhoto item in Modell.ProductPhotoes)
            //{
            //    dynamic photos = new ExpandoObject();
            //    photos.Image = item.Photo;

            //}

            foreach (ProductPhoto photo in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.Photo = photo.Photo;               
                string filePath = HttpContext.Current.Server.MapPath("~/Images/" + photo.Photo);
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            fileStream.CopyTo(memoryStream);
                            Bitmap image = new Bitmap(1, 1);
                            image.Save(memoryStream, ImageFormat.Png);

                            byte[] byteImage = memoryStream.ToArray();
                            string base64String = Convert.ToBase64String(byteImage);
                            newObject.Image = "data:image/png;base64," + base64String;

                        }
                    }
                }
                catch
                {
                    newObject.Image = "";
                }
                myphotos.Add(newObject);
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
        [System.Web.Http.Route("DeleteClientBasket")]
        public dynamic DeleteClientBasket(int BasketID, int ProductID, string SessionID)
        {
            db.Configuration.ProxyCreationEnabled = false;

            int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz => zz.UserID).FirstOrDefault();
            if (findUser != 0)
            {
                BasketLine basket = db.BasketLines.Where(zz => zz.ProductID == ProductID && zz.BasketID == BasketID).FirstOrDefault();
                if (basket == null)
                {
                    return NotFound();
                }

                db.BasketLines.Remove(basket);
                db.SaveChanges();
                return "sucess";
            }
            else
            {
                return "Session is not valid";
            }
        }





        [System.Web.Mvc.HttpDelete]
        [System.Web.Http.Route("DeleteClientBooking")]
        public IHttpActionResult DeleteClientBooking(int id)
        {

            db.Configuration.ProxyCreationEnabled = false;
            Booking bookings = db.Bookings.Where(zz => zz.BookingID == id).FirstOrDefault();

            bookings.StatusID = 3;
            db.SaveChanges();

            foreach (EmployeeSchedule emschedule in bookings.EmployeeSchedules)
            {
                EmployeeSchedule bookinglist = db.EmployeeSchedules.Where(zz => zz.EmployeeID == emschedule.EmployeeID
                && zz.DateID == emschedule.DateID && zz.TimeID == emschedule.TimeID).FirstOrDefault();
                if (bookinglist != null)
                {
                    bookinglist.StatusID = 1;
                    bookinglist.BookingID = null;
                    db.SaveChanges();
                }

            }
            return Ok(id);
        }

        [System.Web.Mvc.HttpDelete]
        [System.Web.Http.Route("CancelClientBooking")]
        public IHttpActionResult CancelClientBooking(int id)
        {

            db.Configuration.ProxyCreationEnabled = false;
            Booking bookings = db.Bookings.Where(zz => zz.BookingID == id).FirstOrDefault();

            bookings.StatusID = 5;
            db.SaveChanges();

            foreach (EmployeeSchedule emschedule in bookings.EmployeeSchedules)
            {
                EmployeeSchedule bookinglist = db.EmployeeSchedules.Where(zz => zz.EmployeeID == emschedule.EmployeeID
                && zz.DateID == emschedule.DateID && zz.TimeID == emschedule.TimeID).FirstOrDefault();
                if (bookinglist != null)
                {
                    bookinglist.StatusID = 1;
                    bookinglist.BookingID = null;
                    db.SaveChanges();
                }

            }
            return Ok(id);
        }





        //the one the client does
        [HttpPost]
        [Route("AcceptClientsBooking")]
        public dynamic AcceptClientsBooking(int bookingID)
        {
            try
            {
                Booking bookings = db.Bookings.Where(zz => zz.BookingID == bookingID).FirstOrDefault();

                bookings.StatusID = 4;
                db.SaveChanges();

                EmployeeSchedule bookinglist = db.EmployeeSchedules.Where(zz => zz.BookingID == bookingID).FirstOrDefault();
                if (bookinglist != null)
                {
                    bookinglist.StatusID = 3;
                    bookinglist.BookingID = bookingID;
                    db.SaveChanges();

                }
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        [Route("getBadge")]
        [HttpGet]
        public int getBadge(string SessionID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            Client findUser = db.Clients.Where(zz => zz.User.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                int BasketID = db.Baskets.Where(zz => zz.ClientID == findUser.ClientID ).Select(zz => zz.BasketID).FirstOrDefault();
                List<BasketLine> findLine = db.BasketLines.Where(zz => zz.BasketID == BasketID).ToList();
                return findLine.Sum(zz => zz.Quantity);
            }
            else
            {
                return 0;
            }
           
        }

        ////[System.Web.Mvc.HttpDelete]
        //[System.Web.Http.Route("api/Clients/AccpetClientBooking")]
        //public IHttpActionResult AccpetClientBooking(int id)
        //{

        //    db.Configuration.ProxyCreationEnabled = false;
        //    Booking bookings = db.Bookings.Where(zz => zz.BookingID == id).FirstOrDefault();

        //    bookings.StatusID = 4;
        //    db.SaveChanges();

        //    foreach (EmployeeSchedule emschedule in bookings.EmployeeSchedules)
        //    {
        //        EmployeeSchedule bookinglist = db.EmployeeSchedules.Where(zz => zz.EmployeeID == emschedule.EmployeeID
        //        && zz.DateID == emschedule.DateID && zz.TimeID == emschedule.TimeID).FirstOrDefault();
        //        if (bookinglist != null)
        //        {
        //            bookinglist.StatusID = 1;
        //            bookinglist.BookingID = null;
        //            db.SaveChanges();
        //        }

        //    }
        //    return Ok(id);
        //}

        [Route("RetrieveBookings")]
        [HttpGet]
        public dynamic RetrieveBookings()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> findBookings = db.Bookings.Include(zz => zz.BookingLines).Include(zz => zz.EmployeeSchedules).Include(zz => zz.Client)
                .Include(zz => zz.DateRequesteds).Include(zz => zz.BookingNotes).ToList();
            return formatBookings(findBookings);
        }

        private dynamic formatBookings(List<Booking> Modell)
        {
            List<dynamic> BookingList = new List<dynamic>();
            foreach (Booking items in Modell)
            {
                if (items.StatusID == 1 || items.StatusID == 4 || items.StatusID == 5 || items.StatusID == 2)
                {
                    dynamic BookingObject = new ExpandoObject();
                    BookingObject.BookingID = items.BookingID;
                    BookingObject.BookingStatusID = items.StatusID;
                    BookingObject.BookingStatus = db.BookingStatus.Where(zz => zz.StatusID == items.StatusID).Select(zz => zz.Status).FirstOrDefault(); ;
                    BookingObject.Client = items.Client.Name;

                    if (items.StatusID == 1)
                    {
                        foreach (DateRequested requests in items.DateRequesteds)
                        {
                            dynamic requestObject = new ExpandoObject();
                            requestObject.RequestedID = requests.RequestedID;
                            requestObject.Dates = requests.Date;
                            requestObject.Time = requests.StartTime;
                            DateTime makeDT = (DateTime)requests.Date + (TimeSpan)requests.StartTime;
                            requestObject.DateTime = makeDT;

                            BookingObject.BookingRequest = requestObject;
                        }
                    }

                    List<dynamic> getSchedule = new List<dynamic>();
                    foreach (EmployeeSchedule booking in items.EmployeeSchedules)
                    {
                        dynamic scheduleObject = new ExpandoObject();  //can you change employee after confirmed booking && where does the advise get saved
                        scheduleObject.DateID = booking.DateID;
                        scheduleObject.Employee = db.Employees.Where(zz => zz.EmployeeID == booking.EmployeeID).Select(zz => zz.Name).FirstOrDefault();

                        DateTime getDate = db.Dates.Where(zz => zz.DateID == booking.DateID).Select(zz => zz.Date1).FirstOrDefault();
                        scheduleObject.Dates = getDate;

                        TimeSpan getTime = db.Timeslots.Where(zz => zz.TimeID == booking.TimeID).Select(zz => zz.StartTime).FirstOrDefault();
                        scheduleObject.StartTime = getTime;

                        scheduleObject.EndTime = db.Timeslots.Where(zz => zz.TimeID == booking.TimeID).Select(zz => zz.EndTime).FirstOrDefault();
                        scheduleObject.Status = db.ScheduleStatus.Where(zz => zz.StatusID == booking.StatusID).Select(zz => zz.Status).FirstOrDefault();

                        DateTime makeDT = getDate + getTime;
                        scheduleObject.DateTime = makeDT;
                        getSchedule.Add(scheduleObject);
                    }
                    if (getSchedule.Count != 0)
                        BookingObject.BookingSchedule = getSchedule;

                    List<dynamic> getLines = new List<dynamic>();
                    foreach (BookingLine lineItems in items.BookingLines)
                    {
                        dynamic lineObject = new ExpandoObject();
                        lineObject.Service = db.Services.Where(zz => zz.ServiceID == lineItems.ServiceID).Select(zz => zz.Name).FirstOrDefault(); ;
                        lineObject.Option = db.ServiceOptions.Where(zz => zz.OptionID == lineItems.OptionID).Select(zz => zz.Name).FirstOrDefault(); ;
                 
                        getLines.Add(lineObject);
                    }
                    BookingObject.BookingLines = getLines;


                    BookingList.Add(BookingObject);
                }
            }

            return BookingList;
        }

    }

}
