using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Ratings.Dtos
{
    public class CustomerOrderRatingAvailabilityDto
    {
        public int OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public bool CanRateDriver { get; set; }
        public bool HasDriverRating { get; set; }
        public bool DriverRatingOpenedBecauseDelivered { get; set; }
        public bool DriverRatingOpenedBecauseDriverCancelled { get; set; }

        public CustomerOrderRatingDriverInfoDto? Driver { get; set; }
        public List<CustomerOrderStoreRatingAvailabilityItemDto> Stores { get; set; } = new();
    }

    public class CustomerOrderRatingDriverInfoDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }

    public class CustomerOrderStoreRatingAvailabilityItemDto
    {
        public int StoreId { get; set; }
        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;

        public OrderStoreStatus StoreOrderStatus { get; set; }
        public bool CanRate { get; set; }
        public bool HasRating { get; set; }
    }
}