using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExperTech_Api.Models;

namespace ExperTech_Api.Controllers
{
    public class CompanyInfoController : ApiController
    {
        [RoutePrefix("Api/CompanyInfo")]
        public class CRUDController : ApiController
        {
            ExperTechEntities AccessOBJ = new ExperTechEntities();
            [HttpGet]
            [Route("GetCompanyInfos")]
            public IQueryable<CompanyInfo> GetCompanyInfos()
            {
                try
                {
                    return AccessOBJ.CompanyInfoes;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            [HttpPost]
            [Route("AddCompany")]
            public IHttpActionResult AddCompany(CompanyInfo companyInfo)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                try
                {
                    AccessOBJ.CompanyInfoes.Add(companyInfo);
                    AccessOBJ.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }
                return Ok(companyInfo);
            }

            [HttpPut]
            [Route("EditCompany")]
            public IHttpActionResult EditCompany(CompanyInfo company)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                try
                {
                    CompanyInfo ObjCompany = new CompanyInfo();
                    ObjCompany = AccessOBJ.CompanyInfoes.Find(company.InfoID);
                    if (ObjCompany != null)
                    {
                        ObjCompany.Name = company.Name;
                        ObjCompany.Address = company.Address;
                        ObjCompany.ContactNo = company.ContactNo;
                    }
                    this.AccessOBJ.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }
                return Ok(company);
            }

            [HttpDelete]

            [Route("DeleteCompany")]
            public IHttpActionResult DeleteCompany(int id)
            {
                CompanyInfo info = AccessOBJ.CompanyInfoes.Find(id);
                if (info == null)
                {
                    return NotFound();
                }
                AccessOBJ.CompanyInfoes.Remove(info);
                AccessOBJ.SaveChanges();
                return Ok(info);
            }

        }
    }
}
