namespace Contact.API.IntegrationEvents.Event
{
    public class UserProfileChangedEvent
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Avatar { get; set; }
    }
}