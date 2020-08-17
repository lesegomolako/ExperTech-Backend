using ExperTech_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
        [HttpPut]
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

    }
}
