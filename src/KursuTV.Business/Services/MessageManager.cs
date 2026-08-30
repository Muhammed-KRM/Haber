using FluentValidation;
using Ganss.Xss;
using MassTransit;
using KursuTV.Business.DTOs;
using KursuTV.Business.Events;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class MessageManager : IMessageService
{
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // HATA KODLARI â€” MessageManager (Prefix: MM)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private const string EC_INBOX       = "MM-001"; // GetInboxAsync
    private const string EC_CONVO       = "MM-002"; // GetConversationAsync
    private const string EC_SEND        = "MM-003"; // SendMessageAsync
    private const string EC_UNLOCK      = "MM-004"; // UnlockMessageAsync
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private readonly IRepository<Message> _messageRepo;
    private readonly ITokenService _tokenService;
    private readonly IValidator<MessageSendDto> _sendValidator;
    private readonly ISettingService _settingService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogService _logService;

    public MessageManager(
        IRepository<Message> messageRepo,
        ITokenService tokenService,
        IValidator<MessageSendDto> sendValidator,
        ISettingService settingService,
        IPublishEndpoint publishEndpoint,
        ILogService logService)
    {
        _messageRepo = messageRepo;
        _tokenService = tokenService;
        _sendValidator = sendValidator;
        _settingService = settingService;
        _publishEndpoint = publishEndpoint;
        _logService = logService;
    }

    public async Task<List<MessageDto>> GetInboxAsync(Guid userId)
    {
        try
        {
            var messages = await _messageRepo.FindAsync(m => m.ReceiverId == userId);
            return messages.OrderByDescending(m => m.CreatedAt).Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_INBOX, ex, userId, userId);
            throw;
        }
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId)
    {
        try
        {
            var messages = await _messageRepo.FindAsync(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId));
            return messages.OrderBy(m => m.CreatedAt).Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_CONVO, ex, new { userId, otherUserId }, userId);
            throw;
        }
    }

    /// <summary>
    /// Mesaj gÃ¶nderme â€” Ä°ki senaryo desteklenir:
    /// Senaryo A (Normal): Ä°lan Ã¼zerinden Ã¼cretsiz mesaj. Ä°lan sahibi jeton harcayarak aÃ§ar.
    /// Senaryo B (Direkt Teklif): GÃ¶nderen 1 jeton harcar, alÄ±cÄ± mesajÄ± zaten aÃ§Ä±k olarak gÃ¶rÃ¼r.
    /// </summary>
    public async Task<MessageDto> SendMessageAsync(MessageSendDto dto, Guid senderId)
    {
        try
        {
        var validationResult = await _sendValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new BusinessException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var sanitizer = new HtmlSanitizer();
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            ListingId = dto.ListingId,
            Content = sanitizer.Sanitize(dto.Content),
            IsInitiatedWithToken = dto.IsDirectOffer,
        };

        if (dto.IsDirectOffer)
        {
            var cost = await _settingService.GetIntSettingAsync("DirectOfferCost", 2);
            await _tokenService.SpendTokenAsync(senderId, cost, "Direkt teklif mesajÄ± gÃ¶nderildi");
            message.Status = MessageStatus.Unlocked;
        }
        else
        {
            message.Status = MessageStatus.Locked;
        }

        await _messageRepo.AddAsync(message);
        await _messageRepo.SaveChangesAsync();

        // AlÄ±cÄ±ya bildirim gÃ¶nder
        try
        {
            await _publishEndpoint.Publish(new SendNotificationEvent
            {
                UserId = dto.ReceiverId,
                Type = "MessageReceived",
                Title = "Yeni MesajÄ±nÄ±z Var ğŸ’¬",
                Message = dto.IsDirectOffer
                    ? "Birileri size direkt teklif gÃ¶nderdi."
                    : "Ä°lanÄ±nÄ±za yeni bir mesaj geldi.",
                ActionUrl = "/panel/mesajlarim",
                SendEmail = true,
                IdempotencyKey = $"msg-{message.Id}"
            });
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync("MSG-NOTIF", ex, new { messageId = message.Id });
        }

        return MapToDto(message);
        }
        catch (BusinessException) { throw; }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_SEND, ex, dto, senderId);
            throw;
        }
    }

    /// <summary>
    /// Ä°lan sahibi kilitli mesajÄ± 1 jeton harcayarak aÃ§ar (Senaryo A).
    /// </summary>
    public async Task UnlockMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
        var message = (await _messageRepo.FindAsync(m => m.Id == messageId)).FirstOrDefault()
            ?? throw new NotFoundException("Mesaj", messageId);

        if (message.ReceiverId != userId)
            throw new UnauthorizedException("Bu mesajÄ± aÃ§ma yetkiniz yok.");

        if (message.Status == MessageStatus.Unlocked)
            throw new BusinessException("Bu mesaj zaten aÃ§Ä±lmÄ±ÅŸ.");

        var previouslyUnlocked = await _messageRepo.FindAsync(m =>
            m.SenderId == message.SenderId &&
            m.ReceiverId == userId &&
            m.Status == MessageStatus.Unlocked);

        if (!previouslyUnlocked.Any())
        {
            var cost = await _settingService.GetIntSettingAsync("MessageUnlockCost", 1);
            await _tokenService.SpendTokenAsync(userId, cost, "Mesaj kilidi aÃ§Ä±ldÄ±");
        }

        message.Status = MessageStatus.Unlocked;
        message.ReadAt = DateTime.UtcNow;
        _messageRepo.Update(message);
        await _messageRepo.SaveChangesAsync();
        }
        catch (BusinessException) { throw; }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_UNLOCK, ex, new { messageId }, userId);
            throw;
        }
    }

    private static MessageDto MapToDto(Message m) => new()
    {
        Id = m.Id,
        SenderId = m.SenderId,
        SenderName = m.Sender?.FullName ?? "",
        SenderImageUrl = m.Sender?.ProfileImageUrl,
        ReceiverId = m.ReceiverId,
        ListingId = m.ListingId,
        ListingTitle = m.Listing?.Title,
        Content = m.Content,
        IsInitiatedWithToken = m.IsInitiatedWithToken,
        IsUnlocked = m.Status == MessageStatus.Unlocked,
        Status = m.Status,
        CreatedAt = m.CreatedAt,
        ReadAt = m.ReadAt
    };
}
