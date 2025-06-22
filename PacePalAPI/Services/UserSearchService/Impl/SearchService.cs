using PacePalAPI.Models;
using PacePalAPI.Models.Enums;

namespace PacePalAPI.Services.UserSearchService.Impl
{
    public class UserSearchService : IUserSearchService
    {
        // Hash tables for quick lookup
        private Dictionary<string, List<int>> _cityToUserIds = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<int>> _countryToUserIds = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<int, UserSearchEntity> _usersById = new Dictionary<int, UserSearchEntity>();

        // Dictionary to map friendships by user ID
        private Dictionary<int, List<int>> _friendships = new Dictionary<int, List<int>>();

        private RadixTree _usernamesRadixTree;
        private RadixTree _firstnameRadixTree;
        private RadixTree _lastnameRadixTree;
        private RadixTree _fullNameRadixTree;

        public UserSearchService(
            RadixTree usernamesRadixTree,
            RadixTree firstnameRadixTree,
            RadixTree lastnameRadixTree,
            RadixTree fullNameRadixTree,
            List<UserModel> users)
        {
            _usernamesRadixTree = usernamesRadixTree;
            _firstnameRadixTree = firstnameRadixTree;
            _lastnameRadixTree = lastnameRadixTree;
            _fullNameRadixTree = fullNameRadixTree;

            foreach (var user in users)
            {
                // Combine the Sent and Received Friendships into a single list
                var combinedFriendships = user.SentFriendships
                    .Concat(user.ReceivedFriendships)
                    .ToList();

                var userSearchEntity = new UserSearchEntity
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    City = user.City,
                    Country = user.Country,
                    Friends = combinedFriendships
                        .Where(f => f.Status == EFriendshipState.Accepted)
                        .Select(f => f.ReceiverId == user.Id ? f.RequesterId : f.ReceiverId)
                        .ToList()
                };
                AddUser(userSearchEntity);
            }
        }

        // Insert user data into the Radix Tree
        public void AddUser(UserSearchEntity user)
        {
            _usernamesRadixTree.Insert(user.Username, user);
            _firstnameRadixTree.Insert(user.FirstName, user);
            _lastnameRadixTree.Insert(user.LastName, user);
            _fullNameRadixTree.Insert($"{user.FirstName} {user.LastName}", user);

            // Add user to the city hash table
            if (!_cityToUserIds.ContainsKey(user.City))
            {
                _cityToUserIds[user.City] = new List<int>();
            }
            _cityToUserIds[user.City].Add(user.Id);

            // Add user to the country hash table
            if (!_countryToUserIds.ContainsKey(user.Country))
            {
                _countryToUserIds[user.Country] = new List<int>();
            }
            _countryToUserIds[user.Country].Add(user.Id);

            // Initialize an empty friends list in the friendships dictionary
            if (!_friendships.ContainsKey(user.Id))
            {
                _friendships[user.Id] = new List<int>();
            }

            _usersById[user.Id] = user; // Cache the user entity by ID

            foreach (var friendId in user.Friends)
            {
                AddFriendship(user.Id, friendId);
            }
        }

        // Method to add a friendship between two users by their IDs
        private void AddFriendship(int userId, int friendId)
        {
            // Ensure both users have entries in the friendships dictionary
            if (!_friendships.ContainsKey(userId))
            {
                _friendships[userId] = new List<int>();
            }
            if (!_friendships.ContainsKey(friendId))
            {
                _friendships[friendId] = new List<int>();
            }

            // Add each user to the other's friends list if not already present
            if (!_friendships[userId].Contains(friendId))
            {
                _friendships[userId].Add(friendId);
            }
            if (!_friendships[friendId].Contains(userId))
            {
                _friendships[friendId].Add(userId);
            }
        }

        // Method to get a user's friends by their ID
        public List<int> GetFriends(int userId)
        {
            return _friendships.ContainsKey(userId) ? _friendships[userId] : new List<int>();
        }

        // Search and rank users by prefix
        public List<(int userId, int commonFriendsNum)> SearchUsers(string input, (int id, String city, String country) searcher)
        {
            List<int> userIds = _usernamesRadixTree.SearchByPrefix(input);
            if (userIds.Count == 0) userIds = _firstnameRadixTree.SearchByPrefix(input);
            if (userIds.Count == 0) userIds = _lastnameRadixTree.SearchByPrefix(input);
            if (userIds.Count == 0) userIds = _fullNameRadixTree.SearchByPrefix(input);

            return RankUsers(userIds, searcher);
        }

        // Helper method to rank users by mutual friends, city, and country O(nlogn)
        // First int is the user id, and second one is mutual friends count
        private List<(int, int)> RankUsers(IEnumerable<int> usersIds, (int, String, String) searcher)
        {
            // Step 0: Remove duplicates
            var distinctUserIds = usersIds.Distinct();

            // Step 1: Cache mutual friends count for efficiency
            var mutualFriendsCount = distinctUserIds.ToDictionary(
                id => id,
                id => _usersById[id].GetMutualFriends(searcher.Item1, GetFriends)
                );

            // Step 2: Group users by city and country using dictionaries
            var cityMatches = _cityToUserIds.ContainsKey(searcher.Item2)
                ? new HashSet<int>(_cityToUserIds[searcher.Item2])
                : new HashSet<int>();

            var countryMatches = _countryToUserIds.ContainsKey(searcher.Item3)
                ? new HashSet<int>(_countryToUserIds[searcher.Item3])
                : new HashSet<int>();

            // Step 3: Sort by mutual friends, then by city, then by country
            var result = distinctUserIds
                .OrderByDescending(id => mutualFriendsCount[id])                   // Primary: mutual friends count
                .ThenByDescending(id => cityMatches.Contains(id) ? 1 : 0)          // Secondary: city match
                .ThenByDescending(id => countryMatches.Contains(id) ? 1 : 0)       // Tertiary: country match
                .ToList();

            return result.Select(id => (id, mutualFriendsCount[id])).ToList();
        }
    }
}
