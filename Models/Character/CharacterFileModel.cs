namespace ExamBackend.Models.Character {
    public class CharacterFileModel {
        public string FileName { get; set; }
        public byte[] Content { get; set; }

        public CharacterFileModel(string fileName, byte[] content) {
            FileName = fileName;
            Content = content;
        }
    }
}