using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;


namespace SenseServer
{
    public class HistoryController : ApiController
    {
        MongoClient mongo = new MongoClient();

        public HttpResponseMessage Get(string userId, string deviceId, string type, long begin, long end)
        {
            var db = mongo.GetDatabase("sense");
            var collection = db.GetCollection<BsonDocument>(type);

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("userid", userId) 
                & builder.Eq("deviceid", deviceId) 
                & builder.Gte("senseStartTimeMillis", begin) 
                & builder.Lte("senseStartTimeMillis", end);

            var result = collection.Find(filter).Project<BsonDocument>(Builders<BsonDocument>.Projection.Exclude("_id"));
            string json = result.ToList().ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });

            var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }
    }
}
