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
using System.IO.Compression;

namespace SenseServer
{
    public class UploadController : ApiController
    {
        private MongoClient mongo = new MongoClient();

        public static int counter = 0;

        public async Task<HttpResponseMessage> PostFormData()
        {
            Console.WriteLine("UPLOAD");

            var provider = new MultipartFileStreamProvider("C:\\test");
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith(x =>
                {
                    var zipName = string.Format("db{0}.zip", counter);
                    var file = string.Format("C:\\test\\{0}", zipName);
                    counter++;

                    if (File.Exists(file))
                        File.Delete(file);

                    Console.WriteLine("Parts: {0}", provider.FileData.Count);

                    foreach (var filePart in provider.FileData)
                    {
                        File.Copy(filePart.LocalFileName, file);
                        File.Delete(filePart.LocalFileName);
                    }

                    var db = mongo.GetDatabase("sense");

                    using (ZipArchive archive = ZipFile.OpenRead(file))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string[] tokens = entry.Name.Split('_');

                            string deviceId = tokens[0];
                            string type = tokens[1];

                            var collection = db.GetCollection<BsonDocument>(type);

                            using (Stream stream = entry.Open())
                            {
                                var reader = new StreamReader(stream);

                                while (!reader.EndOfStream)
                                    collection.InsertOne(BsonDocument.Parse(reader.ReadLine()));
                            }
                        }
                    }

                    File.Delete(file);

                    return Request.CreateResponse(System.Net.HttpStatusCode.OK);
                });

            var response = await task;

            return response;
        }
    }
}
