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

namespace ExperTech_Api.Controllers
{
    public class ProductsController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Products/AddProduct")]
        [HttpPost]
        public dynamic AddProduct([FromBody] Product Modell)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                Product findProduct = db.Products.Where(zz => zz.Name == Modell.Name).FirstOrDefault();
                if (findProduct == null)
                {
                    db.Products.Add(Modell);
                    db.SaveChanges();

                    int ProductID = db.Products.Where(zz => zz.Name == Modell.Name).Select(zz => zz.ProductID).FirstOrDefault();

                    if (Modell.ProductPhotoes != null)
                    {
                        foreach (ProductPhoto Items in Modell.ProductPhotoes)
                        {
                            ProductPhoto Images = new ProductPhoto();
                            Images.ProductID = ProductID;
                            Images.Photo = Items.Photo;
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

            foreach(ProductPhoto photo in findProduct.ProductPhotoes)
            {
                db.ProductPhotoes.Remove(photo);
                db.SaveChanges();
            }

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
            foreach(Product items in Modell)
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
            foreach(ProductPhoto items in Modell.ProductPhotoes)
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
    }
}
