namespace PacePalAPI.Services.UserSearchService.Impl
{
    public class RadixTreeNode
    {
        public string prefix;
        public Dictionary<string, RadixTreeNode> Children { get; set; }
        public List<int> Users { get; set; }
        public bool IsEnd { get; set; }

        public RadixTreeNode()
        {
            prefix = "";
            Children = new Dictionary<string, RadixTreeNode>();
            Users = new List<int>();
            IsEnd = false;
        }
    }
}
