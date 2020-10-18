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

        //[Route("api/Reports/GetProductReportData")]
        //[HttpPost]
        //public dynamic GetProductReportData([FromBody] Criteria Criteria)
        //{

        //    db.Configuration.ProxyCreationEnabled = false;
        //    try
        //    {
        //        DateTime StartDate = Criteria.StartDate.AddDays(1);
        //        DateTime EndDate = Criteria.EndDate.AddDays(1);
        //        List<SaleLine> getSales = db.SaleLines.Include(zz => zz.Product).Where(zz => zz.Sale.Date >= StartDate && zz.Sale.Date <= EndDate).ToList();



        //        return getReport(getSales, Criteria);
        //    }
        //    catch (Exception err)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sale report details are invalid");
        //    }
        //}


        private dynamic getReport(List<SaleLine> ReportList, dynamic Criteria)
        {

            dynamic Output = new ExpandoObject();
            var categoryList = ReportList.GroupBy(zz => zz.ProductID);
            List<dynamic> catList = new List<dynamic>();
            foreach (var count in categoryList)
            {
                string findProd = db.Products.Where(zz => zz.ProductID == count.Key).Select(zz => zz.Name).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findProd;
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
                var Total = 0;
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
        public dynamic GetFinancialReportData([FromBody] Criteria Criteria, string SessionID)
        {

            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is no longer valid";
            }
            try
            {
                DateTime StartDate = Criteria.StartDate.AddDays(1);
                DateTime EndDate = Criteria.EndDate.AddDays(1);
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
        public dynamic GetBookingReportData([FromBody] Criteria Criteria, string SessionID)
        {

            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is no longer valid";
            }
            try
            {
                DateTime StartDate = Criteria.StartDate.AddDays(1);
                DateTime EndDate = Criteria.EndDate.AddDays(1);

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
        public dynamic GetSaleReportData([FromBody] Criteria Criteria, string SessionID)
        {
            
            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is no longer valid";
            }

            try
            {
                DateTime StartDate = Criteria.StartDate.AddDays(1);
                DateTime EndDate = Criteria.EndDate.AddDays(1);
                List<Sale> getSales = db.Sales.Include(zz => zz.SaleLines).Where(zz => zz.Date >= StartDate && zz.Date <= EndDate).ToList();

                int Option = Criteria.Option;

                switch(Option)
                {
                    case 1:
                        List<SaleLine> getSaleLines = db.SaleLines.Include(zz => zz.Product).Where(zz => zz.Sale.Date >= StartDate && zz.Sale.Date <= EndDate).ToList();
                        return getReport(getSaleLines, Option);
                    case 2:
                        List<Booking> getBookingSale = db.Bookings.Include(zz => zz.BookingLines).Include(zz => zz.Sale).Where(zz => zz.SaleID != null).Where(zz => zz.Sale.Date >= StartDate && zz.Sale.Date <= EndDate).ToList();
                        return getBookingSaleReport(getBookingSale);
                    case 3:
                        List<ClientPackage> getPackageSales = db.ClientPackages.Where(zz => zz.Sale.Date >= StartDate && zz.Sale.Date <= EndDate).ToList();
                        return getPackageSalesReport(getPackageSales);
                    case 4:
                        return getAllSalesReport(getSales, Option);
                    default:
                        return getAllSalesReport(getSales, Option);
                }

               
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sale report details are invalid");
            }

        }

        private dynamic getPackageSalesReport(List<ClientPackage> ReportList)
        {
            dynamic Output = new ExpandoObject();
            var categoryList = ReportList.GroupBy(zz => zz.PackageID);
            List<dynamic> catList = new List<dynamic>();
            foreach (var count in categoryList)
            {
                dynamic object1 = new ExpandoObject();

                int ServiceID = db.ServicePackages.Where(zz => zz.PackageID == count.Key).Select(zz => zz.ServiceID).FirstOrDefault();
                object1.Name = db.Services.Where(zz => zz.ServiceID == ServiceID).Select(zz => zz.Name).FirstOrDefault();
                object1.NumActivated = count.Count();
                object1.Total = count.Sum(zz => zz.Sale.Payment);
                catList.Add(object1);
            }
            Output.Category = catList;
            return Output;
        }

        private dynamic getBookingSaleReport(List<Booking> ReportList) 
        {
            dynamic Output = new ExpandoObject();
            
            List<dynamic> bookList = new List<dynamic>();
            foreach(var items in ReportList)
            {
                var group = items.BookingLines.GroupBy(zz => zz.ServiceID);
                foreach (var count in group)
                {
                    int j = Convert.ToInt32(count.Key);
                    string ServiceName = db.Services.Where(zz => zz.ServiceID == j).Select(zz => zz.Name).FirstOrDefault();
                    dynamic object1 = new ExpandoObject();
                    object1.Service = ServiceName;
                    object1.NumBookings = count.Count();
                    object1.Total = count.Sum(zz => zz.Booking.Sale.Payment);
                    bookList.Add(object1);
                }

            }
            Output.Category = bookList;
            return Output;
        }

        private dynamic getAllSalesReport(List<Sale> ReportList, int Criteria)
        {

            dynamic Output = new ExpandoObject();
            var categoryList = ReportList.GroupBy(zz => zz.SaleTypeID);
            List<dynamic> catList = new List<dynamic>();
            foreach (var count in categoryList)
            {
                string findCat = db.SaleTypes.Where(zz => zz.SaleTypeID == count.Key).Select(zz => zz.Type).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findCat;
                object1.NumSold = count.Count();
                object1.Total = count.Sum(zz => zz.Payment);
                catList.Add(object1);
            }
            Output.Category = catList;
            return Output;
        }

        [Route("api/Reports/GetSupplierData")]
        [HttpPost]
        public dynamic GetSupplierData([FromBody] Criteria Criteria, string SessionID)
        {

            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is no longer valid";
            }
            try
            {
                DateTime StartDate = Criteria.StartDate.AddDays(1);
                DateTime EndDate = Criteria.EndDate.AddDays(1);
                List<StockItemLine> getSuppliers = db.StockItemLines.Where(zz => zz.SupplierOrder.Date >= StartDate && zz.SupplierOrder.Date <= EndDate && zz.Received == true)
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

        [Route("api/Reports/GetAllReports")]
        [HttpPost]
        public dynamic GetAllReports([FromBody] Criteria Criteria, string SessionID)
        {
            User findUser = UserController.CheckUser(SessionID);
            if (findUser == null)
            {
                return UserController.SessionError();
            }

            DateTime StartDate = Criteria.StartDate.AddDays(1).Date;
            DateTime EndDate = Criteria.EndDate.AddDays(2).Date;
            List<Booking> getBookings = db.Bookings.Include(zz => zz.EmployeeSchedules).Include(zz => zz.DateRequesteds)
                .Include(zz => zz.BookingLines).Include(zz => zz.Sale).Where(zz => zz.DateCreated >= StartDate && zz.DateCreated <= EndDate).ToList();

            return formatBookingReport(getBookings, Criteria);
        }

        private dynamic formatBookingReport(List<Booking> Modell, Criteria dates)
        {
            dynamic Output = new ExpandoObject();

            var bookings = Modell.GroupBy(zz => zz.StatusID);
            List<dynamic> statuses = new List<dynamic>();
            foreach(var count in bookings)
            {
                dynamic object1 = new ExpandoObject();
                BookingStatu findStatus = db.BookingStatus.Find(count.Key);
                object1.Name = findStatus.Status;
                object1.Count = count.Count();
                statuses.Add(object1);
            }
            Output.Statuses = statuses;


            List<dynamic> services = new List<dynamic>();
            foreach (var items in Modell)
            {
                var group = items.BookingLines.GroupBy(zz => zz.ServiceID);
                foreach (var count in group)
                {
                    int j = Convert.ToInt32(count.Key);
                    string ServiceName = db.Services.Where(zz => zz.ServiceID == j).Select(zz => zz.Name).FirstOrDefault();
                    dynamic object1 = new ExpandoObject();
                    object1.Service = ServiceName;
                    object1.NumBookings = count.Count();
                    services.Add(object1);
                }

            }
            Output.ServicesBooked = services;


            List<dynamic> packages = new List<dynamic>();
            List<PackageInstance> findIntance = db.PackageInstances.Where(zz => zz.Date >= dates.StartDate && zz.Date <= dates.EndDate && zz.LineID != null && zz.StatusID == 2).ToList();
            var groupList = findIntance.GroupBy(zz => zz.PackageID);

            foreach(var liss in groupList)
            {
                ServicePackage findPackae = db.ServicePackages.Where(zz => zz.PackageID == liss.Key).FirstOrDefault();
                dynamic object1 = new ExpandoObject();
                object1.Name = findPackae.Service.Name;
                object1.Total = liss.Count();

                packages.Add(object1);
            }
            Output.ServicePackages = packages;

            return Output;

        }

    }

   

    public class Criteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Option { get; set; }
    }
}

