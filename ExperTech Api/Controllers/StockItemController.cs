using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ExperTech_Api.Models;
using System.Dynamic;
using System.Security.Cryptography;

namespace ExperTech_Api.Controllers
{
    public class StockItemController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Stockitem/GetStockitemList")]
        [HttpGet]

        public List<dynamic> GetStockItemList()
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
                return StockItemList(db.StockItems.ToList());
            }
        

        private List<dynamic> StockItemList(List<StockItem> Model1)
        {
            List<dynamic> newlist = new List<dynamic>();
            foreach (StockItem loop in Model1)
            {
                dynamic dynobject = new ExpandoObject();
                dynobject.ItemID = loop.ItemID;
                dynobject.Name = loop.Name;
                dynobject.Description = loop.Description;
                dynobject.Price = loop.Price;
                dynobject.QuantityInStock = loop.QuantityInStock;
                newlist.Add(dynobject);


            }
            return newlist;
        }

        [Route("api/StockItem/UpdateStockItem")]
        [HttpPut]
        public dynamic UpdateStockItem([FromBody] StockItem UpdateObject, string SessionID)
        {
            var admin = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(admin == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is not valid";
                return toReturn;
            }

            if (UpdateObject != null)
            {
                StockItem findStockItem = db.StockItems.Where(zz => zz.ItemID == UpdateObject.ItemID).FirstOrDefault();
                findStockItem.Name = UpdateObject.Name;
                findStockItem.Description = UpdateObject.Description;
                findStockItem.Price = UpdateObject.Price;
                findStockItem.QuantityInStock = UpdateObject.QuantityInStock;
                db.SaveChanges();
                return "success";

            }
            else
            {
                return null;
            }
        }

        

        [Route("api/StockItem/DeleteStockItem")]
        [HttpDelete]
        public object DeleteStockItem(string SessionID, int ItemID)
        {

            var admin = db.Users.Where(zz => zz.SessionID == SessionID).ToList();
            if (admin == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is not valid";
                return toReturn;
            }
            db.Configuration.ProxyCreationEnabled = false;
            StockItem findStockItem = db.StockItems.Find(ItemID);
            List<StockTakeLine> findTakeLine = db.StockTakeLines.Where(zz => zz.ItemID == ItemID).ToList();
            List<WriteOffLine> findWriteOffLine = db.WriteOffLines.Where(zz => zz.ItemID == ItemID).ToList();
            List<StockItemLine> findItemLine = db.StockItemLines.Where(zz => zz.ItemID == ItemID).ToList();

            if (findTakeLine.Count == 0 && findWriteOffLine.Count == 0 && findItemLine.Count == 0)
            {
                db.StockItems.Remove(findStockItem);
                db.SaveChanges();
                return "sucess";
            }
            else
            {
                return null;  //HEY LOOK HERE
            }

        }

        [Route("api/StockItem/AddStockItem")]
        [HttpPost]
        public dynamic AddStockItem(string SessionID, [FromBody] StockItem AddObject)
        {
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
                    StockItem findStockItem = db.StockItems.Where(zz => zz.Name == AddObject.Name).FirstOrDefault();
                    if (findStockItem == null)
                    {
                        db.StockItems.Add(AddObject);
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

        [Route("api/StockTake/AddStockTake")]
        [HttpPost]
        public dynamic AddStocktake(string SessionID, [FromBody] StockTake AddObject)
        {
            {
                User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
                if (findUser == null)
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "Session is no longer available";
                    return toReturn;
                }

                if (AddObject != null)
                {
                    Admin findAdmin = db.Admins.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                    if (findAdmin != null)
                    {
                        StockTake Takes = new StockTake();
                        Takes.AdminID = findAdmin.AdminID;
                        Takes.Description = AddObject.Description;
                        Takes.Date = DateTime.Now;
                        db.StockTakes.Add(Takes);
                        db.SaveChanges();

                        int StockTakeID = Takes.StockTakeID;

                        foreach (StockTakeLine lines in AddObject.StockTakeLines)
                        {
                            StockTakeLine newObject = new StockTakeLine();
                            newObject.ItemID = lines.ItemID;
                            newObject.StockTakeID = StockTakeID;
                            newObject.Quantity = lines.Quantity;
                            db.StockTakeLines.Add(newObject);
                            db.SaveChanges();

                            StockItem updateStock = db.StockItems.Where(zz => zz.ItemID == lines.ItemID).FirstOrDefault();
                            updateStock.QuantityInStock = lines.Quantity;
                            db.SaveChanges();
                        }
                        return "success";
                    }
                    else
                    {
                        return "User not found";
                    }

                }
                else
                {

                    return null;
                }
            }
        }

        [Route("api/StockTake/AddStockWriteOff")]
        [HttpPost]
        public dynamic AddStockWriteOff(string SessionID, [FromBody] StockWriteOff AddObject)
        {

            User admin = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (admin == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is not valid";
                return toReturn;
            }

            if (AddObject != null)
            {

                StockWriteOff Writes = new StockWriteOff();

                Writes.Description = AddObject.Description;
                Writes.Date = DateTime.Now;
                db.StockWriteOffs.Add(Writes);
                db.SaveChanges();
               
                int WriteOffID = Writes.WriteOffID;

                foreach (WriteOffLine lines in AddObject.WriteOffLines)
                {
                    WriteOffLine newObject = new WriteOffLine();
                    newObject.ItemID = lines.ItemID;
                    newObject.WriteOffID = WriteOffID;
                    newObject.Reason = lines.Reason;
                    newObject.Quantity = lines.Quantity;
                    db.WriteOffLines.Add(newObject);
                    db.SaveChanges();

                    StockItem updateStock = db.StockItems.Where(zz => zz.ItemID == lines.ItemID).FirstOrDefault();
                    updateStock.QuantityInStock -= lines.Quantity;
                    db.SaveChanges();
                }
                return "success";

            }
            else
            {

                return null;
            }

        }
        public IQueryable<StockItem> GetStockItems()
        {
            return db.StockItems;
        }

        [ResponseType(typeof(StockItem))]
        public IHttpActionResult GetStockItem(string sess, int ItemID)
        {
            {
                var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
                if (User == null)
                {
                    return BadRequest();
                }
                StockItem stockitem = db.StockItems.Find(ItemID);
                if (stockitem == null)
                {
                    return NotFound();

                }

                return Ok(stockitem);
            }
        }

        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutStockItem(string sess, int ItemID, StockItem stockitem)
        //{
        //    {
        //        var admin = db.Users.Where(zz => zz.SessionID == sess).ToList();
        //        if (User == null)
        //        {
        //            return BadRequest();
        //        }
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        db.Entry(stockitem).State = EntityState.Modified;

        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (DBConcurrencyException)
        //        {
        //            if (!StockItemExists(ItemID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }

        //        return StatusCode(HttpStatusCode.NoContent);
        //    }

        //        [ResponseType(typeof(void))]

        //        public IHttpActionResult PostStockItem([FromBody] StockItem stockitem)
        //        {
        //            {

        //                if (!ModelState.IsValid)
        //                {
        //                    return BadRequest(ModelState);

        //                }

        //                db.StockItems.Add(stockitem);

        //                try
        //                {
        //                    db.SaveChanges();
        //                }
        //                catch (DbUpdateException)
        //                {
        //                    if (StockItemExists(stockitem.ItemID))
        //                    {
        //                        return Conflict();

        //                    }
        //                    else
        //                    {
        //                        throw;
        //                    }
        //                }

        //                return CreatedAtRoute("DefaultAPI", new { ItemID = stockitem.ItemID }, stockitem);
        //            }
        //        }

        //        [ResponseType(typeof(StockItem))]
        //        public IHttpActionResult StockItem(int ItemID)
        //        {
        //            {

        //                StockItem stockitem = db.StockItems.Find(ItemID);
        //                if (stockitem == null)
        //                {
        //                    return NotFound();
        //                }

        //                db.StockItems.Remove(stockitem);
        //                db.SaveChanges();

        //                return Ok(stockitem);

        //            }
        //        }



        //    protected override void Dispose(bool disposing)
        //    {
        //        if (disposing)
        //        {
        //            db.Dispose();
        //        }
        //        base.Dispose(disposing);
        //    }

        //    private bool StockItemExists(int ItemID)
        //    {
        //        return db.StockItems.Count(e => e.ItemID == ItemID) > 0;
        //    }

        //}
    }
}