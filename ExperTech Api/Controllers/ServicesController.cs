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
using Microsoft.Ajax.Utilities;
using System.Runtime.Serialization;

namespace ExperTech_Api.Controllers
{
    public class ServicesController : ApiController
    {
        private ExperTechEntities db = new ExperTechEntities();


        //************************************Service Type************************************************
        [Route("api/Services/AddServiceType")]
        [HttpPost]
        public dynamic AddServiceType([FromBody] ServiceType Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                ServiceType Verify = db.ServiceTypes.Where(zz => zz.Name == Modell.Name).FirstOrDefault();
                if (Verify == null)
                {
                    db.ServiceTypes.Add(Modell);
                    db.SaveChanges();
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

        [Route("api/Services/GetServiceType")]
        [HttpGet]
        public List<dynamic> GetServiceType()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServiceType> myList = db.ServiceTypes.ToList();
            return ListServiceTypes(myList);
        }

        private List<dynamic> ListServiceTypes(List<ServiceType> Modell)
        {
            List<dynamic> makeList = new List<dynamic>();
            foreach (ServiceType Item in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.TypeID = Item.TypeID;
                newObject.Name = Item.Name;
                newObject.Description = Item.Description;
                makeList.Add(newObject);
            }
            return makeList;
        }

        [Route("api/Services/UpdateServiceType")]
        [HttpPut]
        public dynamic UpdateServiceType(ServiceType Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            ServiceType Update = db.ServiceTypes.Where(zz => zz.TypeID == Modell.TypeID).FirstOrDefault();
            if (Update != null)
            {

                Update.Name = Modell.Name;
                Update.Description = Modell.Description;
                db.SaveChanges();
                return "success";
            }
            else
            {
                return "failed";
            }
        }

        [Route("api/Services/DeleteServiceType")]
        [HttpDelete]
        public dynamic DeleteServiceType(int TypeID)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                ServiceType find = db.ServiceTypes.Where(zz => zz.TypeID == TypeID).FirstOrDefault();
                db.ServiceTypes.Remove(find);
                db.SaveChanges();
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        //************************************Service************************************************
        [Route("api/Services/AddService")]
        [HttpPost]
        public dynamic AddService([FromBody] Service Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            try
            {   //checks for dupicate service

                Service Verify = db.Services.Where(zz => zz.Name == Modell.Name).FirstOrDefault();
                if (Verify == null)
                {
                    //if not duplicate, execute SaveService()
                    //Service ServiceObject = FormatServices(Modell);
                    return SaveService(Modell);
                }
                else
                {
                    //if duplicate, return duplicate
                    return "duplicate";
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        private Service FormatServices(dynamic Modell)
        {
            Service newService = new Service();
            newService.TypeID = (int)Modell.TypeID;
            newService.Name = Modell.Name;
            newService.Description = Modell.Description;
            newService.Duration = Modell.Duration;

            //newService.TypeID = Modell
            return newService;
        }

        private dynamic SaveService(Service Modell)
        {
            try
            {

                db.Configuration.ProxyCreationEnabled = false;
                //first Save the service information
                Service myObject = new Service();
                myObject.Name = Modell.Name;
                myObject.Description = Modell.Description;
                myObject.Duration = Modell.Duration;
                myObject.TypeID = Modell.TypeID;
                db.Services.Add(myObject);
                db.SaveChanges();

                //retrieve the service ID from the info that were just saved
                int ServiceID = db.Services.Where(zz => zz.Name == Modell.Name).Select(zz => zz.ServiceID).FirstOrDefault();

                foreach (ServicePrice Items in Modell.ServicePrices)
                {
                    //Save the Service price object
                    ServicePrice PriceObject = new ServicePrice();
                    PriceObject.ServiceID = ServiceID;
                    //PriceObject.OptionID = Items.OptionID;
                    PriceObject.Price = Items.Price;
                    PriceObject.Date = DateTime.Now;
                    db.ServicePrices.Add(PriceObject);

                    db.SaveChanges();
                }

                foreach (ServiceTypeOption Items in Modell.ServiceTypeOptions)
                {
                    //saves the OptionID and ServiceID into bride entity
                    ServiceTypeOption newObject = new ServiceTypeOption();
                    newObject.ServiceID = ServiceID;
                    int OptionID = (int)Items.OptionID;
                    newObject.OptionID = OptionID;
                    db.ServiceTypeOptions.Add(newObject);
                    db.SaveChanges();

                    foreach (ServicePrice PriceItem in Items.ServicePrices)
                    {
                        ServicePrice PriceObject = new ServicePrice();
                        PriceObject.ServiceID = ServiceID;
                        PriceObject.OptionID = PriceItem.OptionID;
                        PriceObject.Price = PriceItem.Price;
                        PriceObject.Date = DateTime.Now;
                        db.ServicePrices.Add(PriceObject);

                        db.SaveChanges();
                    }

                }

                //check if service object has photos
                if (Modell.ServicePhotoes != null)
                {
                    foreach (ServicePhoto Items in Modell.ServicePhotoes)
                    {
                        //then save the photos
                        ServicePhoto AddPhoto = new ServicePhoto();
                        AddPhoto.Photo = Items.Photo;
                        AddPhoto.ServiceID = ServiceID;
                        db.ServicePhotoes.Add(AddPhoto);
                        db.SaveChanges();
                    }


                }

                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        [Route("api/Services/DeleteService")]
        [HttpDelete]
        public dynamic DeleteService(int ServiceID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                Service find = db.Services.Where(zz => zz.ServiceID == ServiceID).FirstOrDefault();
                List<ServicePrice> findPrices = db.ServicePrices.Where(zz => zz.ServiceID == ServiceID).ToList();
                List<ServicePhoto> findPhotos = db.ServicePhotoes.Where(zz => zz.ServiceID == ServiceID).ToList();
                List<ServiceTypeOption> findTypeOption = db.ServiceTypeOptions.Where(zz => zz.ServiceID == ServiceID).ToList();

                if (findPrices != null)
                {
                    db.ServicePrices.RemoveRange(findPrices);
                    db.SaveChanges();
                }

                if (findPhotos != null)
                {
                    db.ServicePhotoes.RemoveRange(findPhotos);
                    db.SaveChanges();
                }

                if (findTypeOption != null)
                {
                    db.ServiceTypeOptions.RemoveRange(findTypeOption);
                    db.SaveChanges();
                }

                db.Services.Remove(find);
                db.SaveChanges();
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }


        [Route("api/Services/GetService")]
        [HttpGet]
        public List<dynamic> GetService()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Service> myList = db.Services.Include(zz => zz.ServicePrices).Include(zz => zz.ServiceType)
                                    .Include(zz => zz.ServicePhotoes).Include(zz => zz.ServiceTypeOptions).ToList();
            return getServices(myList);
        }

        private List<dynamic> getServices(List<Service> Modell)
        {
            List<dynamic> ServiceList = new List<dynamic>();
            foreach (Service Items in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.ServiceID = Items.ServiceID;
                newObject.Name = Items.Name;
                newObject.ServiceType = Items.ServiceType.Name;
                newObject.TypeID = Items.TypeID;
                newObject.Description = Items.Description;
                newObject.Duration = Items.Duration;
                if (Items.ServiceTypeOptions.Count > 0)
                    newObject.ServiceTypeOptions = getOptions(Items);
                else
                    newObject.ServicePrices = getSPrice(Items);

                ServiceList.Add(newObject);
            }
            return ServiceList;
        }


        private dynamic getOptions(Service Modell)
        {
            List<dynamic> myList = new List<dynamic>();
            foreach (ServiceTypeOption Items in Modell.ServiceTypeOptions)
            {
                dynamic newObject = new ExpandoObject();
                ServiceOption findOption = db.ServiceOptions.Where(zz => zz.OptionID == Items.OptionID).FirstOrDefault();
                newObject.Option = findOption.Name;
                newObject.OptionID = Items.OptionID;
                List<dynamic> ServicePrices = new List<dynamic>();
                foreach (ServicePrice PriceItem in Items.ServicePrices)
                {
                    dynamic PriceObject = new ExpandoObject();
                    PriceObject.Price = PriceItem.Price;

                    ServicePrices.Add(PriceObject);
                }
                newObject.ServicePrices = ServicePrices;
                myList.Add(newObject);
            }

            return myList;


        }

        private dynamic getSPrice(Service Modell)
        {
            List<dynamic> myList = new List<dynamic>();
            foreach (ServicePrice Items in Modell.ServicePrices)
            {
                dynamic newObject = new ExpandoObject();
                newObject.Price = Items.Price;
                if (Items.OptionID != null)
                {
                    newObject.OptionID = Items.OptionID;
                }
                myList.Add(newObject);
            }

            return myList;


        }

        [Route("api/Services/UpdateService")]
        [HttpPost]
        public dynamic UpdateService([FromBody] Service Modell)
        {
            try
            {
                Service findService = db.Services.Where(zz => zz.ServiceID == Modell.ServiceID).FirstOrDefault();
                findService.Name = Modell.Name;
                findService.Description = Modell.Description;
                findService.Duration = Modell.Duration;
                findService.TypeID = Modell.TypeID;
                db.SaveChanges();

                foreach (ServicePrice Items in Modell.ServicePrices)
                {
                    ServicePrice findPrice = db.ServicePrices.Where(zz => zz.PriceID == Items.PriceID && zz.Price == Items.Price).FirstOrDefault();
                    if (findPrice == null)
                    {
                        ServicePrice PriceObject = new ServicePrice();
                        PriceObject.ServiceID = findService.ServiceID;
                        PriceObject.OptionID = Items.OptionID;
                        PriceObject.Price = Items.Price;
                        PriceObject.Date = DateTime.Now;
                        db.ServicePrices.Add(PriceObject);
                        db.SaveChanges();
                    }
                }
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        //****************************************Service Option**************************************

        [Route("api/Services/AddServiceOption")]
        [HttpPost]
        public dynamic AddServiceOption(ServiceOption Modell)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.ServiceOptions.Add(Modell);
                db.SaveChanges();
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }

        }



        [Route("api/Services/GetServiceOption")]
        [HttpGet]
        public List<ServiceOption> GetServiceOption()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServiceOption> mylist = db.ServiceOptions.ToList();
            return mylist;
        }

        [Route("api/Services/UpdateServiceOption")]
        [HttpPut]
        public dynamic UpdateServiceOption(ServiceOption Modell)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                ServiceOption findOption = db.ServiceOptions.Where(zz => zz.OptionID == Modell.OptionID).FirstOrDefault();
                findOption.Name = Modell.Name;
                findOption.Duration = Modell.Duration;
                db.SaveChanges();
                return "success";


            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        [Route("api/Services/DeleteServiceOption")]
        [HttpDelete]
        public dynamic DeleteServiceOption(ServiceOption Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            ServiceOption findOption = db.ServiceOptions.Where(zz => zz.OptionID == Modell.OptionID).FirstOrDefault();
            db.ServiceOptions.Remove(findOption);
            db.SaveChanges();
            return "success";
        }

        //******************************Service Package************************************
        [Route("api/Services/CreateServicePackage")]
        [HttpPost]
        public dynamic CreateServicePackage(ServicePackage Modell)
        {
            try
            {
                ServicePackage findPackage = db.ServicePackages.Where(zz => zz.Name == Modell.Name).FirstOrDefault();

                if (findPackage == null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.ServicePackages.Add(Modell);
                    db.SaveChanges();
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

        [Route("api/Services/RetrieveServicePackage")]
        [HttpGet]
        public dynamic RetrieveServicePackage()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServicePackage> mylist = db.ServicePackages.Include(zz => zz.Service).ToList();
            return getPackage(mylist);
        }

        private dynamic getPackage(List<ServicePackage> Modell)
        {
            List<dynamic> thisList = new List<dynamic>();
            foreach (ServicePackage items in Modell)
            {
                dynamic myObject = new ExpandoObject();
                myObject.Service = items.Service.Name;
                myObject.Description = items.Name;
                myObject.Price = items.Price;
                myObject.Quantity = items.Quantity;

                thisList.Add(myObject);
            }

            return thisList;
        }

        [Route("api/Services/RemoveServicePackage")]
        [HttpDelete]
        public dynamic RemoveServicePackage(int PackageID)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                ServicePackage findPackage = db.ServicePackages.Where(zz => zz.PackageID == PackageID).FirstOrDefault();
                db.ServicePackages.Remove(findPackage);
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        //*********************************Refiloe's stuff****************************

        [Route("api/Services/DisplaySchedule")]
        [HttpGet]
        public dynamic DisplaySchedule()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Schedule> findSchedule = db.Schedules.Include(zz => zz.Date).Include(zz => zz.Timeslot).ToList();
            return GetSchedule(findSchedule);
        }

        private dynamic GetSchedule(List<Schedule> Modell)
        {
            List<Date> Dates = db.Dates.ToList();
            List<dynamic> getList = new List<dynamic>();
            dynamic result = new ExpandoObject();

            for (int j = 0; j < Dates.Count; j++)
            {
                dynamic newObject = new ExpandoObject();
                newObject.DateID = Dates[j].DateID;
                newObject.Dates = Dates[j].Date1;
                List<dynamic> getTimes = new List<dynamic>();

                foreach (Schedule Items in Modell)
                {
                    if (Items.DateID == Dates[j].DateID)
                    {
                        dynamic TimeObject = new ExpandoObject();
                        TimeObject.TimeID = Items.TimeID;
                        TimeObject.StartTime = Items.Timeslot.StartTime;
                        TimeObject.EndTime = Items.Timeslot.EndTime;
                        getTimes.Add(TimeObject);
                    }
                }
                newObject.Times = getTimes;
                getList.Add(newObject);
            }

            return getList;
        }

        [Route("api/Services/getBookings")]
        [HttpGet]
        public dynamic getBookings()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> findBooking = db.Bookings.Include(zz => zz.EmployeeSchedules).Include(zz => zz.DateRequesteds)
                .Include(zz => zz.Client).Include(zz => zz.BookingLines).Include(zz => zz.BookingNotes).ToList();
            return viewBooking(findBooking);
        }

        private dynamic viewBooking(List<Booking> findBooking)
        {
            List<dynamic> BookingList = new List<dynamic>();
            foreach (Booking items in findBooking)
            {
                dynamic BookingObject = new ExpandoObject();
                BookingObject.BookingID = items.BookingID;
                BookingObject.BookingStatusID = items.StatusID;
                BookingObject.BookingStatus = db.BookingStatus.Where(zz => zz.StatusID == items.StatusID).Select(zz => zz.Status).FirstOrDefault(); ;
                BookingObject.Client = items.Client.Name;

                foreach (DateRequested requests in items.DateRequesteds)
                {
                    dynamic requestObject = new ExpandoObject();
                    requestObject.Dates = requests.Date;
                    requestObject.Time = requests.StartTime;
                    BookingObject.BookingRequest = requestObject;
                }

                List<dynamic> getSchedule = new List<dynamic>();
                foreach (EmployeeSchedule booking in items.EmployeeSchedules)
                {
                    dynamic scheduleObject = new ExpandoObject();  //can you change employee after confirmed booking && where does the advise get saved
                    scheduleObject.Employee = db.Employees.Where(zz => zz.EmployeeID == booking.EmployeeID).Select(zz => zz.Name).FirstOrDefault();
                    scheduleObject.Dates = db.Dates.Where(zz => zz.DateID == booking.DateID).Select(zz => zz.Date1).FirstOrDefault();
                    scheduleObject.StartTime = db.Timeslots.Where(zz => zz.TimeID == booking.TimeID).Select(zz => zz.StartTime).FirstOrDefault();
                    scheduleObject.EndTime = db.Timeslots.Where(zz => zz.TimeID == booking.TimeID).Select(zz => zz.EndTime).FirstOrDefault();
                    scheduleObject.Status = db.ScheduleStatus.Where(zz => zz.StatusID == booking.StatusID).Select(zz => zz.Status).FirstOrDefault(); ;

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
                BookingObject.BookingLine = getLines;


                BookingList.Add(BookingObject);
            }

            return BookingList;
        }

    }
}
