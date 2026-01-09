using BusinessLayer.Interfaces;
using DataBaseLayer.DbContexts;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using ModelLayer.DTOs;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly FundooNotesDbContext _context;
    private readonly ICollaboratorService _collaboratorService;

    public NoteService(
        INoteRepository noteRepository,
        FundooNotesDbContext context,
        ICollaboratorService collaboratorService)
    {
        _noteRepository = noteRepository;
        _context = context;
        _collaboratorService = collaboratorService;
    }

    public async Task CreateAsync(CreateNoteRequestDto request, int userId)
    {
        var note = new Note
        {
            Title = request.Title,
            Description = request.Description,
            Color = request.Color,
            IsPinned = request.IsPinned,
            IsArchived = request.IsArchived,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _noteRepository.AddAsync(note);

        _context.NoteHistories.Add(new NoteHistory
        {
            NoteId = note.NoteId,
            Action = "Created"
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<NoteResponseDto>> GetAllAsync(int userId)
    {
        var notes = await _noteRepository.GetAllIncludingCollaborationsAsync(userId);

        return notes.Select(n => new NoteResponseDto
        {
            NoteId = n.NoteId,
            Title = n.Title,
            Description = n.Description,
            Color = n.Color,
            IsPinned = n.IsPinned,
            IsArchived = n.IsArchived,
            IsDeleted = n.IsDeleted,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt
        }).ToList();
    }

    public async Task UpdateAsync(int noteId, UpdateNoteRequestDto request, int userId)
    {
        // 1️⃣ Try owner access
        var note = await _noteRepository.GetByIdAsync(noteId, userId);

        // 2️⃣ If not owner → check collaborator permission
        if (note == null)
        {
            var canEdit = await _collaboratorService.CanEditAsync(noteId, userId);
            if (!canEdit)
                throw new UnauthorizedAccessException("No edit permission");

            // 3️⃣ Fetch note without owner restriction
            note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new Exception("Note not found");
        }

        // 4️⃣ Apply updates
        if (request.Title != null) note.Title = request.Title;
        if (request.Description != null) note.Description = request.Description;
        if (request.Color != null) note.Color = request.Color;
        if (request.IsPinned.HasValue) note.IsPinned = request.IsPinned.Value;
        if (request.IsArchived.HasValue) note.IsArchived = request.IsArchived.Value;

        note.UpdatedAt = DateTime.UtcNow;

        await _noteRepository.UpdateAsync(note);

        _context.NoteHistories.Add(new NoteHistory
        {
            NoteId = note.NoteId,
            Action = "Updated"
        });

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int noteId, int userId)
    {
        // Only owner can delete (no collaborator delete)
        var note = await _noteRepository.GetByIdAsync(noteId, userId)
            ?? throw new Exception("Note not found");

        note.IsDeleted = true;
        note.UpdatedAt = DateTime.UtcNow;

        await _noteRepository.UpdateAsync(note);

        _context.NoteHistories.Add(new NoteHistory
        {
            NoteId = note.NoteId,
            Action = "Deleted"
        });

        await _context.SaveChangesAsync();
    }
}
