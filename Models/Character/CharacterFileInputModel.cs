using Microsoft.AspNetCore.Http;

namespace ExamBackend.Models.Character {
    public class CharacterFileInputModel {
        public IFormFile File { get; set;}
        public string CharacterId { get; set;}
    }
}