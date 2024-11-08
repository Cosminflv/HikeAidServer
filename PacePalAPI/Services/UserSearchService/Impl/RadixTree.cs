namespace PacePalAPI.Services.UserSearchService.Impl
{
    public class RadixTree
    {
        private RadixTreeNode root;

        public RadixTree()
        {
            root = new RadixTreeNode();
        }
        // O(k) where k - maximum length of all strings is the set
        public void Insert(string word, UserSearchEntity user)
        {
            RadixTreeNode node = root;
            int i = 0;

            while (i < word.Length)
            {
                char key = word[i];
                bool childExists = node.Children.Keys.Contains(key.ToString());

                if (!childExists)
                {
                    RadixTreeNode newNode = new RadixTreeNode();
                    newNode.Users = new List<int>();
                    newNode.IsEnd = true;
                    newNode.prefix = word.Substring(i);
                    newNode.Users.Add(user.Id);
                    node.Children.Add(key.ToString(), newNode);
                    break;
                }

                node = node.Children[key.ToString()];
                int prefixLen = GetCommonPrefixLength(word.Substring(i), node.prefix);

                i += prefixLen;

                if (prefixLen < node.prefix.Length)
                {
                    RadixTreeNode newChild = new RadixTreeNode();
                    newChild.prefix = node.prefix.Substring(prefixLen);
                    newChild.IsEnd = node.IsEnd;
                    newChild.Children = node.Children;
                    newChild.Users = node.Users;
                    node.prefix = node.prefix.Substring(0, prefixLen);
                    node.Children = new Dictionary<string, RadixTreeNode> { { newChild.prefix[0].ToString(), newChild } };
                    node.IsEnd = i == word.Length;
                }

                // If there are more users with same FirstName / LastName add them to node
                if (word.Length == i && node.prefix.Length == prefixLen)
                {
                    node.Users.Add(user.Id);
                }
            }
        }
        // O(k) where k - maximum length of all strings is the set
        public List<int> SearchByPrefix(string prefix)
        {
            RadixTreeNode node = root;
            int i = 0;

            // Traverse the tree to match the given prefix
            while (i < prefix.Length)
            {
                bool found = false;

                // Iterate through the children to find a matching prefix
                foreach (var childKey in node.Children.Keys)
                {
                    RadixTreeNode childNode = node.Children[childKey];
                    int matchLength = GetCommonPrefixLength(prefix.Substring(i), childNode.prefix);

                    // If there's a match, move down the tree
                    if (matchLength > 0)
                    {
                        // Move to the matching node
                        node = childNode;
                        i += matchLength;
                        found = true;
                        break;
                    }
                }

                // If no matching child is found, return an empty list (prefix doesn't exist)
                if (!found)
                {
                    return new List<int>();
                }
            }

            // At this point, we've matched the entire prefix. Collect all users from this node down.
            return CollectAllUsersIds(node);
        }

        // Helper function to collect all users starting from a given node
        private List<int> CollectAllUsersIds(RadixTreeNode node)
        {
            List<int> result = new List<int>();

            if (node.IsEnd)
                result.AddRange(node.Users);

            foreach (var child in node.Children.Values)
            {
                result.AddRange(CollectAllUsersIds(child));
            }

            return result;
        }

        // Helper function to get the common prefix length between two strings
        private int GetCommonPrefixLength(string str1, string str2)
        {
            int length = Math.Min(str1.Length, str2.Length);
            for (int i = 0; i < length; i++)
            {
                if (str1[i] != str2[i])
                    return i;
            }
            return length;
        }
    }
}
