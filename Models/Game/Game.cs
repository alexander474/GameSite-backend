using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExamBackend.Models.Game {

    public class Game {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string GameId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [BsonElement("name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }    
        [BsonElement("description")]
        [BsonRepresentation(BsonType.String)]
        public string Description { get; set; }
        [BsonElement("category")]
        [BsonRepresentation(BsonType.String)]
        public string Category { get; set; }

        [BsonElement("characters")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> CharacterIds { get; set; } = new List<string>();
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
        [BsonElement("Images")]
        [BsonRepresentation(BsonType.String)]
        public List<string> Images { get; set; } = new List<string>();
        
    }
}