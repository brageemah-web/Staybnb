namespace staybnb.Models
{
    public enum PropertyType {
        Apartment, 
        House, 
        Villa, 
        Cabin, 
        Cottage, 
        Other
    }
    public enum BookingStatus{
        Pending,
        Approved,
        Rejected,
        Canceled,
        CheckedIn
    }

    public enum PaymentStatus { 
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public enum ActivityType {
        Login, 
        Logout, 
        Registration, 
        RoleChange,
        HostApplicationSubmitted, 
        HostApplicationReviewed,
        PropertyCreated, 
        PropertyUpdated, 
        PropertyStatusToggled,
        BookingCreated, 
        BookingStatusUpdated,
        CheckInUpdated, 
        DocumentUploaded, 
        DocumentReviewed
    }

    public enum NotificationType {
        Info,
        Success,
        Warning,
        BookingRequested,
        BookingApproved,
        BookingRejected,
        CheckInSubmitted,
        CheckInVerified,
        HostApplicationSubmitted,
        HostApplicationApproved,
        HostApplicationRejected,
        NewMessage
    }

    public enum ApplicationStatus {
        Pending,
        Approved,
        Rejected
    }

    public enum CheckInStatus { 
        NotStarted,
        InProgress,
        Submitted,
        Verified,
        Rejected
    }

    public enum DocumentStatus { 
        Pending, 
        Rejected,
        Approved
    }
}
