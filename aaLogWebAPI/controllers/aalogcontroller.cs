//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using System.Web.Http;
//using System.Net.Http;
//using System.Net;
//using aaLogWebAPI.Models;
//using aaLogReader;
//using Newtonsoft.Json;

//namespace aaLogWebAPI.Controllers
//{    
//    [RoutePrefix("api/aalog/v1")]
//    public class aaLogV1Controller:ApiController
//    {

//        //[Route("api/aalog/v1/unread")]
//        //[HttpGet]
//        //public string Get()
//        //{
//        //    aaLogReader.aaLogReader lr = new aaLogReader.aaLogReader();

//        //    List<LogRecord> lrecords = lr.GetRecordsInternal(10);

//        //    return JsonConvert.SerializeObject(lrecords);
//        //}

//        [Route("unread")]
//        [HttpGet]
//        public string GetUnread(ulong count=100,string stopmessagepattern = "", bool ignorecachefile = false, ulong startmessagenumber = ulong.MaxValue)
//        {
//            aaLogReader.aaLogReader lr = new aaLogReader.aaLogReader();
//            List<LogRecord> lrecords = lr.GetRecordsInternal(count, stopmessagepattern, ignorecachefile, startmessagenumber);
//            return JsonConvert.SerializeObject(lrecords);
//        }

//        [Route("unread")]
//        [HttpPost]
//        public string GetUnread([FromBody]aaLogReader.aaLogReaderOptionsStruct options,ulong count = 100, string stopmessagepattern = "", bool ignorecachefile = false, ulong startmessagenumber = ulong.MaxValue)
//        {
//            aaLogReader.aaLogReader lr = new aaLogReader.aaLogReader(options);
//            List<LogRecord> lrecords = lr.GetRecordsInternal(count, stopmessagepattern, ignorecachefile, startmessagenumber);
//            return JsonConvert.SerializeObject(lrecords);
//        }

//        //[Route("unread")]
//        //[HttpPost]
//        //public HttpResponseMessage GetUnread([FromBody]aaLogReader.aaLogReaderOptionsStruct options)
//        //{

//        //    Console.WriteLine(Request.ToString());

//        //    aaLogReader.aaLogReader lr = new aaLogReader.aaLogReader();
//        //    //List<LogRecord> lrecords = lr.GetRecordsInternal(count, "stopmessagepattern", ignorecachefile);
//        //    //return JsonConvert.SerializeObject(lrecords);
//        //    return new HttpResponseMessage(HttpStatusCode.OK);

//        //}

//        //public Company Get(int id)
//        //{


//        //    var company = _Db.FirstOrDefault(c => c.Id == id);
//        //    if (company == null)
//        //    {
//        //        throw new HttpResponseException(
//        //            System.Net.HttpStatusCode.NotFound);
//        //    }
//        //    return company;
//        //}


//        //public IHttpActionResult Post(Company company)
//        //{
//        //    if (company == null)
//        //    {
//        //        return BadRequest("Argument Null");
//        //    }
//        //    var companyExists = _Db.Any(c => c.Id == company.Id);

//        //    if (companyExists)
//        //    {
//        //        return BadRequest("Exists");
//        //    }

//        //    _Db.Add(company);
//        //    return Ok();
//        //}


//        //public IHttpActionResult Put(Company company)
//        //{
//        //    if (company == null)
//        //    {
//        //        return BadRequest("Argument Null");
//        //    }
//        //    var existing = _Db.FirstOrDefault(c => c.Id == company.Id);

//        //    if (existing == null)
//        //    {
//        //        return NotFound();
//        //    }

//        //    existing.Name = company.Name;
//        //    return Ok();
//        //}


//        //public IHttpActionResult Delete(int id)
//        //{
//        //    var company = _Db.FirstOrDefault(c => c.Id == id);
//        //    if (company == null)
//        //    {
//        //        return NotFound();
//        //    }
//        //    _Db.Remove(company);
//        //    return Ok();
//        //}
//    }
//}