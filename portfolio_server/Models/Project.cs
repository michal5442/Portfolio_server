using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using portfolio_server.Enums;


namespace portfolio_server.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid IdntAgaff { get; set; }

        public string? AgaffName { get; set; } 

        // [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        // public Agaff? Agaff { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid IdntTsevetMevatsea { get; set; }

        public string? TsevetMevatseaName { get; set; }

        // [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        // public TsevetMevatsea? TsevetMevatsea { get; set; }

        public string ProjectName { get; set; }

        public string Teur { get; set; }

        public Maslol Maslol { get; set; }

        public int IdntMaslol { get; set; }

        public bool LogHemsheci { get; set; }

        public int TotalTakzivCoachAdam { get; set; }

        public int TotalTakzivRechesh { get; set; }

        public int CoachAdam { get; set; }

        public string Hearot { get; set; }
        
        public bool Active { get; set; }
         
        public int Year { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}