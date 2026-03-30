using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB;


using MongoDB.Bson;
using MongoDB.Driver;

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("frases_de_acao");
var collection = database.GetCollection<Perolas>("Perolas_da_call");

public class Perolas
{
    public ObjectId Id { get; set; }
    public string Texto { get; set; }
}