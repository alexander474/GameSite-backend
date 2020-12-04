using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExamBackend.Models.Character {
    public class Character {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CharacterId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [BsonElement("name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }    
        [BsonElement("category")]
        [BsonRepresentation(BsonType.String)]
        public string Description { get; set; }   
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> GameIds { get; set; } = new List<string>();
        [BsonElement("Images")]
        [BsonRepresentation(BsonType.String)]
        public List<string> Images { get; set; } = new List<string>();
    }
}