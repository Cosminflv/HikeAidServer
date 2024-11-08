namespace PacePalAPI.Services.UserSearchService
{
    public interface IUserSearchService
    {
        public List<(int userId, int commonFriendsNum)> SearchUsers(string input, (int id, String city, String country) searcher);
    }
}
