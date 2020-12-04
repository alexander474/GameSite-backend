using System;
using System.Collections;
using ExamBackend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using ExamBackend.Models.Character;
using ExamBackend.Models.Game;

namespace ExamBackend.Services {

    public class GameService {

        private readonly IMongoCollection<Game> _games;
        private readonly IWebHostEnvironment _hosting;
        
        public GameService(IGamesDatabaseSettings settings, IWebHostEnvironment hosting){
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _hosting = hosting;
            _games = database.GetCollection<Game>(settings.GamesCollectionName);
        }

        public async Task<List<Game>> Get(){
            return await _games.Find( g => true ).ToListAsync();
        }

        public async Task<Game>  Get(string gameId){
            return await _games.Find( g => g.GameId == gameId ).SingleOrDefaultAsync();
        }

        public async Task<Game> Create(Game game) {
            await _games.InsertOneAsync(game);
            return game;
        }

        public async Task Update(string gameId, Game game){
            await _games.ReplaceOneAsync( g => g.GameId == gameId, game);
        }

        public async Task Remove(string gameId){
            DeleteImages(gameId); // Delete all images before deleting game
            await _games.DeleteOneAsync( g => g.GameId == gameId);
        }
        
        public async Task AddCharacter(String gameId, Character character) {

            if (gameId != null && character != null) {
                var game = await Get(gameId); // Get character
                if (game != null) {
                    if (game.CharacterIds.Contains(character.CharacterId)) {
                        return;
                    }
                    game.CharacterIds.Add(character.CharacterId);
                    await Update(gameId, game);
                }
            }
            
        }
        
        public async Task RemoveCharacter(String gameId, Character character) {

            if (gameId != null && character != null) {
                var game = await Get(gameId); // Get character
                if (game != null) {
                    if (game.CharacterIds.Contains(character.CharacterId)) {
                        game.CharacterIds.Remove(character.CharacterId);
                        await Update(gameId, game);
                    }
                }
            }
            
        }

        private void DeleteImages(string gameId) {
            if (gameId == null) {
                return;
            }
            
            string wwwrootPath = _hosting.WebRootPath;
            string gameFolderPath = Path.Combine(wwwrootPath, "images", "games", gameId );

            if (Directory.Exists(gameFolderPath)) {
                string[] files = Directory.GetFiles(gameFolderPath);
                foreach (var file in files) {
                    DeleteImage(gameId, file);
                }
                // Delete all images
                Directory.Delete(gameFolderPath);
            }
        }
        
        public void DeleteImage(string gameId, string fileName) {
            if (gameId == null || fileName == null) {
                return;
            }
            
            string wwwrootPath = _hosting.WebRootPath;
            string imagePath = Path.Combine(wwwrootPath, "images", "games", gameId, fileName );

            if (File.Exists(imagePath)) {
                // Delete one image
                File.Delete(imagePath);
            }
        }

        public GameFileModel GetImageFiles(String gameId) {
            string wwwrootPath = _hosting.WebRootPath;
            string gameFolderPath = Path.Combine(wwwrootPath, "images", "games", gameId );
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

                    return new GameFileModel(ms.ToArray(), gameId + ".zip");
                }
            }

            return null;
        }

        public string CreateImageFile(GameFileInputModel file) {
            string wwwrootPath = _hosting.WebRootPath;
            string gameFolderPath = Path.Combine(wwwrootPath, "images", "games", file.GameId );
            if(!Directory.Exists(gameFolderPath)){
                Directory.CreateDirectory(gameFolderPath);
            }
            string absolutePath = Path.Combine( gameFolderPath, file.File.FileName );
            using(var fileStream = new FileStream(absolutePath, FileMode.Create)){
                file.File.CopyTo( fileStream );
            }

            return Path.Combine("images", "games", file.GameId, file.File.FileName);
        }

    }
}