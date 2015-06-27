//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

 
//// Add Usings:
//using System.Net.Http;
 
//namespace aaLogWebAPIClientTester
//{
//    public class CompanyClient
//    {
//        string _hostUri;
//        public CompanyClient(string hostUri)
//        {
//            _hostUri = hostUri;
//        }
 
 
//        public HttpClient CreateClient()
//        {
//            var client = new HttpClient();
//            client.BaseAddress = new Uri(new Uri(_hostUri), "api/companies/");
//            return client;
//        }
 
 
//        public IEnumerable<Company> GetCompanies()
//        {
//            HttpResponseMessage response;
//            using (var client = CreateClient())
//            {
//                response = client.GetAsync(client.BaseAddress).Result;
//            }
//            var result = response.Content.ReadAsAsync<IEnumerable<Company>>().Result;
//            return result;
//        }
 
 
//        public Company GetCompany(int id)
//        {
//            HttpResponseMessage response;
//            using (var client = CreateClient())
//            {
//                response = client.GetAsync(
//                    new Uri(client.BaseAddress, id.ToString())).Result;
//            }
//            var result = response.Content.ReadAsAsync<Company>().Result;
//            return result;
//        }
 
 
//        public System.Net.HttpStatusCode AddCompany(Company company)
//        {
//            HttpResponseMessage response;
//            using (var client = CreateClient())
//            {
//                response = client.PostAsJsonAsync(client.BaseAddress, company).Result;
//            }
//            return response.StatusCode;
//        }
 
 
//        public System.Net.HttpStatusCode UpdateCompany(Company company)
//        {
//            HttpResponseMessage response;
//            using (var client = CreateClient())
//            {
//                response = client.PutAsJsonAsync(client.BaseAddress, company).Result;
//            }
//            return response.StatusCode;
//        }
 
 
//        public System.Net.HttpStatusCode DeleteCompany(int id)
//        {
//            HttpResponseMessage response;
//            using (var client = CreateClient())
//            {
//                response = client.DeleteAsync(
//                    new Uri(client.BaseAddress, id.ToString())).Result;
//            }
//            return response.StatusCode;
//        }
//    }
//}
