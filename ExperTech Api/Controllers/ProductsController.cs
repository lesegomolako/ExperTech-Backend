using ExperTech_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using Microsoft.Ajax.Utilities;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExperTech_Api.Controllers
{
    public class ProductsController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();



        [Route("api/Products/AddProduct")]
        [HttpPost]
        public dynamic AddProduct()
        {
            var httpRequest = HttpContext.Current.Request;
            string imageName = "";
            //string SessionID = httpRequest["SessionID"];
            //int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz => zz.UserID).FirstOrDefault();

            //if (findUser != 0)
            //{
                try
                {
                    var postedFile = httpRequest.Files["Image"];
                    imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(postedFile.FileName.Length).ToArray()).Replace(" ", "-");
                    imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var FilePath = HttpContext.Current.Server.MapPath("~/Images/" + imageName);
                    postedFile.SaveAs(FilePath);
                }
                catch (Exception err)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Image was not saved (" + err.Message + ")");
                }

                try
                {
                    string name = httpRequest["Name"];
                    Product verify = db.Products.Where(zz => zz.Name == name).FirstOrDefault();
                    if (verify == null)
                    {
                        Product newProd = new Product();

                        newProd.Name = httpRequest["Name"];
                        newProd.Description = httpRequest["Description"];
                        newProd.CategoryID = Convert.ToInt32(httpRequest["CategoryID"]);
                        newProd.Price = Convert.ToDecimal(httpRequest["Price"]);
                        newProd.SupplierID = Convert.ToInt32(httpRequest["SupplierID"]);
                        newProd.QuantityOnHand = Convert.ToInt32(httpRequest["QuantityOnHand"]);

                        db.Products.Add(newProd);
                        db.SaveChanges();
                        db.Entry(newProd).GetDatabaseValues();
                        int ProdID = newProd.ProductID;

                        ProductPhoto photo = db.ProductPhotoes.Where(zz => zz.ProductID == ProdID).FirstOrDefault();
                        if (photo == null)
                        {
                            ProductPhoto addPhoto = new ProductPhoto();
                            addPhoto.ProductID = ProdID;
                            addPhoto.Photo = imageName;

                            db.ProductPhotoes.Add(addPhoto);
                            db.SaveChanges();
                        }
                        else
                        {
                            photo.ProductID = ProdID;
                            photo.Photo = imageName;

                            db.SaveChanges();
                        }


                        return "success";
                    }
                    else
                    {
                        dynamic toReturn = new ExpandoObject();
                        toReturn.Error = "duplicate";
                        toReturn.Data = verify;
                        return "duplicate";
                    }
                }
                catch
                {
                    return "Product details are invalid";
                }

            //}
            //else
            //{
            //    dynamic toReturn = new ExpandoObject();
            //    toReturn.Error = "session";
            //    toReturn.Message = "Session is not valid";
            //    return "Session is not valid";
            //}

        }

        [Route("api/Products/GetProduct")]
        [HttpGet]
        public IHttpActionResult GetProduct()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return GetProductz(db.Products.Include(zz => zz.ProductPhotoes).Include(zz => zz.ProductCategory).Where(zz => zz.Deleted == false).ToList());
        }

        private IHttpActionResult GetProductz(List<Product> Modell)
        {
            List<dynamic> ProductList = new List<dynamic>();
            foreach (Product items in Modell)
            {
                dynamic ProdObject = new ExpandoObject();
                ProdObject.ProductID = items.ProductID;
                ProdObject.Name = items.Name;
                ProdObject.Description = items.Description;
                ProdObject.Price = items.Price;
                ProdObject.QuantityOnHand = items.QuantityOnHand;
                ProdObject.SupplierID = items.SupplierID;
                ProdObject.CategoryID = items.CategoryID;
                ProdObject.Category = items.ProductCategory.Category;


                List<dynamic> photoList = new List<dynamic>();
                foreach (ProductPhoto photo in items.ProductPhotoes)
                {
                    dynamic newObject = new ExpandoObject();
                    newObject.PhotoID = photo.PhotoID;
                    newObject.Photo = photo.Photo;
                    photoList.Add(newObject);
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
                                ProdObject.Image = "data:image/png;base64," + base64String;
                                
                            }
                        }
                    }
                    catch
                    {
                        ProdObject.Image = "";
                    }
                }
                ProdObject.Photos = photoList;
                ProductList.Add(ProdObject);
            }
           // HttpResponseMessage response = Request.CreateResponse(ProductList);
            return Ok(ProductList) ;
        }


        [Route("api/Products/UpdateProduct")]
        [HttpPost]
        public dynamic UpdateProduct([FromBody] Product Modell)
        {
            Product findProduct = db.Products.Where(zz => zz.ProductID == Modell.ProductID).FirstOrDefault();
            findProduct.Name = Modell.Name;
            findProduct.Price = Modell.Price;
            findProduct.QuantityOnHand = Modell.QuantityOnHand;
            findProduct.Description = Modell.Description;
            db.SaveChanges();
            return "success";
        }

        [Route("api/Products/DeleteProduct")]
        [HttpDelete]
        public dynamic DeleteProduct(int ProductID)
        {
            Product findProduct = db.Products.Where(zz => zz.ProductID == ProductID).FirstOrDefault();
            List<SaleLine> findLines = db.SaleLines.Where(zz => zz.ProductID == findProduct.ProductID).ToList();
            if(findLines.Count != 0)
            {
                findProduct.Deleted = true;
                db.SaveChanges();
                return "success";
            }
            db.ProductPhotoes.RemoveRange(findProduct.ProductPhotoes);
            db.Products.Remove(findProduct);
            db.SaveChanges();
            return "success";
        }

        [Route("api/Products/GetProducts")]
        [HttpGet]
        public dynamic GetProducts()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Product> findProduct = db.Products.Include(zz => zz.ProductCategory)
                .Include(zz => zz.ProductPhotoes).Include(zz => zz.Supplier).ToList();

            return GetProds(findProduct);
        }

        private dynamic GetProds(List<Product> Modell)
        {
            List<dynamic> myList = new List<dynamic>();
            foreach (Product items in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.ProductID = items.ProductID;
                newObject.Name = items.Name;
                newObject.QuantityOnHand = items.QuantityOnHand;
                newObject.Description = items.Description;
                newObject.Price = items.Price;
                newObject.Category = items.ProductCategory.Category;
                newObject.Supplier = items.Supplier.Name;
                newObject.CategoryID = items.CategoryID;
                newObject.SupplierID = items.SupplierID;
                newObject.Photos = getPhotos(items);

                myList.Add(newObject);
            }
            return myList;
        }

        private dynamic getPhotos(Product Modell)
        {
            List<dynamic> mylist = new List<dynamic>();
            foreach (ProductPhoto items in Modell.ProductPhotoes)
            {
                dynamic newObject = new ExpandoObject();
                newObject.PhotoID = items.PhotoID;
                newObject.Photo = items.Photo;

                mylist.Add(newObject);
            }

            return mylist;
        }

        [Route("api/Products/getSuppliers")]
        [HttpGet]
        public List<Supplier> getSuppliers()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Supplier> getSupplier = db.Suppliers.ToList();
            return getSupplier;
        }

        [Route("api/Products/getCategories")]
        [HttpGet]
        public List<ProductCategory> getCategories()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ProductCategory> getCategory = db.ProductCategories.ToList();
            return getCategory;
        }

        //[HttpGet]
        //[Route("api/Products/populateDates")]
        //public dynamic populateDates()
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    List<dynamic> DateList = new List<dynamic>();
        //    List<Date> DatesList = new List<Date>();

        //    for (int j = 1; j < 107; j++)
        //    {
        //        DateTime getDate = Convert.ToDateTime("2020 - 10 - 04");
        //        dynamic newObject = new ExpandoObject();
        //        newObject.nDate = getDate.AddDays(j);
        //        DateList.Add(newObject);

        //        Date Modell = new Date();
        //        Modell.Date1 = getDate.AddDays(j);
        //        DatesList.Add(Modell);

        //    }
        //    db.Dates.AddRange(DatesList);
        //    db.SaveChanges();
        //    return DateList;
        //}

        //[HttpGet]
        //[Route("api/Products/populateTimes")]
        //public dynamic populateTimes()
        //{
        //    db.Configuration.ProxyCreationEnabled = false;

        //    List<Date> DatesList = db.Dates.ToList();
        //    List<Schedule> newList = new List<Schedule>();
        //    for (int j = 1; j < DatesList.Count; j++)
        //    {
        //        List<Schedule> newSchedge = db.Schedules.Where(zz => zz.DateID == j).ToList();
        //        if (newSchedge.Count == 0)
        //        {
        //            List<Timeslot> newTimes = db.Timeslots.ToList();
        //            for (int k=1; k < newTimes.Count; k++)
        //            {
        //                Schedule newTime = new Schedule();
        //                newTime.DateID = j;
        //                newTime.TimeID = k;
        //                newList.Add(newTime);
        //            }
        //        }

        //    }
        //    db.Schedules.AddRange(newList);
        //    db.SaveChanges();
        //    return newList;
        //}

        //[HttpGet]
        //[Route("api/Products/populateEmployeeTimes")]
        //public dynamic populateTimes()
        //{
        //    db.Configuration.ProxyCreationEnabled = false;

        //    List<Schedule> ScheduleList = db.Schedules.ToList();
        //    List<EmployeeSchedule> newList = new List<EmployeeSchedule>();
        //    for (int j = 0; j < ScheduleList.Count; j++)
        //    {
        //        int thisDateID = ScheduleList[j].DateID;
        //        int thisTimeID = ScheduleList[j].TimeID;
        //        EmployeeSchedule newSchedge = db.EmployeeSchedules.Where(zz => zz.DateID == thisDateID && zz.TimeID == thisTimeID).FirstOrDefault();
        //        if (newSchedge == null)
        //        {
        //            EmployeeSchedule items = new EmployeeSchedule();
        //            items.EmployeeID = 1;
        //            items.DateID = thisDateID;
        //            items.TimeID = thisTimeID;
        //            items.StatusID = 1;
        //            newList.Add(items);
        //        }

        //    }
        //    db.EmployeeSchedules.AddRange(newList);
            
        //    db.SaveChanges();
        //    return "success";
        //}
    }
}
