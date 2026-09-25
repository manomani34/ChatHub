using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Messages.Dtos;
using ChatHub.Domain.Entities;

namespace ChatHub.Application.Messages;

public class MessageReactionService
    : IMessageReactionService
{
    private readonly IMessageReactionRepository
        _reactionRepository;

    public MessageReactionService(
        IMessageReactionRepository reactionRepository)
    {
        _reactionRepository =
            reactionRepository;
    }

    /* =========================================================
       Get Reactions
       ========================================================= */

    public async Task<List<MessageReactionDto>>
        GetByMessageIdAsync(
            int messageId,
            int currentUserId)
    {
        if (
            messageId <= 0 ||
            currentUserId <= 0)
        {
            return new List<MessageReactionDto>();
        }

        var reactions =
            await _reactionRepository
                .GetByMessageIdAsync(
                    messageId);

        return reactions
            .GroupBy(x => x.Emoji)
            .Select(group => new MessageReactionDto
            {
                Emoji =
                    group.Key,

                Count =
                    group.Count(),

                ReactedByCurrentUser =
                    group.Any(
                        x =>
                            x.UserId ==
                            currentUserId)
            })
            .OrderBy(x => x.Emoji)
            .ToList();
    }

    /* =========================================================
       Toggle Reaction
       ========================================================= */

    public async Task<List<MessageReactionDto>>
        ToggleAsync(
            ToggleMessageReactionDto request,
            int currentUserId)
    {
        if (
            request is null ||
            request.MessageId <= 0 ||
            currentUserId <= 0 ||
            string.IsNullOrWhiteSpace(
                request.Emoji))
        {
            return new List<MessageReactionDto>();
        }

        var emoji =
            request.Emoji.Trim();

        var existing =
            await _reactionRepository.GetAsync(
                request.MessageId,
                currentUserId,
                emoji);

        if (existing is null)
        {
            var reaction =
                new MessageReaction
                {
                    MessageId =
                        request.MessageId,

                    UserId =
                        currentUserId,

                    Emoji =
                        emoji
                };

            await _reactionRepository.AddAsync(
                reaction);
        }
        else
        {
            await _reactionRepository.RemoveAsync(
                existing);
        }

        return await GetByMessageIdAsync(
            request.MessageId,
            currentUserId);
    }
}