namespace ExamBackend.Models
{
    public class GamesDatabaseSettings : IGamesDatabaseSettings {
        public string GamesCollectionName { get; set; }
        public string CharacterCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    public interface IGamesDatabaseSettings{
        string GamesCollectionName { get; set; }
        string CharacterCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}