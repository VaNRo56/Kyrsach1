using Microsoft.AspNetCore.Mvc;
using Kyrs1.Models;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using System;


namespace Kyrs1.Controllers;

[ApiController]
[Route("[controller]")]
public class CryptoGet : ControllerBase
{

    private readonly database _db;

    public CryptoGet()
    {
        _db = new database();
    }
    private readonly HttpClient client = new HttpClient();
    public static Dictionary<int, Note> _notes = new Dictionary<int, Note>();


    [HttpGet]
    public async Task<IActionResult> GetNews()
    {
        try
        {
            const string API_KEY = "1d6c5564f66b3b98d410f64ec4c548a4";
            const string URL = $"https://gnews.io/api/v4/search?q=example&lang=en&country=us&max=10&apikey={API_KEY}";

            HttpResponseMessage response = await client.GetAsync(URL);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve news");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ApiResponse>(responseBody);

            return Ok(data.Articles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetAllNotes()
    {
        var notes = await _db.GetAllNotesAsync();
        return Ok(notes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] NotesInputModel input)
    {
        await _db.CreateNoteAsync(input);
        return Ok("Note created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] NotesInputModel input)
    {
        input.NoteNumber = id; // Убедитесь, что ID из URL записывается в модель
        await _db.UpdateNoteAsync(input);
        return Ok("Note updated successfully");
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        await _db.DeleteNoteAsync(id);
        return Ok("Note deleted successfully");
    }
}