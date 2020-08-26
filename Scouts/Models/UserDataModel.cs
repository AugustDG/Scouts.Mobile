using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Scouts.Models.Enums;
using Xamarin.Forms;

namespace Scouts.Models
{
    [BsonIgnoreExtraElements]
    public class UserDataModel
    {
        [JsonIgnore] public ObjectId id = ObjectId.GenerateNewId();

        public string UserId;

        public string Username { get; set; }
        public string Password;

        public UserType UserType;
        public string AccessCodeUsed;
        public DateTime CreatedOn;

        public double[] RGB = new[] {Random.NextDouble(), Random.NextDouble(), Random.NextDouble()};
        
        [BsonIgnore] private static Random Random = new Random();
        
        [BsonIgnore] public Color Color
        {
            get => new Color(RGB[0], RGB[1], RGB[2]);
            set
            {
                RGB = new[] {value.R, value.G, value.B};
            }
        }
    }
}