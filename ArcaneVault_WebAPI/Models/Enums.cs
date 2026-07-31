namespace ArcaneVault_WebAPI.Models
{
    /// <summary>
    /// Condition grading for collectibles, following common collector terminology.
    /// </summary>
    public enum ItemCondition
    {
        Mint = 1,
        NearMint = 2,
        Excellent = 3,
        Good = 4,
        Fair = 5,
        Poor = 6
    }

    /// <summary>
    /// Moderation state for a catalogue item submitted by a user.
    /// Only Approved items are visible in the public shop.
    /// </summary>
    public enum SubmissionStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum TradeOfferStatus
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2,
        Cancelled = 3
    }

    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    /// <summary>
    /// Whether an item in a trade offer is being given by the proposer
    /// or requested from the recipient.
    /// </summary>
    public enum TradeItemDirection
    {
        Offered = 0,
        Requested = 1
    }
}
