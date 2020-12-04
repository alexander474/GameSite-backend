using Microsoft.AspNetCore.Http;

namespace ExamBackend.Models.Game {

    public class GameFileInputModel {
        public IFormFile File { get; set;}
        public string GameId { get; set;}
    }
}