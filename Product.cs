using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace HomeDepotAPI
{
    public class Product
    {
        public string StoreSkuId { get; protected set; }
        public string BrandName { get; protected set; }
        public string ProductName { get; protected set; }
        public int AisleNumber { get; protected set; }
        public string BayNumber { get; protected set; }
        private int StoreNumber { get; set; }
        public string OnlineNumber { get; protected set; }
        public string LargestImageURL { get; protected set; }

        public Product(string skuid, string onlineNum, int storeNumber)
        {
            StoreNumber = storeNumber;
            StoreSkuId = skuid;
            OnlineNumber = onlineNum;
            SetInfo();
        }

        private void SetInfo()
        {
            SetNameInfo();
            SetLocationInfo();
        }

        private void SetNameInfo()
        {
            string url = "https://www.homedepot.com/p/svcs/frontEndModel/" + OnlineNumber;

            var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.Method = "GET";

            http.Accept = "*/*";
            // http.Connection = "keep-alive";

            string response = new StreamReader(http.GetResponse().GetResponseStream()).ReadToEnd();

            JObject primaryItemData = JObject.Parse(JObject.Parse(response)["primaryItemData"].ToString());
            JObject info = JObject.Parse(primaryItemData["info"].ToString());
            JArray mediaInfo = JArray.Parse(primaryItemData["media"]["mediaList"].ToString());

            foreach (JObject j in mediaInfo)
            {
                //search for largest image
                if (j["mediaType"].ToString() == "IMAGE")
                    LargestImageURL = j["location"].ToString();
            }

            try
            {
                BrandName = info["brandName"].ToString();
            }
            catch (NullReferenceException)
            {
                BrandName = null;
            }

            ProductName = info["productLabel"].ToString();
        }

        private void SetLocationInfo()
        {
            string url = "https://www.homedepot.com/edge-service/aislebay?storeSkuid=" + StoreSkuId + "&storeid=" + StoreNumber + "&type=json";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.Method = "GET";
            try
            {
                string response = new StreamReader(http.GetResponse().GetResponseStream()).ReadToEnd();

                JArray a = JArray.Parse(JObject.Parse(response)["storeSkus"].ToString());
                JObject aisleInfo = JObject.Parse(a[0].ToString());

                AisleNumber = int.Parse(aisleInfo["aisleBayInfo"]["aisle"].ToString());
                BayNumber = aisleInfo["aisleBayInfo"]["bay"].ToString();
            }
            catch (WebException e)
            {
                throw new Exception("Generic Product");
            }
        }

    }
}
