using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExperTech_Api.Models;
using System.Dynamic;
using System.Data.Entity;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExperTech_Api.Controllers
{
    public class ReportsController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Reports/GetProductReportData")]
        [HttpPost]
        public dynamic GetProductReportData([FromBody]Criteria Criteria)
        {
          
            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                DateTime StartDate = Convert.ToDateTime(Criteria.StartDate);
                DateTime EndDate = Convert.ToDateTime(Criteria.EndDate);
                List<SaleLine> getSales = db.SaleLines.Include(zz => zz.Product).Where(zz => zz.Sale.Date >= StartDate && zz.Sale.Date <= EndDate).ToList();



                return getReport(getSales, Criteria);
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sale report details are invalid");
            }

        }

        private dynamic getReport(List<SaleLine> ReportList, dynamic Criteria)
        {

            dynamic Output = new ExpandoObject();
            var categoryList = ReportList.GroupBy(zz => zz.Product.CategoryID);
            List<dynamic> catList = new List<dynamic>();
            foreach (var count in categoryList)
            {
                string findCat = db.ProductCategories.Where(zz => zz.CategoryID == count.Key).Select(zz => zz.Category).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.Total = count.Sum(zz => zz.Quantity);
                catList.Add(object1);
            }
            Output.Category = catList;
            

            var productList = ReportList.GroupBy(zz => new { zz.Product.Name, zz.Product.CategoryID }).GroupBy(zz => zz.Key.CategoryID);
            List<dynamic> proList = new List<dynamic>();
            foreach (var count in productList)
            {
                dynamic object1 = new ExpandoObject();
                object1.Name = db.ProductCategories.Where(zz => zz.CategoryID == count.Key).Select(zz => zz.Category).FirstOrDefault();
                List<dynamic> stockCount = new List<dynamic>();
                foreach (var item in count)
                {
                    dynamic object2 = new ExpandoObject();
                    object2.Name = item.Key.Name;
                    object2.Total = item.Sum(zz => zz.Quantity);
                    object2.Price = item.Sum(zz => zz.Quantity * zz.Product.Price);
                    stockCount.Add(object2);
                }
                object1.StockCount = stockCount;
                proList.Add(object1);
            }
            Output.Product = proList;
            return Output;
        }


        [Route("api/Reports/GetFinancialReportData")]
        [HttpPost]
        public dynamic GetFinancialReportData([FromBody] Criteria Criteria)
        {

            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                DateTime StartDate = Convert.ToDateTime(Criteria.StartDate);
                DateTime EndDate = Convert.ToDateTime(Criteria.EndDate);
                List<Sale> getSales = db.Sales.Where(zz => zz.Date >= StartDate && zz.Date <= EndDate).ToList();
                List<StockItemLine> getStocks = db.StockItemLines.Include(zz => zz.SupplierOrder).Include(zz => zz.StockItem)
                    .Where(zz => zz.SupplierOrder.Date >= StartDate && zz.SupplierOrder.Date <= EndDate).ToList();

                return getPReport(getSales, getStocks);
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Product details are invalid");
            }

        }

        private dynamic getPReport(List<Sale> ReportList, List<StockItemLine> OrderList)
        {

            dynamic Output = new ExpandoObject();
            var incomeList = ReportList.GroupBy(zz => zz.SaleTypeID);
            List<dynamic> InList = new List<dynamic>();
            foreach (var count in incomeList)
            {
                string findCat = db.SaleTypes.Where(zz => zz.SaleTypeID == count.Key).Select(zz => zz.Type).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.Total = count.Sum(zz => zz.Payment);
                InList.Add(object1);
            }
            Output.Income = InList;

            
            var expenseList = OrderList.GroupBy(zz => zz.ItemID);
            List<dynamic> ExList = new List<dynamic>();
            foreach (var count in expenseList)
            {
                string findCat = db.StockItems.Where(zz => zz.ItemID == count.Key).Select(zz => zz.Name).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.Total = count.Sum(zz => zz.Quantity*zz.StockItem.Price);
                ExList.Add(object1);
            }
            Output.Expense = ExList;


            //var profitList = ReportList.GroupBy(zz => zz.SaleTypeID);
            //List<dynamic> proList = new List<dynamic>();
            //foreach (var count in profitList)
            //{
            //    dynamic object1 = new ExpandoObject();
            //    object1.Name = db.Sales.Where(zz => zz.SaleTypeID == count.Key).Select(zz => zz.SaleType).FirstOrDefault();
            //    object1.TotalPrice = count.Sum(zz => zz.Payment);
                
            //    proList.Add(object1);
            //}
            //Output.Product = proList;
            return Output;
        }

        [Route("api/Reports/GetBookingReportData")]
        [HttpPost]
        public dynamic GetBookingReportData([FromBody] Criteria Criteria)
        {

            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                DateTime StartDate = Convert.ToDateTime(Criteria.StartDate);
                DateTime EndDate = Convert.ToDateTime(Criteria.EndDate);
               
                List<EmployeeSchedule> getBookings = db.EmployeeSchedules.Include(zz => zz.Booking).Include(zz => zz.Employee)
                    .Where(zz => zz.Schedule.Date.Date1 >= StartDate && zz.Schedule.Date.Date1 <= EndDate && zz.Booking.StatusID == 6).ToList();

                return getBookingReport(getBookings);
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Criteria is invalid");
            }

        }

        private dynamic getBookingReport(List<EmployeeSchedule> ReportList)
        {

            dynamic Output = new ExpandoObject();
            var bookingList = ReportList.GroupBy(zz => zz.Employee.Name);
            List<dynamic> InList = new List<dynamic>();
            foreach (var count in bookingList)
            {
                dynamic object1 = new ExpandoObject();
                object1.Name = count.Key;
                int Total = 0;
                foreach (var items in bookingList)
                {
                    if(items.Key == count.Key)
                    {
                        Total++;
                    }
                }
                object1.NumBookings = Total;
                
                InList.Add(object1);
            }
            Output.Bookings = InList;

            return Output;
        }

        [Route("api/Reports/GetSaleReportData")]
        [HttpPost]
        public dynamic GetSaleReportData([FromBody] Criteria Criteria)
        {

            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                DateTime StartDate = Convert.ToDateTime(Criteria.StartDate);
                DateTime EndDate = Convert.ToDateTime(Criteria.EndDate);
                List<Sale> getSales = db.Sales.Where(zz => zz.Date >= StartDate && zz.Date <= EndDate).ToList();

                return getReport(getSales);
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sale report details are invalid");
            }

        }

        private dynamic getReport(List<Sale> ReportList)
        { 

            dynamic Output = new ExpandoObject();
            var categoryList = ReportList.GroupBy(zz => zz.SaleTypeID);
            List<dynamic> catList = new List<dynamic>();
            foreach (var count in categoryList)
            {
                string findCat = db.SaleTypes.Where(zz => zz.SaleTypeID == count.Key).Select(zz => zz.Type).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.Total = count.Sum(zz => zz.Payment);
                catList.Add(object1);
            }
            Output.Category = catList;


            return Output;
        }

        [Route("api/Reports/GetSupplierData")]
        [HttpPost]
        public dynamic GetSupplierData([FromBody] Criteria Criteria)
        {

            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                DateTime StartDate = Convert.ToDateTime(Criteria.StartDate);
                DateTime EndDate = Convert.ToDateTime(Criteria.EndDate);
                List<StockItemLine> getSuppliers = db.StockItemLines.Where(zz => zz.SupplierOrder.Date >= StartDate && zz.SupplierOrder.Date <= EndDate)
                    .Include(zz => zz.StockItem).Include(zz => zz.SupplierOrder).ToList();
                return getSuppReport(getSuppliers);
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sale report details are invalid");
            }

        }

        private dynamic getSuppReport(List<StockItemLine> ReportList)
        {

            dynamic Output = new ExpandoObject();
            var categoryList = ReportList.GroupBy(zz => zz.SupplierOrder.SupplierID);
            List<dynamic> supList = new List<dynamic>();
            foreach (var count in categoryList)
            {
                string findCat = db.Suppliers.Where(zz => zz.SupplierID == count.Key).Select(zz => zz.Name).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.NumOrders = count.Sum(zz => zz.Quantity);
                supList.Add(object1);
            }
            Output.Stock = supList;


            var stockList = ReportList.GroupBy(zz => zz.SupplierOrder.SupplierID);
            List<dynamic> stockLiss = new List<dynamic>();
            foreach (var count in stockList)
            {
                string findCat = db.Suppliers.Where(zz => zz.SupplierID == count.Key).Select(zz => zz.Name).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.NumOrders = count.Sum(zz => zz.Quantity);
                object1.Price = count.Sum(zz => zz.SupplierOrder.Price);
                stockLiss.Add(object1);
            }
            Output.Totals = stockLiss;

            return Output;
        }

    }

    public class Criteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

