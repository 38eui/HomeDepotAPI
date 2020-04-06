using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace HomeDepotAPI
{
    public class ProductSearcher
    {
        private int StoreNumber { get; set; }
        public ProductSearcher(int storeNumber)
        {
            StoreNumber = storeNumber;
        }

        public List<Product> Search(string searchTerm)
        {
            List<Product> result = new List<Product>();

            string url = "https://www.homedepot.com/dynamicrecs/searchViewed?storeid=" + StoreNumber + "&anchor=" + searchTerm;

            var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.Method = "GET";

            string response = new StreamReader(http.GetResponse().GetResponseStream()).ReadToEnd();

            JArray a = JArray.Parse(JObject.Parse(response)["products"].ToString());

            for (int i = 0; i < ((a.Count < 5) ? a.Count : 5); i++)
            {
                try
                {
                    JObject aisleInfo = JObject.Parse(a[i].ToString());
                    result.Add(new Product(aisleInfo["storeSkuNumber"].ToString(), aisleInfo["productId"].ToString(), StoreNumber));
                }
                catch (Exception)
                {

                }
            }

            if (result.Count == 0)
                throw new Exception("No Products Found");
            return result;
        }

    }
}
