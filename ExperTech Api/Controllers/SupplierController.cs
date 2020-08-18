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
        private ExperTechEntities db = new ExperTechEntities();

        [Route("api/Supplier/GetSupplierList")]
        [HttpGet]

        public List<dynamic> GetSupplierList()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return SupplierList(db.Suppliers.ToList());


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
        public List<dynamic> UpdateSupplier([FromBody] Supplier UpdateObject)
        {
            if (UpdateObject != null)
            {
                Supplier findSupplier = db.Suppliers.Where(zz => zz.SupplierID == UpdateObject.SupplierID).FirstOrDefault();
                findSupplier.Name = UpdateObject.Name;
                findSupplier.ContactNo = UpdateObject.ContactNo;
                findSupplier.Email = UpdateObject.Email;
                findSupplier.Address = UpdateObject.Address;
                db.SaveChanges();
                return GetSupplierList();

            }
            else
            {
                return null;
            }
        }

        [Route("api/Supplier/DeleteSupplier")]
        [HttpDelete]
        public List<dynamic> DeleteSupplier(int SupplierID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            Supplier findSupplier = db.Suppliers.Find(SupplierID);
            db.Suppliers.Remove(findSupplier);
            db.SaveChanges();
            return GetSupplierList();
        }

        [Route("api/Supplier/AddSupplierOrder")]
        [HttpPost]
        public dynamic AddSupplierOrder([FromBody] SupplierOrder Items)
        {
            SupplierOrder newObject = new SupplierOrder();
            newObject.OrderID = Items.OrderID;
            newObject.SupplierID = Items.SupplierID;
            newObject.Description = Items.Description;
            newObject.Price = Items.Price;

            db.SaveChanges();

            int OrderID = db.SupplierOrders.Where(zz => zz.Description == Items.Description).Select(zz => zz.OrderID).FirstOrDefault();

            foreach (StockItemLine details in Items.StockItemLines)
            {
                StockItemLine addItems = new StockItemLine();
                addItems.ItemID = details.ItemID;
                addItems.Quantity = details.Quantity;
                addItems.OrderID = details.OrderID;


                db.StockItemLines.Add(addItems);
                db.SaveChanges();

            }
            return "sucess";

        }

        [Route("api/Supplier/DeleteSupplierOrder")]
        [HttpDelete]
        public dynamic DeleteSupplierOrder(int OrderID)
        {
            SupplierOrder findOrder = db.SupplierOrders.Where(zz => zz.OrderID == OrderID).FirstOrDefault();

            foreach (StockItemLine lines in findOrder.StockItemLines)
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
        public dynamic AddSupplier([FromBody] Supplier AddObject)
        {
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

        public IQueryable<Supplier> GetSuppliers()
        {
            return db.Suppliers;
        }

        [ResponseType(typeof(Supplier))]
        public IHttpActionResult GetSupplier(int SupplierID)
        {
            Supplier supplier = db.Suppliers.Find(SupplierID);
            if (supplier == null)
            {
                return NotFound();

            }

            return Ok(supplier);
        }

        [ResponseType(typeof(void))]
        public IHttpActionResult PutSupplier(int SupplierID, Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Entry(supplier).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DBConcurrencyException)
            {
                if (!SupplierExists(SupplierID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [ResponseType(typeof(Supplier))]
        public IHttpActionResult PostSupplier(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }

            db.Suppliers.Add(supplier);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (SupplierExists(supplier.SupplierID))
                {
                    return Conflict();

                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultAPI", new { SupplierID = supplier.SupplierID }, supplier);
        }

        [ResponseType(typeof(Supplier))]
        public IHttpActionResult Supplier(int SupplierID)
        {
            Supplier supplier = db.Suppliers.Find(SupplierID);
            if (supplier == null)
            {
                return NotFound();
            }

            db.Suppliers.Remove(supplier);
            db.SaveChanges();

            return Ok(supplier);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SupplierExists(int SupplierID)
        {
            return db.Suppliers.Count(e => e.SupplierID == SupplierID) > 0;
        }

    }
}
