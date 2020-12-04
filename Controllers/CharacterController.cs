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
    public class CharacterController : ControllerBase {
        
        private readonly CharacterService _characterService;
        private readonly GameService _gameService;


        public CharacterController(CharacterService characterService, GameService gameService) {
            _characterService = characterService;
            _gameService = gameService;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<List<Character>>> Get([FromQuery]string search) {
            var characters = await _characterService.Get();
            if (string.IsNullOrEmpty(search)) return characters;
            
            var returnList = new List<Character>();
            if (!string.IsNullOrEmpty(search)) {
                var matchedCharacters = characters.FindAll(g => {
                    var match = false;
                    if (!string.IsNullOrEmpty(g.Name)) {
                        if (g.Name.ToUpper().Contains(search.ToUpper())) match = true;
                    }

                    return match;
                });
                foreach (var c in matchedCharacters) { returnList.Add(c); }
            }


            return Ok(returnList.Distinct().ToList());
        }

        [HttpGet("{characterId:length(24)}", Name = "GetCharacter")]
        [Produces("application/json")]
        public async Task<ActionResult<Character>> GetById(string characterId){
            var foundCharacter = await _characterService.Get(characterId);

            if( foundCharacter == null){
                return NotFound();
            }

            return Ok(foundCharacter);
        }

        [HttpPut("{characterId:length(24)}")]
        public async Task<IActionResult> Put(string characterId, Character character){
            var foundCharacter = await _characterService.Get(characterId);

            if( foundCharacter == null){
                return NotFound();
            }

            await _characterService.Update( characterId, character );

            return NoContent();
        }
        
        [HttpDelete("{characterId:length(24)}")]
        public async Task<IActionResult> Delete(string characterId){
            var foundCharacter = await _characterService.Get(characterId);

            if( foundCharacter == null){
                return NotFound();
            }
            
            RemoveFromGame(foundCharacter.GameIds, foundCharacter);

            await _characterService.Remove(foundCharacter.CharacterId);

            return NoContent();
        }
        
        private async Task<List<String>> ValidateGame(List<String> gameIds) {
            var clearedGames = new List<string>();

            if (gameIds == null || gameIds.Count <= 0) return clearedGames;
            
            foreach (var gameId in gameIds) {
                var game = await _gameService.Get(gameId); // Get character
                if (game != null) {
                    clearedGames.Add(game.GameId);
                }
            }

            return clearedGames;
        }
        
        private async void AddToGame(List<String> gameIds, Character character) {
            if (gameIds == null || gameIds.Count <= 0) return;
            
            foreach (var gameId in gameIds) {
                await _gameService.AddCharacter(gameId, character);
            }
        }
        
        private async void RemoveFromGame(List<String> gameIds, Character character) {
            if (gameIds == null || gameIds.Count <= 0) return;
            
            foreach (var gameId in gameIds) {
                await _gameService.RemoveCharacter(gameId, character);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Character>> Create(Character character) {
            var clearedGameIds = await ValidateGame(character.GameIds);
            character.GameIds = clearedGameIds;
            var foundCharacter = await _characterService.Create(character);
            
            if (foundCharacter == null) {
                return BadRequest();
            }
            
            AddToGame(character.GameIds, character);

            return CreatedAtRoute("GetCharacter", new { characterId = character.CharacterId }, character);
        }
        
        [HttpGet("{characterId:length(24)}/images/zip", Name = "GetCharacterImage")]
        [Produces("application/zip")]
        public IActionResult GetImage(string characterId) {
            var data = _characterService.GetImageFiles(characterId);
            if (data == null) {
                return NoContent();
            }

            return File(data.Content, "application/zip", data.FileName);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] CharacterFileInputModel file){
            var foundCharacter = await _characterService.Get(file.CharacterId); // Needs to be a game that can use image

            if( foundCharacter == null){
                return BadRequest(); // Dont add a image that would not be used
            }

            foundCharacter.Images.Add(_characterService.CreateImageFile(file));
            await _characterService.Update(foundCharacter.CharacterId, foundCharacter);
            return CreatedAtRoute("GetCharacterImage", new { CharacterId = file.CharacterId }, foundCharacter);
        }
        
        [HttpDelete("{characterId:length(24)}/image/{fileName:length(24)}")]
        public async Task<IActionResult> Delete(string characterId, string fileName){
            var character = await _characterService.Get(characterId);

            if (character == null) {
                return NotFound();
            }

            _characterService.DeleteImage(characterId, fileName);
            
            return NoContent();
        }

    }
}