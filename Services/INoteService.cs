using NoteApp.Models;

namespace NoteApp.Services
{
    public interface INoteService
    {
        List<Note> GetAll();
        Note GetById(string id);
        void Create(Note note);
        void Update(string id, Note note);
        void Delete(string id);
    }
}