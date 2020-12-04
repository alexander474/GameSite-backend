
namespace ExamBackend.Models.Game {
    public class GameFileModel {
        public string FileName { get; set; }
        public byte[] Content { get; set; }

        public GameFileModel(byte[] content, string fileName) {
            FileName = fileName;
            Content = content;
        }
    }
}