using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kyrs1.Models;

namespace Kyrs1
{
    public class database
    {
        private string connectionString = Constants.Connect;

        public async Task CreateNoteAsync(NotesInputModel noteModel)
        {
            using var con = new NpgsqlConnection(connectionString);
            await con.OpenAsync();
            var sql = "INSERT INTO notes (title, content) VALUES (@title, @content)";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@title", noteModel.Title);
            cmd.Parameters.AddWithValue("@content", noteModel.Content);
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            var notes = new List<Note>();
            using var con = new NpgsqlConnection(connectionString);
            await con.OpenAsync();
            var sql = "SELECT id, title, content FROM notes";
            using var cmd = new NpgsqlCommand(sql, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notes.Add(new Note { NoteNumber = reader.GetInt32(0), Title = reader.GetString(1), Content = reader.GetString(2) });
            }
            await con.CloseAsync();
            return notes;
        }

        public async Task UpdateNoteAsync(NotesInputModel noteModel)
        {
            using var con = new NpgsqlConnection(connectionString);
            await con.OpenAsync();
            var sql = "UPDATE notes SET title = @title, content = @content WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", noteModel.NoteNumber);
            cmd.Parameters.AddWithValue("@title", noteModel.Title);
            cmd.Parameters.AddWithValue("@content", noteModel.Content);
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }

        public async Task DeleteNoteAsync(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            await con.OpenAsync();
            var sql = "DELETE FROM notes WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }
    }
}
