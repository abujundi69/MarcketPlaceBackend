namespace MarcketPlace.Application.Customer.Favorites.Dtos
{
    public class CustomerFavoriteToggleResultDto
    {
        public int ProductId { get; set; }
        public bool IsFavorite { get; set; }
    }
}