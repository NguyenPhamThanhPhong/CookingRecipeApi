namespace CookingRecipeApi.RequestsResponses.Requests.NotificationRequestsDTO
{
    public class NotificationCreateRequest
    {
        public IEnumerable<string> userIds { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string redirectPath { get; set; }
    }
}
