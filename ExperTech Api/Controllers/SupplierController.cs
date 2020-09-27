using Microsoft.Ajax.Utilities;
using ExperTech_Api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;

namespace ExperTech_Api.Controllers
{
    public class SupplierController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Supplier/GetSupplierList")]
        [HttpGet]

        public List<dynamic> GetSupplierList()
        {
            //var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
            //if (admin == null)
            //{
            //    dynamic toReturn = new ExpandoObject();
            //    toReturn.Error = "Session is no longer available";
            //    return toReturn;
            //}

            db.Configuration.ProxyCreationEnabled = false;
            return SupplierList(db.Suppliers.Where(zz => zz.Deleted == false).ToList());


        }

        private List<dynamic> SupplierList(List<Supplier> Model1)
        {
            List<dynamic> newlist = new List<dynamic>();
            foreach (Supplier loop in Model1)
            {
                dynamic dynobject = new ExpandoObject();
                dynobject.SupplierID = loop.SupplierID;
                dynobject.Name = loop.Name;
                dynobject.ContactNo = loop.ContactNo;
                dynobject.Email = loop.Email;
                dynobject.Address = loop.Address;
                newlist.Add(dynobject);


            }
            return newlist;
        }

        [Route("api/Supplier/UpdateSupplier")]
        [HttpPut]
        public dynamic UpdateSupplier(string SessionID, [FromBody] Supplier UpdateObject)
        {
            {
                var admin = db.Users.Where(zz => zz.SessionID == SessionID).ToList();
                if (admin == null)
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "Session is no longer available";
                    return toReturn;
                }
                try
                {
                    Supplier findSupplier = db.Suppliers.Where(zz => zz.SupplierID == UpdateObject.SupplierID).FirstOrDefault();
                    findSupplier.Name = UpdateObject.Name;
                    findSupplier.ContactNo = UpdateObject.ContactNo;
                    findSupplier.Email = UpdateObject.Email;
                    findSupplier.Address = UpdateObject.Address;
                    db.SaveChanges();
                    return "success";

                }
                catch
                {
                    return "Supplier details are invalid";
                }
            }
        }


        [Route("api/Supplier/DeleteSupplier")]
        [HttpDelete]
        public dynamic DeleteSupplier(int SupplierID, string SessionID)
        {
            
             var admin = db.Users.Where(zz => zz.SessionID == SessionID).ToList();
             if (admin == null)
             {
                  dynamic toReturn = new ExpandoObject();
                  toReturn.Error = "Session is no longer available";
                  return GetSupplierList();
             }
            

            db.Configuration.ProxyCreationEnabled = false;
            List<Product> findProds = db.Products.Where(zz => zz.SupplierID == SupplierID).ToList();
            List<SupplierOrder> findSuppOrder = db.SupplierOrders.Where(zz => zz.SupplierID == SupplierID).ToList();

            if (findProds.Count == 0)
            {
                Supplier findSupplier = db.Suppliers.Find(SupplierID);
                if (findSuppOrder.Count == 0 )
                {
                    db.Suppliers.Remove(findSupplier);
                    db.SaveChanges();
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Message = "success";
                    return toReturn;
                }
                else
                {
                    findSupplier.Deleted = true;
                    db.SaveChanges();
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Message = "success";
                    return toReturn;
                }
               
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                int countProd = findProds.Count;
                toReturn.Error = "dependencies";
                toReturn.Message = "There are " + countProd.ToString() + " Products that depend this Supplier. \nDelete those products first.";
                return toReturn;
            }
        }


        [Route("api/Supplier/GetSupplierOrderList")]
        [HttpGet]
        public List<dynamic> GetSupplierOrderList()
        {
            //{
            //    var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
            //    if (admin == null)
            //    {
            //        dynamic toReturn = new ExpandoObject();
            //        toReturn.Error = "Session is no longer available";
            //        return toReturn;
            //    }
                db.Configuration.ProxyCreationEnabled = false;
                return SupplierOrderList(db.SupplierOrders.Include(zz => zz.StockItemLines).ToList());
            
        }

        private dynamic SupplierOrderList(List<SupplierOrder> Modell)
        {
            List<dynamic> newObject = new List<dynamic>();
            foreach (SupplierOrder Items in Modell)
            {
                dynamic OrderObject = new ExpandoObject();
                OrderObject.OrderID = Items.OrderID;
                OrderObject.SupplierID = Items.SupplierID;
                string SupplierName = db.Suppliers.Where(zz => zz.SupplierID == Items.SupplierID).Select(zz => zz.Name).FirstOrDefault();
                OrderObject.Supplier = SupplierName;
                OrderObject.Description = Items.Description;
                OrderObject.Price = Items.Price;
                OrderObject.Date = Items.Date;

                List<dynamic> stockItem = new List<dynamic>();

                foreach (StockItemLine Line in Items.StockItemLines)
                {
                    dynamic LineObject = new ExpandoObject();
                    LineObject.ItemID = Line.ItemID;
                    LineObject.SupplierID = Line.OrderID;
                    LineObject.Quantity = Line.Quantity;
                    string findItem = db.StockItems.Where(zz => zz.ItemID == Line.ItemID).Select(zz => zz.Name).FirstOrDefault();
                    LineObject.Items = findItem;
                    stockItem.Add(LineObject);
                }
                OrderObject.StockItemLines = stockItem;
                newObject.Add(OrderObject);

            }

            return newObject;
        }


        [Route("api/Supplier/AddSupplierOrder")]
        [HttpPost]
        public dynamic AddSupplierOrder([FromBody] SupplierOrder Items, string SessionID)
        {

            var admin = db.Users.Where(zz => zz.SessionID == SessionID).ToList();
            if (admin == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is not valid";
                return toReturn;
            }

            SupplierOrder newObject = new SupplierOrder();
            newObject.SupplierID = Items.SupplierID;
            newObject.Description = Items.Description;
            decimal total = 0;
            foreach (StockItemLine details in Items.StockItemLines)
            {
                decimal findPrice = db.StockItems.Where(zz => zz.ItemID == details.ItemID).Select(zz => zz.Price).FirstOrDefault(); ;
                total += (decimal)(details.Quantity * findPrice);
            }
            newObject.Price = total;
            newObject.Date = DateTime.Now;

            db.SupplierOrders.Add(newObject);
            db.SaveChanges();
           

            int OrderID = newObject.OrderID;

            List<StockItemLine> lineList = new List<StockItemLine>();
            foreach (StockItemLine details in Items.StockItemLines)
            {
                StockItemLine addItems = new StockItemLine();
                addItems.ItemID = details.ItemID;
                addItems.Quantity = details.Quantity;
                addItems.OrderID = OrderID;
                lineList.Add(addItems);
            }
            db.StockItemLines.AddRange(lineList);
            db.SaveChanges();
            return "success";

        }




        [Route("api/Supplier/DeleteSupplierOrder")]
        [HttpDelete]
        public dynamic DeleteSupplierOrder(int OrderID)
        {
            //{
            //    var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
            //    if (admin == null)
            //    {
            //        dynamic toReturn = new ExpandoObject();
            //        toReturn.Error = "Session is no longer available";
            //        return toReturn;
            //    }
            SupplierOrder findOrder = db.SupplierOrders.Where(zz => zz.OrderID == OrderID).FirstOrDefault();

            //check the to list stuff
            foreach (StockItemLine lines in findOrder.StockItemLines.ToList())
            {
                db.StockItemLines.Remove(lines);
                db.SaveChanges();
            }
            db.SupplierOrders.Remove(findOrder);
            db.SaveChanges();
            return "sucess";


        }



        [Route("api/Supplier/AddSupplier")]
        [HttpPost]
        public dynamic AddSupplier([FromBody] Supplier AddObject, string SessionID)
        {

            var admin = db.Users.Where(zz => zz.SessionID == SessionID).ToList();
            if (admin == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is no longer available";
                return toReturn;
            }

            if (AddObject != null)
            {
                Supplier findSupplier = db.Suppliers.Where(zz => zz.Name == AddObject.Name).FirstOrDefault();
                if (findSupplier == null)
                {
                    db.Suppliers.Add(AddObject);
                    db.SaveChanges();
                    return "success";
                }
                else
                {
                    return "duplicate";
                }
            }
            else
            {
                return null;
            }
        }
    }


    //        public IQueryable<Supplier> GetSuppliers()
    //        {
    //            return db.Suppliers;
    //        }

    //        [ResponseType(typeof(Supplier))]
    //        public IHttpActionResult GetSupplier(string sess, int SupplierID)
    //        {
    //            var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
    //               if (User == null)
    //               {
    //                 return BadRequest();
    //               }
    //            Supplier supplier = db.Suppliers.Find(SupplierID);
    //            if (supplier == null)
    //            {
    //                return NotFound();

    //            }

    //            return Ok(supplier);
    //        }

    //        [ResponseType(typeof(void))]
    //        public IHttpActionResult PutSupplier(string sess, int SupplierID, Supplier supplier)
    //        {
    //           var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
    //           if (admin == null)
    //           {
    //             dynamic toReturn = new ExpandoObject();
    //             toReturn.Error = "Session is no longer available";
    //             return toReturn;
    //           }
    //             if (!ModelState.IsValid)
    //            {
    //              return BadRequest(ModelState);
    //            }

    //            db.Entry(supplier).State = EntityState.Modified;

    //            try
    //            {
    //                db.SaveChanges();
    //            }
    //            catch (DBConcurrencyException)
    //            {
    //                if (!SupplierExists(SupplierID))
    //                {
    //                    return NotFound();
    //                }
    //                else
    //                {
    //                    throw;
    //                }
    //            }

    //            return StatusCode(HttpStatusCode.NoContent);
    //        }

    //        [ResponseType(typeof(Supplier))]
    //        public IHttpActionResult PostSupplier(string sess, Supplier supplier)
    //        {
    //            {
    //                var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
    //                if (admin == null)
    //                {
    //                    dynamic toReturn = new ExpandoObject();
    //                    toReturn.Error = "Session is no longer available";
    //                    return toReturn;
    //                }
    //            }
    //            if (!ModelState.IsValid)
    //            {
    //                return BadRequest(ModelState);

    //            }

    //            db.Suppliers.Add(supplier);

    //            try
    //            {
    //                db.SaveChanges();
    //            }
    //            catch (DbUpdateException)
    //            {
    //                if (SupplierExists(supplier.SupplierID))
    //                {
    //                    return Conflict();

    //                }
    //                else
    //                {
    //                    throw;
    //                }
    //            }

    //            return CreatedAtRoute("DefaultAPI", new { SupplierID = supplier.SupplierID }, supplier);
    //        }

    //        [ResponseType(typeof(Supplier))]
    //        public IHttpActionResult Supplier(string sess, int SupplierID)
    //        {
    //            {
    //                var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
    //                if (admin == null)
    //                {
    //                    dynamic toReturn = new ExpandoObject();
    //                    toReturn.Error = "Session is no longer available";
    //                    return toReturn;
    //                }
    //            }
    //            Supplier supplier = db.Suppliers.Find(SupplierID);
    //            if (supplier == null)
    //            {
    //                return NotFound();
    //            }

    //            db.Suppliers.Remove(supplier);
    //            db.SaveChanges();

    //            return Ok(supplier);

    //        }

    //        protected override void Dispose(bool disposing)
    //        {
    //            if(disposing)
    //            {
    //                db.Dispose();
    //            }
    //            base.Dispose(disposing);
    //        }

    //        private bool SupplierExists(int SupplierID)
    //        {
    //            return db.Suppliers.Count(e => e.SupplierID == SupplierID) > 0;
    //        }

    //    }
    //}
}