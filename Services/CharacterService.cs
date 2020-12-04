using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ExamBackend.Models;
using ExamBackend.Models.Character;
using ExamBackend.Models.Game;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;

namespace ExamBackend.Services {
    public class CharacterService {
        private readonly IMongoCollection<Character> _characters;
        private readonly IWebHostEnvironment _hosting;

        public CharacterService(IGamesDatabaseSettings settings, IWebHostEnvironment hosting){
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _hosting = hosting;
            _characters = database.GetCollection<Character>(settings.CharacterCollectionName);
        }
        
        public async Task<List<Character>> Get(){
            return await _characters.Find( c => true ).ToListAsync();
        }

        public async Task<Character> Get(string characterId){
            return await _characters.Find( c => c.CharacterId == characterId ).SingleOrDefaultAsync();
        }
        

        public async Task<Character> Create(Character character) {
            await _characters.InsertOneAsync(character);
            return character;
        }

        public async Task Update(string characterId, Character character){
            await _characters.ReplaceOneAsync( c => c.CharacterId == characterId, character);
        }
        
        public async Task Remove(string characterId){
            DeleteImages(characterId); // Delete all images before deleting game
            await _characters.DeleteOneAsync( c => c.CharacterId == characterId);
        }
        
        public async Task AddGame(String characterId, Game game) {

            if (characterId != null && game != null) {
                var character = await Get(characterId); // Get character
                if (character != null) {
                    if (character.GameIds.Contains(game.GameId)) {
                        return;
                    }
                    character.GameIds.Add(game.GameId);
                    await Update(characterId, character);
                }
            }
            
        }
        
        public async Task RemoveCharacter(String characterId, Game game) {

            if (characterId != null && game != null) {
                var character = await Get(characterId); // Get character
                if (character != null) {
                    if (character.GameIds.Contains(game.GameId)) {
                        character.GameIds.Remove(game.GameId);
                        await Update(characterId, character);
                    }
                }
            }
            
        }

        private void DeleteImages(string characterId) {
            if (characterId == null) {
                return;
            }
            
            string wwwrootPath = _hosting.WebRootPath;
            string characterFolderPath = Path.Combine(wwwrootPath, "images", "games", characterId );

            if (Directory.Exists(characterFolderPath)) {
                string[] files = Directory.GetFiles(characterFolderPath);
                foreach (var file in files) {
                    DeleteImage(characterId, file);
                }
                // Delete all images
                Directory.Delete(characterFolderPath);
            }
        }
        
        public void DeleteImage(string characterId, string fileName) {
            if (characterId == null || fileName == null) {
                return;
            }
            
            string wwwrootPath = _hosting.WebRootPath;
            string imagePath = Path.Combine(wwwrootPath, "images", "characters", characterId, fileName );

            if (File.Exists(imagePath)) {
                // Delete one image
                File.Delete(imagePath);
            }
        }

        public GameFileModel GetImageFiles(String characterId) {
            string wwwrootPath = _hosting.WebRootPath;
            string gameFolderPath = Path.Combine(wwwrootPath, "images", "characters", characterId );
            // Check that Directory with images exists
            if (Directory.Exists(gameFolderPath)) {
                string[] files = Directory.GetFiles(gameFolderPath);
                using (var ms = new MemoryStream()) {
                    using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, false)) {
                        foreach (string imagePath in files) {
                            // Add each file that exists
                            if (File.Exists(imagePath)) {
                                // Create entry in zip folder
                                var zipEntry = archive.CreateEntry(Path.GetFileName(imagePath), CompressionLevel.Fastest);
                                using (var originalFileStream = new MemoryStream(File.ReadAllBytes(imagePath)))
                                using (var zipStream = zipEntry.Open()) {
                                    originalFileStream.CopyTo(zipStream);
                                }
                            }
                        }
                    }

                    return new GameFileModel(ms.ToArray(), characterId + ".zip");
                }
            }

            return null;
        }

        public string CreateImageFile(CharacterFileInputModel file) {
            string wwwrootPath = _hosting.WebRootPath;
            string gameFolderPath = Path.Combine(wwwrootPath, "images", "characters", file.CharacterId );
            if(!Directory.Exists(gameFolderPath)){
                Directory.CreateDirectory(gameFolderPath);
            }
            string absolutePath = Path.Combine( gameFolderPath, file.File.FileName );
            using(var fileStream = new FileStream(absolutePath, FileMode.Create)){
                file.File.CopyTo( fileStream );
            }

            return Path.Combine("images", "characters", file.CharacterId, file.File.FileName);
        }
    }
}