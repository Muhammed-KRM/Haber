using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface IReviewService
{
    Task<List<ReviewDto>> GetByListingAsync(Guid listingId);
    Task<ReviewDto> CreateAsync(ReviewCreateDto dto, Guid reviewerId);
    Task ApproveReviewAsync(Guid reviewId);
}
