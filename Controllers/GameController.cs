using System;
using ExamBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamBackend.Models.Character;
using ExamBackend.Models.Game;

namespace ExamBackend.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase {
        
        private readonly GameService _gameService;

        private readonly CharacterService _characterService;


        public GameController(GameService gameService, CharacterService characterService) {
            _gameService = gameService;
            _characterService = characterService;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<List<Game>>> Get([FromQuery]string search) {
            var games = await _gameService.Get();
            if (string.IsNullOrEmpty(search)) return games;
            
            
            var returnList = new List<Game>();
            if (!string.IsNullOrEmpty(search)) {
                var matchedGames = games.FindAll(g => {
                    var match = false;
                    if (!string.IsNullOrEmpty(g.Name)) {
                        if (g.Name.ToUpper().Contains(search.ToUpper())) match = true;
                    }
                    if (!string.IsNullOrEmpty(g.Category)) {
                        if (g.Category.ToUpper().Contains(search.ToUpper())) match = true;
                    }

                    return match;
                });
                foreach (var g in matchedGames) { returnList.Add(g); }
            }


            return Ok(returnList.Distinct().ToList());
        }

        [HttpGet("{gameId:length(24)}", Name = "GetGame")]
        [Produces("application/json")]
        public async Task<ActionResult<Game>> GetById(string gameId){
            var foundGame = await _gameService.Get(gameId);

            if( foundGame == null){
                return NotFound();
            }

            return Ok(foundGame);
        }

        [HttpPut("{gameId:length(24)}")]
        public async Task<IActionResult> Put(string gameId, Game game){
            var foundGame = await _gameService.Get(gameId);

            if( foundGame == null){
                return NotFound();
            }

            await _gameService.Update( gameId, game );

            return NoContent();
        }
        
        [HttpDelete("{gameId:length(24)}")]
        public async Task<IActionResult> Delete(string gameId){
            var game = await _gameService.Get(gameId);

            if (game == null) {
                return NotFound();
            }
            
            RemoveFromGame(game.CharacterIds, game);

            await _gameService.Remove(game.GameId);

            return NoContent();
        }
        
        private async Task<List<String>> ValidateCharacter(List<String> characterIds) {
            var clearedCharacters = new List<string>();

            if (characterIds == null || characterIds.Count <= 0) return clearedCharacters;
            
            foreach (var characterId in characterIds) {
                var character = await _characterService.Get(characterId); // Get character
                if (character != null) {
                    clearedCharacters.Add(character.CharacterId);
                }
            }

            return clearedCharacters;
        }
        
        private async void AddToGame(List<String> characterIds, Game game) {
            if (characterIds == null || characterIds.Count <= 0) return;
            
            foreach (var characterId in characterIds) {
                await _characterService.AddGame(characterId, game);
            }
        }
        
        private async void RemoveFromGame(List<String> characterIds, Game game) {
            if (characterIds == null || characterIds.Count <= 0) return;
            
            foreach (var characterId in characterIds) {
                await _characterService.RemoveCharacter(characterId, game);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Game>> Create(Game game) {
            var clearedCharacterIds = await ValidateCharacter(game.CharacterIds);
            game.CharacterIds = clearedCharacterIds;
            
            var createdGame = await _gameService.Create(game);

            
            if (createdGame == null) {
                return BadRequest();
            }
            
            AddToGame(game.CharacterIds, game);
            
            return CreatedAtRoute("GetGame", new { gameId = game.GameId }, game);
        }
        
        [HttpGet("{gameId:length(24)}/images/zip", Name = "GetGameImage")]
        [Produces("application/zip")]
        public IActionResult GetImage(string gameId) {
            var data = _gameService.GetImageFiles(gameId);
            if (data == null) {
                return NoContent();
            }

            return File(data.Content, "application/zip", data.FileName);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] GameFileInputModel file){
            var foundGame = await _gameService.Get(file.GameId); // Needs to be a game that can use image

            if( foundGame == null){
                return BadRequest(); // Dont add a image that would not be used
            }

            foundGame.Images.Add(_gameService.CreateImageFile(file));
            await _gameService.Update(foundGame.GameId, foundGame);
            return CreatedAtRoute("GetGameImage", new { gameId = file.GameId }, foundGame);
        }
        
        [HttpDelete("{gameId:length(24)}/image/{fileName:length(24)}")]
        public async Task<IActionResult> Delete(string gameId, string fileName){
            var game = await _gameService.Get(gameId);

            if (game == null) {
                return NotFound();
            }

            _gameService.DeleteImage(gameId, fileName);

            return NoContent();
        }

    }
}