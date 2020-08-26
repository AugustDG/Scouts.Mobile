using MongoDB.Bson;
using Scouts.Models.Enums;

namespace Scouts.Models
{
    public class AccessModel
    {
        public ObjectId id = new ObjectId();

        public string AccessCode;
        public UserType UserType;
    }
}