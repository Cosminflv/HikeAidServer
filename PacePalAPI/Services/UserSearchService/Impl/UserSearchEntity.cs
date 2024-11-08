namespace PacePalAPI.Services.UserSearchService.Impl
{
    public class UserSearchEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<int> Friends { get; set; }

        // Function to get mutual friends count
        public int GetMutualFriends(int otherUserId, Func<int, List<int>> getFriendsById)
        {
            // Retrieve the friend's list of friends by their ID
            List<int> otherFriends = getFriendsById(otherUserId);

            // Return the count of mutual friends
            return Friends.Intersect(otherFriends).Count();
        }
    }
}
